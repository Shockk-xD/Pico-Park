using Photon.Pun;
using UnityEngine;

public class PhotonConnector : MonoBehaviourPunCallbacks
{
    [SerializeField] private Bootstrap _bootstrap;

    private void Start() {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster() {
        PhotonNetwork.JoinLobby();
        _bootstrap.OnConnectedHandler();
    }
}
