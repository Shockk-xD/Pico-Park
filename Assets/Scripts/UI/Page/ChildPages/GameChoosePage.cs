using UnityEngine;
using UnityEngine.UI;

public class GameChoosePage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _chooseOnlineButton;
    [SerializeField] private Button _returnButton;

    [Header("Pages")]
    [SerializeField] private BasePage _mainMenuPage;
    [SerializeField] private RoomSettingsPage _onlineLobbySettingsPage;
    [SerializeField] private MainMenuButtonsPage _pageToReturn;

    private void OnEnable() {
        _chooseOnlineButton.onClick.AddListener(OpenOnlineLobbySettingsPage);
        _returnButton.onClick.AddListener(Return);
    }

    private void OnDisable() {
        _returnButton.onClick.RemoveListener(Return);
        _chooseOnlineButton.onClick.RemoveListener(OpenOnlineLobbySettingsPage);
    }

    private void OpenOnlineLobbySettingsPage() {
        _mainMenuPage.HidePage();
        _onlineLobbySettingsPage.ShowPage();
    }

    private void Return() {
        _pageToReturn.ShowPage();
        HidePage();
    }
}
