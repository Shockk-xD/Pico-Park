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

    private void Awake() {
        _maxPlayersCount = _roomPreferences.MinPlayerCount;
        SetPlayersCountText();
    }

    private void OnEnable() {
        _playerNickname = PlayerNicknameStorageService.PlayerNickname;
        SetRoomDefaultName();
        _createRoomButton.interactable = true;
        _roomPreferences.IsHost = true;

        _roomNameInputField.onEndEdit.AddListener(OnRoomNameEndEditHandler);
        _increasePlayersCountButton.onClick.AddListener(IncreasePlayersCount);
        _decreasePlayersCountButton.onClick.AddListener(DecreasePlayersCount);
        _createRoomButton.onClick.AddListener(CreateRoom);
    }

    private void OnDisable() {
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
        var defaultName = _roomPreferences.DefaultName;
        var roomName = string.Format(defaultName, _playerNickname);
        _roomNameInputField.text = roomName;
    }

    private void SetPlayersCountText() {
        var prefix = "Кол-во игроков: ";
        _playersCountText.text = prefix + _maxPlayersCount;
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
        _createRoomButton.interactable = false;

        RoomOptions roomOptions = new() {
            IsVisible = !_isPrivateGameToggle.isOn,
            IsOpen = true,
            MaxPlayers = _maxPlayersCount
        };

        _roomManager.CreateRoom(_roomNameInputField.text, roomOptions);
    }
}
