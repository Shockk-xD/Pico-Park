using UnityEngine;
using UnityEngine.UI;

public class HideablePageByButton : BasePage
{
    [SerializeField] private Button _hideButton;

    private void OnEnable() {
        _hideButton.onClick.AddListener(HidePage);
    }

    private void OnDisable() {
        _hideButton.onClick.RemoveListener(HidePage);
    }

    private new void HidePage() {
        base.HidePage();
    }
}
