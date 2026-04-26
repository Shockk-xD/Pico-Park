using DG.Tweening;
using TMPro;
using UnityEngine;

public class MessageView : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageText;

    public void ShowMessage(string message, string sender) {
        var prefix = $"{sender}: ";
        _messageText.text = prefix;

        var doTextDuration = GetDoTextDuration(message);
        _messageText.DOText(prefix + message, doTextDuration).SetEase(Ease.OutCubic);
    }

    private float GetDoTextDuration(string message) {
        float maxDuration = 1;
        int lengthBarrier = 7;
        int divider = Mathf.Clamp((lengthBarrier - message.Length), 1, lengthBarrier);
        float durationCoefficient = maxDuration / divider;

        return durationCoefficient;
    }
}
