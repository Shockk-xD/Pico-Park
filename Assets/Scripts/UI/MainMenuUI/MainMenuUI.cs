using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private BasePage _mainMenuPage;
    [SerializeField] private RoomSettingsPage _roomSettingsPage;
    [SerializeField] private RoomsListPage _roomsPage;
    [SerializeField] private RoomPage _roomPage;

    [SerializeField] private TMP_Text _playerNicknameText;

    private void Awake() {
        LocalGame.End();
        _playerNicknameText.text = PlayerNicknameStorageService.PlayerNickname ?? string.Empty;
    }

    public void OnReturnedToRoom() {
        _mainMenuPage.gameObject.SetActive(false);
        _roomPage.ShowPage();
    }

    public void RefreshRoomState() {
        _roomPage.SetRoomState();
    }

    public void OnJoinedRoomHandler() {
        _roomSettingsPage.HidePage();
        _roomsPage.HidePage();
        _roomPage.ShowPage();
    }
}
