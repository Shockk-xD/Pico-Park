using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomSettingsUI : MonoBehaviour
{
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] private TMP_InputField _roomNameInputField;
    [SerializeField] private Toggle _isPrivateGameToggle;
    [SerializeField] private Button _increasePlayersCountButton;
    [SerializeField] private Button _decreasePlayersCountButton;
    [SerializeField] private TMP_Text _playersCountText;
    [SerializeField] private Button _createRoomButton;
    [SerializeField] private RoomPreferences _roomPreferences;

    private string _playerNickname;
    private int _maxPlayersCount;
    private string _defaultRoomName;

    private void Awake() {
        _maxPlayersCount = _roomPreferences.MinPlayerCount;
        SetPlayersCountText();
    }

    private void OnEnable() {
        GameLocalization.Changed += RefreshLocalizedText;
        _playerNickname = PlayerNicknameStorageService.PlayerNickname;
        SetRoomDefaultName();
        SetPlayersCountText();
        _createRoomButton.interactable = true;
        _roomPreferences.IsHost = true;

        _roomNameInputField.onEndEdit.AddListener(OnRoomNameEndEditHandler);
        _increasePlayersCountButton.onClick.AddListener(IncreasePlayersCount);
        _decreasePlayersCountButton.onClick.AddListener(DecreasePlayersCount);
        _createRoomButton.onClick.AddListener(CreateRoom);
    }

    private void OnDisable() {
        GameLocalization.Changed -= RefreshLocalizedText;
        _roomNameInputField.onEndEdit.RemoveListener(OnRoomNameEndEditHandler);
        _increasePlayersCountButton.onClick.RemoveListener(IncreasePlayersCount);
        _decreasePlayersCountButton.onClick.RemoveListener(DecreasePlayersCount);
        _createRoomButton.onClick.RemoveListener(CreateRoom);
    }

    private void OnRoomNameEndEditHandler(string roomName) {
        if (string.IsNullOrEmpty(roomName) || string.IsNullOrWhiteSpace(roomName)) {
            SetRoomDefaultName();
        }
    }

    private void SetRoomDefaultName() {
        _defaultRoomName = GameLocalization.Get("lobby.roomNameDefault", _playerNickname)
            ?? string.Format(_roomPreferences.DefaultName, _playerNickname);
        _roomNameInputField.text = _defaultRoomName;
    }

    private void SetPlayersCountText() {
        _playersCountText.text = GameLocalization.Get("lobby.maxPlayers", _maxPlayersCount)
            ?? _maxPlayersCount.ToString();
    }

    private void RefreshLocalizedText() {
        SetPlayersCountText();
        if (_roomNameInputField.text == _defaultRoomName) {
            SetRoomDefaultName();
        }
    }

    private void IncreasePlayersCount() {
        var minCount = _roomPreferences.MinPlayerCount;
        var maxCount = _roomPreferences.MaxPlayerCount;
        _maxPlayersCount = MathfExtensions.NumberLoop(_maxPlayersCount + 1, minCount, maxCount);
        SetPlayersCountText();
    }

    private void DecreasePlayersCount() {
        var minCount = _roomPreferences.MinPlayerCount;
        var maxCount = _roomPreferences.MaxPlayerCount;
        _maxPlayersCount = MathfExtensions.NumberLoop(_maxPlayersCount - 1, minCount, maxCount);
        SetPlayersCountText();
    }

    private void CreateRoom() {
        RoomOptions roomOptions = new() {
            IsVisible = !_isPrivateGameToggle.isOn,
            IsOpen = true,
            MaxPlayers = _maxPlayersCount
        };

        if (_roomManager.CreateRoom(_roomNameInputField.text, roomOptions)) {
            _createRoomButton.interactable = false;
        } else {
            GameAudio.Play(GameAudio.Cue.Error);
        }
    }
}
