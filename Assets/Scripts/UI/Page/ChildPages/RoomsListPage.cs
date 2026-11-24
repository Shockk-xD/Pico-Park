using Photon.Realtime;
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

    private string _joinFailureKey;

    private void OnEnable() {
        GameLocalization.Changed += RefreshJoinFailure;
        _returnButton.onClick.AddListener(Return);
        _joinButton.onClick.AddListener(JoinByCode);
        _roomCodeInput.onEndEdit.AddListener(OnRoomCodeEndEditHandler);
        _createRoomButton.onClick.AddListener(OpenRoomSettingsPage);
    }

    private void OnDisable() {
        GameLocalization.Changed -= RefreshJoinFailure;
        _returnButton.onClick.RemoveListener(Return);
        _joinButton.onClick.RemoveListener(JoinByCode);
        _roomCodeInput.onEndEdit.RemoveListener(OnRoomCodeEndEditHandler);
        _createRoomButton.onClick.RemoveListener(OpenRoomSettingsPage);
    }

    public void NoRoomsStateHandler(int roomCount) {
        var isNoRoomsState = roomCount == 0;
        _noRoomsState.SetActive(isNoRoomsState);
    }

    public void OnJoinRoomFailedHandler(short returnCode) {
        _joinFailureKey = returnCode switch {
            ErrorCode.GameDoesNotExist => "lobby.roomMissing",
            ErrorCode.GameFull => "lobby.roomFull",
            ErrorCode.GameClosed => "lobby.roomClosed",
            _ => "lobby.joinFailed"
        };
        RefreshJoinFailure();
        GameAudio.Play(GameAudio.Cue.Error);
        _joinRoomFailedMessageBox.ShowPage();
    }

    private void RefreshJoinFailure() {
        if (_joinFailureKey != null) {
            _joinRoomFailedText.text = GameLocalization.Get(_joinFailureKey) ?? _joinRoomFailedText.text;
        }
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
        GameAudio.Play(GameAudio.Cue.Error);
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
