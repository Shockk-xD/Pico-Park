using UnityEngine;
using UnityEngine.UI;

public class RoomSettingsPage : BasePage
{
    [Header("Buttons")]
    [SerializeField] private Button _returnButton;

    [Header("Pages")]
    [SerializeField] private BasePage _pageToReturn;

    private void OnEnable() {
        _returnButton.onClick.AddListener(Return);
    }

    private void OnDisable() {
        _returnButton.onClick.RemoveListener(Return);
    }

    private void Return() {
        _pageToReturn.ShowPage();
        HidePage();
    }
}
