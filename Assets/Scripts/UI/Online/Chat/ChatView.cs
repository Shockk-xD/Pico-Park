using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Zenject;

[RequireComponent(typeof(PhotonView))]
public class ChatView : MonoBehaviour
{
    [SerializeField] private Button _sendButton;
    [SerializeField] private TMP_InputField _messageInputField;
    [SerializeField] private MessageView _messageViewPrefab;
    [SerializeField] private Transform _messageContent;

    private GameInput _inputSystem;
    private PhotonView _photonView;
    private string PlayerNickname => PhotonNetwork.NickName;

    private void Awake() {
        _inputSystem = new();
        _inputSystem.Enable();
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable() {
        ClearChat();
        _sendButton.onClick.AddListener(SendMessage);
        _messageInputField.onEndEdit.AddListener(OnTextEndEditHandler);
        _inputSystem.UIRoomPage.SendChatMessage.performed += SendMessage;
    }

    private void OnDisable() {
        _sendButton.onClick.RemoveListener(SendMessage);
        _messageInputField.onEndEdit.RemoveListener(OnTextEndEditHandler);
        _inputSystem.UIRoomPage.SendChatMessage.performed -= SendMessage;
    }

    private void OnTextEndEditHandler(string text) {
        if (string.IsNullOrWhiteSpace(text)) {
            _messageInputField.text = string.Empty;
        }
    }

    private void SendMessage() {
        var message = _messageInputField.text;

        if (!string.IsNullOrWhiteSpace(message)) {
            var messageSender = PlayerNickname;

            _photonView.RPC(nameof(GetMessage), RpcTarget.All, message, messageSender);

            _messageInputField.text = null;
        }
    }

    private void SendMessage(UnityEngine.InputSystem.InputAction.CallbackContext _) {
        SendMessage();
    }

    [PunRPC]
    private void GetMessage(string message, string messageSender) {
        var messageView = Instantiate(_messageViewPrefab, _messageContent);
        messageView.ShowMessage(message, messageSender);
    }

    private void ClearChat() {
        foreach (Transform child in _messageContent) {
            Destroy(child.gameObject);
        }
    }
}
