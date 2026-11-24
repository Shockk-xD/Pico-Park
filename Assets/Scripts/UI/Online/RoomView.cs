using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

[RequireComponent(typeof(PhotonView))]
public class RoomView : MonoBehaviour {
    [GUIColor(0, 1, 0)]
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] private TMP_Text _playerCountText;
    [SerializeField] private TMP_Text _roomIdText;
    [SerializeField] private LevelsContainer _levelsContainer;

    [FoldoutGroup("LevelButtons")]
    [SerializeField] private Button _nextLevelButton;
    [FoldoutGroup("LevelButtons")]
    [SerializeField] private Button _previousLevelButton;

    [FoldoutGroup("LevelImages")]
    [SerializeField] private Image _currentLevelImage;
    [FoldoutGroup("LevelImages")]
    [SerializeField] private Image _previousLevelImage;
    [FoldoutGroup("LevelImages")]
    [SerializeField] private Image _nextLevelImage;

    [Space()]
    [SerializeField] private TMP_Text _currentLevelText;
    [SerializeField] private Button _startGameButton;

    private Room CurrentRoom => PhotonNetwork.CurrentRoom;
    private List<LevelData> Levels => _levelsContainer.Levels;
    private int _currentLevel = 0;
    private bool _isStarting;
    private bool _isLoadingScene;
    private PhotonView _photonView;
    private SceneLoader _sceneLoader;

    [Inject]
    private void Construct(SceneLoader sceneLoader) {
        _sceneLoader = sceneLoader;
    }

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable() {
        _isStarting = false;
        _isLoadingScene = false;
        GameLocalization.Changed += RefreshLocalizedText;
        RefreshLocalizedText();
        _nextLevelButton.onClick.AddListener(SetNextLevel);
        _previousLevelButton.onClick.AddListener(SetPreviousLevel);
        _startGameButton.onClick.AddListener(StartGameButtonClicked);
        _roomManager.OnPlayerEntered += OnPlayerEnteredRoom;
        _roomManager.OnPlayerLeft += OnPlayerLeftRoom;

        SetCurrentLevelView(_currentLevel);
    }

    private void OnDisable() {
        GameLocalization.Changed -= RefreshLocalizedText;
        _nextLevelButton.onClick.RemoveListener(SetNextLevel);
        _previousLevelButton.onClick.RemoveListener(SetPreviousLevel);
        _startGameButton.onClick.RemoveListener(StartGameButtonClicked);
        _roomManager.OnPlayerEntered -= OnPlayerEnteredRoom;
        _roomManager.OnPlayerLeft -= OnPlayerLeftRoom;
    }

    private void RefreshLocalizedText() {
        SetCurrentLevelText(_currentLevel);
        if (CurrentRoom == null) {
            return;
        }

        _roomIdText.text = GameLocalization.Get("lobby.roomCodeValue", CurrentRoom.Name) ?? CurrentRoom.Name;
        UpdatePlayerCountText();
    }

    private void OnPlayerEnteredRoom(Player newPlayer) {
        UpdatePlayerCountText();

        if (PhotonNetwork.IsMasterClient) {
            _photonView.RPC(nameof(SetCurrentLevelView), newPlayer, _currentLevel);
        }
    }

    private void OnPlayerLeftRoom(Player otherPlayer) {
        UpdatePlayerCountText();
    }

    private void UpdatePlayerCountText() {
        if (CurrentRoom == null) {
            return;
        }

        _playerCountText.text = GameLocalization.Get("lobby.roomPlayers", CurrentRoom.PlayerCount, CurrentRoom.MaxPlayers)
            ?? $"{CurrentRoom.PlayerCount} / {CurrentRoom.MaxPlayers}";
    }

    private void SetNextLevel() {
        if (!PhotonNetwork.IsMasterClient || _isStarting) {
            return;
        }

        var maxLevel = Levels.Count - 1;
        _currentLevel = MathfExtensions.NumberLoop(_currentLevel + 1, 0, maxLevel);
        _photonView.RPC(nameof(SetCurrentLevelView), RpcTarget.All, _currentLevel);
    }

    private void SetPreviousLevel() {
        if (!PhotonNetwork.IsMasterClient || _isStarting) {
            return;
        }

        var maxLevel = Levels.Count - 1;
        _currentLevel = MathfExtensions.NumberLoop(_currentLevel - 1, 0, maxLevel);
        _photonView.RPC(nameof(SetCurrentLevelView), RpcTarget.All, _currentLevel);
    }

    [PunRPC]
    private void SetCurrentLevelView(int currentLevel) {
        _currentLevel = currentLevel;
        SetLevelPreviews(currentLevel);
        SetCurrentLevelText(currentLevel);
    }

    private void SetLevelPreviews(int currentLevel) {
        var maxLevel = Levels.Count - 1;
        var previousLevelId = MathfExtensions.NumberLoop(currentLevel - 1, 0, maxLevel);
        var nextLevelId = MathfExtensions.NumberLoop(currentLevel + 1, 0, maxLevel);

        _currentLevelImage.sprite = Levels[currentLevel].LevelPreview;
        _previousLevelImage.sprite = Levels[previousLevelId].LevelPreview;
        _nextLevelImage.sprite = Levels[nextLevelId].LevelPreview;
    }

    private void SetCurrentLevelText(int currentLevel) {
        var levelId = Levels[currentLevel].LevelId;
        _currentLevelText.text = GameLocalization.Get("lobby.level", levelId) ?? levelId.ToString("000");
    }

    private void StartGameButtonClicked() {
        if (!PhotonNetwork.IsMasterClient || CurrentRoom == null || _isStarting) {
            return;
        }

        if (CurrentRoom.PlayerCount != CurrentRoom.MaxPlayers) {
            NotEnoughPlayersToStartAnimation();
            return;
        }

        var sceneId = Levels[_currentLevel].SceneId;
        if (sceneId < 0 || sceneId >= SceneManager.sceneCountInBuildSettings) {
            GameAudio.Play(GameAudio.Cue.Error);
            Debug.LogWarning($"Scene for level {Levels[_currentLevel].LevelId} is missing from build settings.");
            return;
        }

        _isStarting = true;
        _roomManager.CloseRoomForMatch();
        _photonView.RPC(nameof(StartGame), RpcTarget.AllViaServer, sceneId);
    }

    [PunRPC]
    private void StartGame(int sceneId, PhotonMessageInfo messageInfo) {
        if (_isLoadingScene || !messageInfo.Sender.IsMasterClient) {
            return;
        }

        _isStarting = true;
        _isLoadingScene = true;
        _sceneLoader.LoadScene(sceneId, pauseNetworkMessages: true);
    }

    private void NotEnoughPlayersToStartAnimation() {
        GameAudio.Play(GameAudio.Cue.Error);
        const float animationDuration = 0.5f;
        var originColor = new Color(37 / 100, 37 / 100, 37 / 100); 

        _playerCountText.transform.DOShakePosition(animationDuration, strength: 5);
        _playerCountText.DOColor(Color.red, animationDuration / 2).OnComplete(() => {
            _playerCountText.DOColor(originColor, animationDuration / 2);
        });
    }
}
