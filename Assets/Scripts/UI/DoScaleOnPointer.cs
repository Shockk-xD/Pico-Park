using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DoScaleOnPointer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler 
{
    [SerializeField] private float _targetScale = 1.05f;
    private const float _timeToScale = 0.1f;

    public void OnPointerEnter(PointerEventData _) {
        transform.DOScale(_targetScale, _timeToScale);
    }

    public void OnPointerExit(PointerEventData _) {
        transform.DOScale(1, _timeToScale);
    }
}
