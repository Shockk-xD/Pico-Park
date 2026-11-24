using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour {
    [SerializeField] private string _key;
    [SerializeField] private TMP_Text _label;

    public void SetKey(string key) {
        _key = key;
        Refresh();
    }

    private void OnEnable() {
        GameLocalization.Changed += Refresh;
        Refresh();
    }

    private void OnDisable() {
        GameLocalization.Changed -= Refresh;
    }

    private void Refresh() {
        var text = GameLocalization.Get(_key);
        if (text != null) {
            _label.text = text;
        }
    }
}
