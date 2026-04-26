using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HighlightingAnimation : MonoBehaviour {
    [SerializeField] private float _highlightingTime = 0.3f;

    private CanvasGroup _canvasGroup;
    private Sequence _animationSequence;

    private void Awake() {
        _canvasGroup = GetComponent<CanvasGroup>();
        SetupAnimation();
    }

    private void SetupAnimation() {
        _animationSequence = DOTween.Sequence()
            .Append(_canvasGroup.DOFade(1, _highlightingTime))
            .Append(_canvasGroup.DOFade(0, _highlightingTime))
            .Append(_canvasGroup.DOFade(1, _highlightingTime))
            .SetLoops(-1).SetEase(Ease.Linear);
    }

    private void OnEnable() {
        _animationSequence.Restart();
    }

    private void OnDisable() {
        _animationSequence.Kill();
    }
}
