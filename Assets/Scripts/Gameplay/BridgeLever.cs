using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class BridgeLever : MonoBehaviour, IPunObservable {
    [SerializeField] private MovingPlatform _firstBridge;
    [SerializeField] private MovingPlatform _secondBridge;
    [SerializeField] private bool _firstBridgeRaised = true;

    private PhotonView _view;

    private void Awake() {
        _view = GetComponent<PhotonView>();
        _view.Synchronization = ViewSynchronization.UnreliableOnChange;
        _view.FindObservables(true);
    }

    private void Start() {
        ApplyState();
    }

    public void Toggle() {
        if (!PhotonNetwork.InRoom || _view.IsMine) {
            ToggleOnHost();
        } else {
            _view.RPC(nameof(ToggleOnHost), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void ToggleOnHost() {
        if (PhotonNetwork.InRoom && !_view.IsMine) {
            return;
        }

        _firstBridgeRaised = !_firstBridgeRaised;
        ApplyState();
    }

    private void ApplyState() {
        _firstBridge.SetRaised(_firstBridgeRaised);
        _secondBridge.SetRaised(!_firstBridgeRaised);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_firstBridgeRaised);
        } else {
            _firstBridgeRaised = (bool)stream.ReceiveNext();
            ApplyState();
        }
    }
}
