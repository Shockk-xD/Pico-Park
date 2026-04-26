using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomsListPage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _returnButton;
    [SerializeField] private Button _joinButton;
    [SerializeField] private Button _createRoomButton;

    [Header("Pages")]
    [SerializeField] private BasePage _pageToReturn;
    [SerializeField] private HideablePageByButton _joinRoomFailedMessageBox;
    [SerializeField] private RoomSettingsPage _roomSettingsPage;

    [Space()]
    [SerializeField] private GameObject _noRoomsState;
    [SerializeField] private TMP_Text _joinRoomFailedText;
    [SerializeField] private TMP_InputField _roomCodeInput;
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] private TMP_Text _inputRoomCodeText;

    private void OnEnable() {
        _returnButton.onClick.AddListener(Return);
        _joinButton.onClick.AddListener(JoinByCode);
        _roomCodeInput.onEndEdit.AddListener(OnRoomCodeEndEditHandler);
        _createRoomButton.onClick.AddListener(OpenRoomSettingsPage);
    }

    private void OnDisable() {
        _returnButton.onClick.RemoveListener(Return);
        _joinButton.onClick.RemoveListener(JoinByCode);
        _roomCodeInput.onEndEdit.RemoveListener(OnRoomCodeEndEditHandler);
        _createRoomButton.onClick.RemoveListener(OpenRoomSettingsPage);
    }

    public void NoRoomsStateHandler(int roomCount) {
        var isNoRoomsState = roomCount == 0;
        _noRoomsState.SetActive(isNoRoomsState);
    }

    public void OnJoinRoomFailedHandler(string message) {
        _joinRoomFailedMessageBox.ShowPage();
        _joinRoomFailedText.text = $"Не удалось зайти в комнату. Ошибка: {message}";
    }

    private void Return() {
        _pageToReturn.ShowPage();
        HidePage();
    }

    private void OnRoomCodeEndEditHandler(string text) {
        _roomCodeInput.text = _roomCodeInput.text.Trim();
    }

    private void JoinByCode() {
        var roomCode = _roomCodeInput.text;

        if (string.IsNullOrWhiteSpace(roomCode)) {
            InputRoomCodeError();
        } else {
            _roomManager.JoinRoomByCode(roomCode);
        }
    }

    private async void InputRoomCodeError() {
        var animationDuration = 0.3f;
        var startColor = _inputRoomCodeText.color;

        _inputRoomCodeText.transform.DOShakePosition(animationDuration, strength: 2);
        await _inputRoomCodeText.DOColor(Color.red, animationDuration / 2).AsyncWaitForCompletion();
        await _inputRoomCodeText.DOColor(startColor, animationDuration / 2).AsyncWaitForCompletion();
    }

    private void OpenRoomSettingsPage() {
        _roomSettingsPage.ShowPage();
        HidePage();
    }
}
