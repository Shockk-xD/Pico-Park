using Cysharp.Threading.Tasks;
using Photon.Pun;
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

    public async void LoadScene(int sceneIndex, bool pauseNetworkMessages = false) {
        if (pauseNetworkMessages) {
            PhotonNetwork.IsMessageQueueRunning = false;
        }

        try {
            await _sceneLoadingView.FadeIn();
            await SceneManager.LoadSceneAsync(sceneIndex).ToUniTask();
        } finally {
            if (pauseNetworkMessages) {
                PhotonNetwork.IsMessageQueueRunning = true;
            }
        }

        await _sceneLoadingView.FadeOut();
    }
}
