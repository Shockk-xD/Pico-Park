using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class NicknameRegistration : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private TMP_Text _inputYourNicknameText;
    [SerializeField] private Button _registerButton;

    private SceneLoader _sceneLoader;
    private const int _mainMenuSceneIndex = 1;

    [Inject]
    private void Construct(SceneLoader sceneLoader) {
        _sceneLoader = sceneLoader;
    }

    private void OnEnable() {
        _nicknameInputField.onEndEdit.AddListener(OnNicknameEndEditHandler);
        _registerButton.onClick.AddListener(RegisterNickname);
    }

    private void OnDisable() {
        _nicknameInputField.onEndEdit.RemoveListener(OnNicknameEndEditHandler);
        _registerButton.onClick.RemoveListener(RegisterNickname);
    }

    private void OnNicknameEndEditHandler(string nickname) {
        _nicknameInputField.text = nickname.Trim();
    }

    private void RegisterNickname() {
        var nickname = _nicknameInputField.text;

        if (string.IsNullOrEmpty(nickname)) {
            InvalidNicknameAnimation();
            return;
        }


        SaveNickname(nickname);
        _sceneLoader.LoadScene(_mainMenuSceneIndex);
    }

    private void SaveNickname(string nickname) {
        PlayerNicknameStorageService.SavePlayerNickname(nickname);
    }

    private async void InvalidNicknameAnimation() {
        var animationTime = 0.2f;
        var animationCount = 2;

        Color defaultColor = _inputYourNicknameText.color;

        for (int i = 0; i < animationCount; i++) {
            var tween = _inputYourNicknameText.DOColor(Color.red, animationTime);
            await tween.AsyncWaitForCompletion();

            var backwardTween = _inputYourNicknameText.DOColor(defaultColor, animationTime);
            await backwardTween.AsyncWaitForCompletion();
        }
    }
}
