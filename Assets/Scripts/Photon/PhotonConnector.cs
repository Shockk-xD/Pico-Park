using UnityEngine;

public class PhotonConnector : MonoBehaviour
{
    private void Start() {
        RoomManager.EnsureOnlineConnection();
    }
}
