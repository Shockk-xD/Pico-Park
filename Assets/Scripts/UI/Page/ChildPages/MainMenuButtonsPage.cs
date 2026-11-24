using UnityEngine;
using UnityEngine.UI;

public class MainMenuButtonsPage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _joinGameButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _exitButton;

    [Header("Pages")]
    [SerializeField] private GameChoosePage _gameChoosePage;
    [SerializeField] private BasePage _mainMenuPage;
    [SerializeField] private RoomsListPage _onlineRoomsPage;
    [SerializeField] private SettingsPage _settingsPage;

    private void OnEnable() {
        _createGameButton.onClick.AddListener(OpenGameChoosePage);
        _joinGameButton.onClick.AddListener(OpenOnlineRoomsPage);
        _settingsButton.onClick.AddListener(OpenSettings);
        _exitButton.onClick.AddListener(ExitGame);
    }

    private void OnDisable() {
        _createGameButton.onClick.RemoveListener(OpenGameChoosePage);
        _joinGameButton.onClick.RemoveListener(OpenOnlineRoomsPage);
        _settingsButton.onClick.RemoveListener(OpenSettings);
        _exitButton.onClick.RemoveListener(ExitGame);
        _settingsPage.Close();
    }

    private void OpenGameChoosePage() {
        _gameChoosePage.ShowPage();
        HidePage();
    }

    private void OpenOnlineRoomsPage() {
        _onlineRoomsPage.ShowPage();
        _mainMenuPage.HidePage();
    }

    private void ExitGame() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OpenSettings() {
        _settingsPage.Open(GetComponent<CanvasGroup>());
    }
}
