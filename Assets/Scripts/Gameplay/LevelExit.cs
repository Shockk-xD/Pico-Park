using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelExit : MonoBehaviourPunCallbacks, IOnEventCallback {
    private const byte KeyPickedUpEvent = 51;
    private const byte PlayerExitedEvent = 52;
    private const byte AdvanceLevelEvent = 53;
    private const byte RestartLevelEvent = 54;
    private const string _levelRunProperty = "levelRun";
    private const string LobbyScenePath = "Assets/Scenes/MainMenu.unity";

    [SerializeField] private LevelKey _key;
    [SerializeField] private GameObject _closedDoorSprite;
    [SerializeField] private GameObject _openedDoorSprite;
    [SerializeField] private LevelCamera _levelCamera;
    [SerializeField] private LevelsContainer _levelsContainer;

    private readonly HashSet<int> _finishedPlayers = new();
    private int _sceneIndex;
    private int _levelRun;
    private bool _isOpen;
    private bool _keyRequested;
    private bool _exitRequested;
    private bool _advanceRequested;
    private bool _isLoading;
    private int _localFinishedPlayers;
    private TextMeshPro _exitCounter;

    public bool IsTransitioning => _isLoading || _advanceRequested;

    public bool HasPlayerFinished(int actorNumber) {
        return _finishedPlayers.Contains(actorNumber);
    }

    private void Awake() {
        _sceneIndex = gameObject.scene.buildIndex;
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(_levelRunProperty, out var value)
            && value is int run) {
            _levelRun = run;
        }
        _closedDoorSprite.SetActive(true);
        _openedDoorSprite.SetActive(false);
    }

    public void PickUpKey() {
        if (_isOpen || _keyRequested) {
            return;
        }

        if (!PhotonNetwork.InRoom) {
            OpenDoor();
            return;
        }

        _keyRequested = SendEvent(KeyPickedUpEvent);
    }

    private void OpenDoor() {
        _isOpen = true;
        _key.gameObject.SetActive(false);
        _closedDoorSprite.SetActive(false);
        _openedDoorSprite.SetActive(true);
        GameAudio.Play(GameAudio.Cue.KeyPickup);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (GamePauseMenu.IsLeaving || IsTransitioning || _exitRequested
            || !other.TryGetComponent<PlayerMovement>(out var player) || !player.IsLocalPlayer || !player.isActiveAndEnabled) {
            return;
        }

        if (!_isOpen) {
            if (_key.Throwable != null && _key.Throwable.IsCarriedBy(player)) {
                PickUpKey();
            }
            return;
        }

        if (PhotonNetwork.InRoom) {
            _exitRequested = SendEvent(PlayerExitedEvent, player.GetComponent<PhotonView>().ViewID);
        } else {
            GameAudio.Play(GameAudio.Cue.PlayerExit);
            player.gameObject.SetActive(false);
            Destroy(player.gameObject);
            _localFinishedPlayers++;
            UpdateExitCounter();
            if (_localFinishedPlayers == (LocalGame.IsActive ? LocalGame.PlayerCount : 1)) {
                LoadNextScene(FindNextScene());
            }
        }
    }

    public void OnEvent(EventData photonEvent) {
        if (GamePauseMenu.IsLeaving || _isLoading || photonEvent.Code < KeyPickedUpEvent || photonEvent.Code > RestartLevelEvent
            || photonEvent.CustomData is not int[] data || data.Length != 3 || data[0] != _sceneIndex
            || data[2] != _levelRun) {
            return;
        }

        switch (photonEvent.Code) {
            case KeyPickedUpEvent:
                if (!_isOpen) {
                    OpenDoor();
                }
                break;
            case PlayerExitedEvent:
                FinishPlayer(photonEvent.Sender, data[1]);
                break;
            case AdvanceLevelEvent:
                if (photonEvent.Sender == PhotonNetwork.MasterClient?.ActorNumber) {
                    LoadNextScene(data[1]);
                }
                break;
            case RestartLevelEvent:
                if (photonEvent.Sender == PhotonNetwork.MasterClient?.ActorNumber
                    && RoomManager.IsRoomCreator(photonEvent.Sender)) {
                    RestartScene();
                }
                break;
        }
    }

    public bool RestartLevel() {
        if (IsTransitioning || _sceneIndex < 0 || !RoomManager.CanRestartLevel) {
            return false;
        }

        if (!PhotonNetwork.InRoom) {
            RestartScene();
            return true;
        }

        var properties = new Hashtable { { _levelRunProperty, _levelRun + 1 } };
        if (!PhotonNetwork.CurrentRoom.SetCustomProperties(properties)) {
            return false;
        }

        PhotonNetwork.DestroyAll();
        _advanceRequested = SendEvent(RestartLevelEvent);
        return _advanceRequested;
    }

    private void RestartScene() {
        _isLoading = true;
        var sceneLoader = ProjectContext.Instance.Container.Resolve<SceneLoader>();
        sceneLoader.LoadScene(_sceneIndex, pauseNetworkMessages: PhotonNetwork.InRoom);
    }

    private void FinishPlayer(int actorNumber, int viewId) {
        if (!_isOpen || viewId <= 0 || viewId / PhotonNetwork.MAX_VIEW_IDS != actorNumber
            || !_finishedPlayers.Add(actorNumber)) {
            return;
        }

        var view = PhotonView.Find(viewId);
        GameAudio.Play(GameAudio.Cue.PlayerExit);
        if (actorNumber == PhotonNetwork.LocalPlayer.ActorNumber) {
            _levelCamera.Spectate();
            if (view != null) {
                view.gameObject.SetActive(false);
                PhotonNetwork.Destroy(view.gameObject);
            }
        } else if (view != null) {
            view.gameObject.SetActive(false);
        }

        UpdateExitCounter();
        TryAdvanceLevel();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        UpdateExitCounter();
        TryAdvanceLevel();
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        TryAdvanceLevel();
    }

    private void UpdateExitCounter() {
        if (!_isOpen || _isLoading) {
            return;
        }

        var finished = _localFinishedPlayers;
        var total = LocalGame.IsActive ? LocalGame.PlayerCount : 1;
        if (PhotonNetwork.InRoom) {
            finished = 0;
            total = 0;
            foreach (var player in PhotonNetwork.PlayerList) {
                if (!player.IsInactive) {
                    total++;
                    if (HasPlayerFinished(player.ActorNumber)) {
                        finished++;
                    }
                }
            }
        }

        if (finished == 0) {
            if (_exitCounter != null) {
                _exitCounter.gameObject.SetActive(false);
            }
            return;
        }

        if (_exitCounter == null) {
            var sprite = _openedDoorSprite.GetComponentInChildren<SpriteRenderer>();
            var bounds = sprite != null ? sprite.bounds : GetComponent<BoxCollider2D>().bounds;
            var position = new Vector3(bounds.center.x, bounds.max.y + 0.3f, transform.position.z);
            _exitCounter = WorldLabel.Create(transform, sprite, "Exit Counter", position, 3.5f);
        }

        _exitCounter.text = $"{finished}/{total}";
        _exitCounter.gameObject.SetActive(true);
    }

    private void TryAdvanceLevel() {
        if (GamePauseMenu.IsLeaving || !_isOpen || IsTransitioning || !PhotonNetwork.IsMasterClient) {
            return;
        }

        foreach (var player in PhotonNetwork.PlayerList) {
            if (!player.IsInactive && !_finishedPlayers.Contains(player.ActorNumber)) {
                return;
            }
        }

        _advanceRequested = SendEvent(AdvanceLevelEvent, FindNextScene());
    }

    private int FindNextScene() {
        var levels = _levelsContainer.Levels;
        var currentIndex = levels.FindIndex(level => level != null && level.SceneId == _sceneIndex);
        if (currentIndex >= 0) {
            for (var i = currentIndex + 1; i < levels.Count; i++) {
                var level = levels[i];
                if (level != null && level.SceneId > 0 && level.SceneId < SceneManager.sceneCountInBuildSettings
                    && level.SceneId != _sceneIndex) {
                    return level.SceneId;
                }
            }
        }

        return SceneUtility.GetBuildIndexByScenePath(LobbyScenePath);
    }

    private void LoadNextScene(int sceneIndex) {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings || sceneIndex == _sceneIndex) {
            Debug.LogError($"Cannot load scene with build index {sceneIndex}.", this);
            return;
        }

        _isLoading = true;
        GameAudio.Play(GameAudio.Cue.LevelClear);
        if (sceneIndex == SceneUtility.GetBuildIndexByScenePath(LobbyScenePath)) {
            GamePauseMenu.ShowCompletion();
            return;
        }

        var sceneLoader = ProjectContext.Instance.Container.Resolve<SceneLoader>();
        sceneLoader.LoadScene(sceneIndex, pauseNetworkMessages: PhotonNetwork.InRoom);
    }

    private bool SendEvent(byte code, int value = 0) {
        return PhotonNetwork.RaiseEvent(code, new[] { _sceneIndex, value, _levelRun },
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
    }
}
