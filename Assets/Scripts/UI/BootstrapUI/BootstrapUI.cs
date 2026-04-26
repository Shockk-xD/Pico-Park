using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BootstrapUI : MonoBehaviour
{
    [SerializeField] private BasePage _connectingPage;
    [SerializeField] private BasePage _userResponsePage;
    [SerializeField] private BasePage _playerNicknameRegistrationPage;
    [SerializeField] private Image _doorImage;
    [SerializeField] private Sprite _openedDoorSprite;

    public void OpenUserResponsePage() {
        _connectingPage.HidePage();
        _userResponsePage.ShowPage();
    }

    public void OpenRegistrationPage() {
        _userResponsePage.HidePage();
        _playerNicknameRegistrationPage.ShowPage();
    }

    public void OpenDoor() {
        _doorImage.sprite = _openedDoorSprite;
    }
}
