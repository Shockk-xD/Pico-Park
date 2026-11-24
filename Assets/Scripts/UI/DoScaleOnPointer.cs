using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoScaleOnPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    [SerializeField] private float _targetScale = 1.05f;
    [SerializeField] private bool _playHoverSound;
    private const float _timeToScale = 0.1f;

    public void OnPointerEnter(PointerEventData _) {
        if (_playHoverSound && TryGetComponent<Button>(out var button) && button.IsInteractable()) {
            GameAudio.Play(GameAudio.Cue.Hover);
        }
        transform.DOScale(_targetScale, _timeToScale);
    }

    public void OnPointerExit(PointerEventData _) {
        transform.DOScale(1, _timeToScale);
    }
}
