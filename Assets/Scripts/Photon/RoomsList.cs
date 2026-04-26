using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomsList : MonoBehaviourPunCallbacks
{
    [SerializeField] private RoomCardView _roomCard;
    [SerializeField] private Transform _content;
    [SerializeField] private RoomsListPage _roomsListPage;
    [SerializeField] private RoomPreferences _roomPreferences;

    private new void OnEnable() {
        base.OnEnable();
        PhotonNetwork.JoinLobby();
        _roomPreferences.IsHost = false;
    }

    private new void OnDisable() {
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InLobby) {
            PhotonNetwork.LeaveLobby();
        }

        base.OnDisable();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        ClearRoomsList();
        _roomsListPage.NoRoomsStateHandler(roomList.Count);

        foreach (var roomInfo in roomList) {
            if (roomInfo.IsVisible) {
                var roomCardView = Instantiate(_roomCard, _content);
                roomCardView.Init(roomInfo, JoinRoom);
            }
        }
    }

    private void JoinRoom(RoomInfo roomInfo) {
        if (!roomInfo.RemovedFromList) {
            var roomName = roomInfo.Name;
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    private void ClearRoomsList() {
        foreach (Transform child in _content) {
            Destroy(child.gameObject);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message) {
        _roomsListPage.OnJoinRoomFailedHandler(message);
    }
}
