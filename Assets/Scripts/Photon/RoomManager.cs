using Photon.Pun;
using Photon.Realtime;
using System;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public const string MatchSeedProperty = "matchSeed";
    public const string MatchActorsProperty = "matchActors";
    private const string _creatorProperty = "roomCreator";
    private const string RoomVisibilityProperty = "lobbyVisibility";

    public event Action<Player> OnPlayerEntered;
    public event Action<Player> OnPlayerLeft;

    [SerializeField] private MainMenuUI _mainMenuUI;

    public static bool CanRestartLevel => !PhotonNetwork.InRoom
        || (PhotonNetwork.IsMasterClient && IsRoomCreator(PhotonNetwork.LocalPlayer.ActorNumber));

    public static void EnsureOnlineConnection() {
        if (PhotonNetwork.NetworkClientState == ClientState.PeerCreated
            || PhotonNetwork.NetworkClientState == ClientState.Disconnected) {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public static bool IsRoomCreator(int actorNumber) {
        var room = PhotonNetwork.CurrentRoom;
        return room != null && (room.CustomProperties.TryGetValue(_creatorProperty, out var value) && value is int creator
            ? actorNumber == creator
            : actorNumber == PhotonNetwork.MasterClient?.ActorNumber);
    }

    private void Start() {
        if (PhotonNetwork.InRoom) {
            RestoreRoomVisibility();
            _mainMenuUI.OnReturnedToRoom();
        }
    }

    public void CloseRoomForMatch() {
        var room = PhotonNetwork.CurrentRoom;
        room.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { RoomVisibilityProperty, room.IsVisible },
            { MatchSeedProperty, PhotonNetwork.ServerTimestamp },
            { MatchActorsProperty, Array.ConvertAll(PhotonNetwork.PlayerList, player => player.ActorNumber) }
        });
        room.IsOpen = false;
        room.IsVisible = false;
    }

    private void RestoreRoomVisibility() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }

        var room = PhotonNetwork.CurrentRoom;
        if (room.CustomProperties.TryGetValue(RoomVisibilityProperty, out var value) && value is bool wasVisible) {
            room.IsVisible = wasVisible;
            room.IsOpen = true;
        }
    }

    public bool CreateRoom(string roomName, RoomOptions roomOptions) {
        if (roomOptions.IsVisible) {
            return CreatePublicRoom(roomName, roomOptions);
        }

        return CreatePrivateRoom(roomOptions);
    }

    public void JoinRoomByCode(string roomCode) {
        PhotonNetwork.JoinRoom(roomCode.Trim().ToUpperInvariant());
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom() {
        _mainMenuUI.OnJoinedRoomHandler();
    }

    public override void OnCreatedRoom() {
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable {
            { _creatorProperty, PhotonNetwork.LocalPlayer.ActorNumber }
        });
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        OnPlayerEntered?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        RestoreRoomVisibility();
        _mainMenuUI.RefreshRoomState();
    }

    private bool CreatePublicRoom(string roomName, RoomOptions roomOptions) {
        return PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    private bool CreatePrivateRoom(RoomOptions roomOptions) {
        var roomCode = GenerateRoomCode();
        return PhotonNetwork.CreateRoom(roomCode, roomOptions);
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
