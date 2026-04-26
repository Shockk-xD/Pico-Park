using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public event Action<Player> OnPlayerEntered;
    public event Action<Player> OnPlayerLeft;

    [SerializeField] private MainMenuUI _mainMenuUI;

    public void CreateRoom(string roomName, RoomOptions roomOptions) {
        if (roomOptions.IsVisible) {
            CreatePublicRoom(roomName, roomOptions);
        } else {
            CreatePrivateRoom(roomOptions);
        }
    }

    public void JoinRoomByCode(string roomCode) {
        PhotonNetwork.JoinRoom(roomCode);
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom() {
        _mainMenuUI.OnJoinedRoomHandler();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        OnPlayerEntered?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    private void CreatePublicRoom(string roomName, RoomOptions roomOptions) {
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private void CreatePrivateRoom(RoomOptions roomOptions) {
        var roomCode = GenerateRoomCode();
        PhotonNetwork.CreateRoom(roomCode, roomOptions);
    }

    private string GenerateRoomCode() {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        const int roomCodeLength = 5;
        StringBuilder roomCodeSB = new(roomCodeLength);

        for (int i = 0; i < roomCodeLength; i++) {
            var randIndex = Random.Range(0, chars.Length);
            var randChar = chars[randIndex];

            roomCodeSB.Append(randChar);
        }

        return roomCodeSB.ToString();
    }
}
