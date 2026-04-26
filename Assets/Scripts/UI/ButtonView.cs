using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private GameObject _buttonDefault;
    [SerializeField] private GameObject _buttonPressed;

    private Button _button;

    private void Awake() {
        _button = GetComponent<Button>();
    }

    private void OnEnable() {
        SetButtonDefault();
    }

    public void OnPointerDown(PointerEventData _) {
        if (_button.interactable) {
            SetButtonPressed();
        }
    }

    public void OnPointerUp(PointerEventData _) {
        SetButtonDefault();
    }

    private void SetButtonDefault() {
        _buttonDefault.SetActive(true);
        _buttonPressed.SetActive(false);
    }

    private void SetButtonPressed() {
        _buttonDefault.SetActive(false);
        _buttonPressed.SetActive(true);
    }
}
