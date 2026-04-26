using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Zenject;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private BootstrapUI _ui;

    private GameInput _inputSystem;
    private SceneLoader _sceneLoader;   
    private readonly JsonToFileStorageService _storageService = new();
    private const string playerNicknameKey = nameof(playerNicknameKey);
    private const int _mainMenuSceneIndex = 1;

    [Inject]
    private void Construct(GameInput inputSystem, SceneLoader sceneLoader) {
        inputSystem.Enable();
        _inputSystem = inputSystem;
        _sceneLoader = sceneLoader;
    }

    public async void OnConnectedHandler() {
        _ui.OpenUserResponsePage();
        await WaitForPlayerResponse();
    }

    private async UniTask WaitForPlayerResponse() {
        await WaitForTap();
        _ui.OpenDoor();

        if (IsPlayerNicknameSet()) {
            _sceneLoader.LoadScene(_mainMenuSceneIndex);
        } else {
            _ui.OpenRegistrationPage();
        }
    }

    private async UniTask WaitForTap() {
        await UniTask.WaitUntil(_inputSystem.UI.Tap.WasPressedThisFrame);
    }

    private bool IsPlayerNicknameSet() {
        var playerNickname = PlayerNicknameStorageService.PlayerNickname;
        return !string.IsNullOrEmpty(playerNickname);
    }
}
