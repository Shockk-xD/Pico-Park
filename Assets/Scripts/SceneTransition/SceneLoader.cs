using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Image _blockoutImage;
    [SerializeField] private TMP_Text _loadingText;

    private SceneLoadingView _sceneLoadingView;

    private void Awake() {
        _sceneLoadingView = new SceneLoadingView(_blockoutImage, _loadingText);
    }

    public async void LoadScene(int sceneIndex) {
        await _sceneLoadingView.FadeIn();
        await SceneManager.LoadSceneAsync(sceneIndex).ToUniTask();
        await _sceneLoadingView.FadeOut();
    }
}
