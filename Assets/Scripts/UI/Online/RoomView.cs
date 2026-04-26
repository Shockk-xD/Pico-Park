using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private PhotonView _photonView;

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable() {
        Init();
        _nextLevelButton.onClick.AddListener(SetNextLevel);
        _previousLevelButton.onClick.AddListener(SetPreviousLevel);
        _startGameButton.onClick.AddListener(StartGame);
        _roomManager.OnPlayerEntered += OnPlayerEnteredRoom;
        _roomManager.OnPlayerLeft += OnPlayerLeftRoom;

        if (PhotonNetwork.IsMasterClient) {
            SetCurrentLevelView(_currentLevel);
        }
    }

    private void OnDisable() {
        _nextLevelButton.onClick.RemoveListener(SetNextLevel);
        _previousLevelButton.onClick.RemoveListener(SetPreviousLevel);
        _startGameButton.onClick.AddListener(StartGame);
        _roomManager.OnPlayerEntered -= OnPlayerEnteredRoom;
        _roomManager.OnPlayerLeft -= OnPlayerLeftRoom;
    }

    private void Init() {
        if (CurrentRoom == null) {
            return;
        }

        _roomIdText.text = $"Код комнаты: {CurrentRoom.Name}";
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

        _playerCountText.text = $"Игроков зашло: {CurrentRoom.PlayerCount} / {CurrentRoom.MaxPlayers}";
    }

    private void SetNextLevel() {
        var maxLevel = Levels.Count - 1;
        _currentLevel = MathfExtensions.NumberLoop(_currentLevel + 1, 0, maxLevel);
        _photonView.RPC(nameof(SetCurrentLevelView), RpcTarget.All, _currentLevel);
    }

    private void SetPreviousLevel() {
        var maxLevel = Levels.Count - 1;
        _currentLevel = MathfExtensions.NumberLoop(_currentLevel - 1, 0, maxLevel);
        _photonView.RPC(nameof(SetCurrentLevelView), RpcTarget.All, _currentLevel);
    }

    [PunRPC]
    private void SetCurrentLevelView(int currentLevel) {
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
        _currentLevelText.text = "Уровень: " + currentLevel.ToString("000");
    }

    private void StartGame() {
        if (CurrentRoom.PlayerCount > 1) {

        } else {
            NotEnoughPlayersToStartAnimation();
        }
    }

    private void NotEnoughPlayersToStartAnimation() {
        const float animationDuration = 0.5f;
        var originColor = new Color(37 / 100, 37 / 100, 37 / 100); 

        _playerCountText.transform.DOShakePosition(animationDuration, strength: 5);
        _playerCountText.DOColor(Color.red, animationDuration / 2).OnComplete(() => {
            _playerCountText.DOColor(originColor, animationDuration / 2);
        });
    }
}
