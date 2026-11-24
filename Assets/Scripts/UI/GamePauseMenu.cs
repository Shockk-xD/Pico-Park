using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

public class GamePauseMenu : MonoBehaviourPunCallbacks {
    private const string _prefabPath = "GamePauseMenu";
    private const string _mainMenuPath = "Assets/Scenes/MainMenu.unity";

    [SerializeField] private CanvasGroup _panel;
    [SerializeField] private GameObject _eventSystem;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private SettingsPage _settingsPrefab;
    [SerializeField] private GameObject _localControls;
    [SerializeField] private GameObject _onlineControls;
    [SerializeField] private LocalizedText _title;
    [SerializeField] private LocalizedText _continueLabel;

    private static GamePauseMenu _instance;
    private SettingsPage _settingsPage;
    private LevelExit _levelExit;
    private bool _isLeaving;
    private bool _mainMenuLoading;
    private bool _pausedTime;
    private bool _isCompleted;
    private float _previousTimeScale;

    public static bool BlocksGameplay => _instance != null
        && (_instance._panel.gameObject.activeSelf || _instance._isLeaving);
    public static bool IsLeaving => _instance != null && _instance._isLeaving;

    public static void CreateForLevel() {
        if (_instance == null) {
            Instantiate(Resources.Load<GameObject>(_prefabPath));
        }
    }

    public static void ShowCompletion() {
        CreateForLevel();
        _instance.OpenCompletion();
    }

    private void OpenCompletion() {
        _isCompleted = true;
        Continue();
        _title.SetKey("menu.allLevelsCleared");
        _continueLabel.SetKey("menu.backToMainMenu");
        _localControls.SetActive(false);
        _onlineControls.SetActive(false);
        _settingsButton.gameObject.SetActive(false);
        _restartButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
        _panel.gameObject.SetActive(true);
        EventSystem.current?.SetSelectedGameObject(_continueButton.gameObject);
    }

    private void Awake() {
        _instance = this;
        _levelExit = FindObjectOfType<LevelExit>();
        _settingsPage = Instantiate(_settingsPrefab, transform);
        _eventSystem.SetActive(EventSystem.current == null);
        _panel.gameObject.SetActive(false);
        GameAudio.BindButton(_continueButton);
        GameAudio.BindButton(_settingsButton);
        GameAudio.BindButton(_restartButton);
        GameAudio.BindButton(_exitButton);
        _continueButton.onClick.AddListener(OnContinueClicked);
        _settingsButton.onClick.AddListener(OpenSettings);
        _restartButton.onClick.AddListener(Restart);
        _exitButton.onClick.AddListener(ExitToMenu);
    }

    private void Update() {
        if (_isLeaving || (_levelExit != null && _levelExit.IsTransitioning)) {
            return;
        }

        if (Keyboard.current?.escapeKey.wasPressedThisFrame != true) {
            return;
        }

        if (_panel.gameObject.activeSelf) {
            Continue();
        } else {
            Open();
        }
    }

    private void Open() {
        _restartButton.gameObject.SetActive(_levelExit != null && RoomManager.CanRestartLevel);
        _localControls.SetActive(LocalGame.IsActive);
        _onlineControls.SetActive(!LocalGame.IsActive);
        _panel.gameObject.SetActive(true);
        if (!PhotonNetwork.InRoom) {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _pausedTime = true;
        }

        EventSystem.current?.SetSelectedGameObject(_continueButton.gameObject);
    }

    private void Continue() {
        _settingsPage.Close();
        _panel.gameObject.SetActive(false);
        EventSystem.current?.SetSelectedGameObject(null);
        RestoreTime();
    }

    private void OnContinueClicked() {
        if (_isCompleted) {
            ExitToMenu();
        } else {
            Continue();
        }
    }

    private void RestoreTime() {
        if (_pausedTime) {
            Time.timeScale = _previousTimeScale;
            _pausedTime = false;
        }
    }

    private void OpenSettings() {
        _settingsPage.Open(_panel);
    }

    private void Restart() {
        if (_levelExit == null || !RoomManager.CanRestartLevel) {
            return;
        }

        Continue();
        if (!_levelExit.RestartLevel()) {
            GameAudio.Play(GameAudio.Cue.Error);
        }
    }

    private void ExitToMenu() {
        if (_isLeaving || (!_isCompleted && _levelExit != null && _levelExit.IsTransitioning)) {
            return;
        }

        _isLeaving = true;
        if (!PhotonNetwork.InRoom) {
            LoadMainMenu();
        } else if (!PhotonNetwork.LeaveRoom(false)) {
            _isLeaving = false;
            GameAudio.Play(GameAudio.Cue.Error);
        }

        if (_isLeaving) {
            Continue();
        }
    }

    public override void OnLeftRoom() {
        if (_isLeaving) {
            LoadMainMenu();
        }
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause) {
        if (_isLeaving) {
            LoadMainMenu();
        }
    }

    private void LoadMainMenu() {
        if (_mainMenuLoading) {
            return;
        }

        _mainMenuLoading = true;
        var sceneIndex = SceneUtility.GetBuildIndexByScenePath(_mainMenuPath);
        var sceneLoader = ProjectContext.Instance.Container.Resolve<SceneLoader>();
        sceneLoader.LoadScene(sceneIndex);
    }

    private void OnDestroy() {
        if (_instance == this) {
            RestoreTime();
            _instance = null;
        }
    }
}
