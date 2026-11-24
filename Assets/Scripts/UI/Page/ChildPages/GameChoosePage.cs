using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class GameChoosePage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _chooseOnlineButton;
    [SerializeField] private Button _chooseLocalButton;
    [SerializeField] private Button _returnButton;

    [Header("Pages")]
    [SerializeField] private BasePage _mainMenuPage;
    [SerializeField] private RoomSettingsPage _onlineLobbySettingsPage;
    [SerializeField] private MainMenuButtonsPage _pageToReturn;

    private void OnEnable() {
        _chooseOnlineButton.onClick.AddListener(OpenOnlineLobbySettingsPage);
        _chooseLocalButton.onClick.AddListener(StartLocalGame);
        _returnButton.onClick.AddListener(Return);
    }

    private void OnDisable() {
        _returnButton.onClick.RemoveListener(Return);
        _chooseOnlineButton.onClick.RemoveListener(OpenOnlineLobbySettingsPage);
        _chooseLocalButton.onClick.RemoveListener(StartLocalGame);
    }

    private void OpenOnlineLobbySettingsPage() {
        RoomManager.EnsureOnlineConnection();
        _mainMenuPage.HidePage();
        _onlineLobbySettingsPage.ShowPage();
    }

    private void StartLocalGame() {
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/Levels/Level1.unity");
        if (sceneIndex < 0 || LocalGame.IsActive) {
            return;
        }

        LocalGame.Begin();
        GetComponent<CanvasGroup>().interactable = false;
        ProjectContext.Instance.Container.Resolve<SceneLoader>().LoadScene(sceneIndex);
    }

    private void Return() {
        _pageToReturn.ShowPage();
        HidePage();
    }
}
