using UnityEngine;
using UnityEngine.UI;

public class RoomPage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _returnButton;

    [Header("Pages")]
    [SerializeField] private BasePage _pageToReturn;

    [Space()]
    [SerializeField] private RoomManager _roomManager;
    [SerializeField] private GameObject _hostState;
    [SerializeField] private GameObject _guestState;
    [SerializeField] private RoomPreferences _roomPreferences;

    private void OnEnable() {
        _returnButton.onClick.AddListener(Return);
        SetRoomState();
    }

    private void OnDisable() {
        _returnButton.onClick.RemoveListener(Return);
    }

    private void Return() {
        _roomManager.LeaveRoom();
        _pageToReturn.ShowPage();
        HidePage();
    }

    private void SetRoomState() {
        var isHost = _roomPreferences.IsHost;

        if (isHost) {
            SetHostState();
        } else {
            SetGuestState();
        }
    }

    private void SetHostState() {
        _hostState.SetActive(true);
        _guestState.SetActive(false);
    }

    private void SetGuestState() {
        _guestState.SetActive(true);
        _hostState.SetActive(false);
    }
}
