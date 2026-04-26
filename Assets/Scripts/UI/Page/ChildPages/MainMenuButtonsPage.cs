using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonsPage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;

    [Header("Pages")]
    [SerializeField] private GameChoosePage _gameChoosePage;
    [SerializeField] private BasePage _mainMenuPage;
    [SerializeField] private RoomsListPage _onlineRoomsPage;

    private void OnEnable() {
        _createGameButton.onClick.AddListener(OpenGameChoosePage);
        _joinGameButton.onClick.AddListener(OpenOnlineRoomsPage);
    }

    private void OnDisable() {
        _createGameButton.onClick.RemoveListener(OpenGameChoosePage);
        _joinGameButton.onClick.RemoveListener(OpenOnlineRoomsPage);
    }

    private void OpenGameChoosePage() {
        _gameChoosePage.ShowPage();
        HidePage();
    }

    private void OpenOnlineRoomsPage() {
        _onlineRoomsPage.ShowPage();
        _mainMenuPage.HidePage();
    }
}
