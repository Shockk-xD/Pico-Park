using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BasePage : MonoBehaviour
{
    [SerializeField] protected float _fadeDuration = 0.25f;

    private CanvasGroup _canvasGroup;

    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public async void HidePage() {
        if (_canvasGroup == null) {
            return;
        }

        _canvasGroup.interactable = false;
        await _canvasGroup.DOFade(0, _fadeDuration).From(1).AsyncWaitForCompletion();
        gameObject.SetActive(false);
    }

    public async void ShowPage() {
        gameObject.SetActive(true);
        await _canvasGroup.DOFade(1, _fadeDuration).From(0).AsyncWaitForCompletion();
        _canvasGroup.interactable = true;
    }
}
