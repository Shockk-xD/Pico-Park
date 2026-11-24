using System;
using Photon.Pun;
using UnityEngine;
using Zenject;

public class PlayerSpawner : MonoBehaviour {
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private LevelCamera _levelCamera;

    private void Start() {
        GamePauseMenu.CreateForLevel();
        if (LocalGame.IsActive) {
            SpawnLocalPlayers();
            return;
        }

        var gameInput = ProjectContext.Instance.Container.Resolve<GameInput>();
        var spawnPosition = transform.position;
        GameObject player;

        if (PhotonNetwork.InRoom) {
            var players = PhotonNetwork.PlayerList;
            var playerIndex = Array.IndexOf(players, PhotonNetwork.LocalPlayer);
            spawnPosition.x += playerIndex - (players.Length - 1) * 0.5f;
            var color = GetMatchColor(playerIndex);
            player = PhotonNetwork.Instantiate(_playerPrefab.name, spawnPosition, Quaternion.identity, 0,
                new object[] { color });
        } else {
            player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            player.GetComponent<PlayerAppearance>().PickLocalColor();
        }

        player.GetComponent<PlayerMovement>().Initialize(gameInput, spawnPosition);
        _levelCamera.Follow(player.transform);
    }

    private void SpawnLocalPlayers() {
        var first = SpawnLocalPlayer(1, transform.position + Vector3.left * 0.5f);
        var second = SpawnLocalPlayer(2, transform.position + Vector3.right * 0.5f);
        _levelCamera.FollowTogether(first.transform, second.transform);
    }

    private PlayerMovement SpawnLocalPlayer(int playerNumber, Vector3 spawnPosition) {
        var player = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
        var appearance = player.GetComponent<PlayerAppearance>();
        appearance.SetColor(appearance.PickColor(playerNumber - 1, LocalGame.ColorSeed));
        var movement = player.GetComponent<PlayerMovement>();
        movement.InitializeLocal(playerNumber, spawnPosition);
        return movement;
    }

    private int GetMatchColor(int playerIndex) {
        var properties = PhotonNetwork.CurrentRoom.CustomProperties;
        var seed = properties.TryGetValue(RoomManager.MatchSeedProperty, out var seedValue)
            && seedValue is int matchSeed ? matchSeed : 0;
        if (properties.TryGetValue(RoomManager.MatchActorsProperty, out var actorValue) && actorValue is int[] actors) {
            var slot = Array.IndexOf(actors, PhotonNetwork.LocalPlayer.ActorNumber);
            if (slot >= 0) {
                playerIndex = slot;
            }
        }

        return _playerPrefab.GetComponent<PlayerAppearance>().PickColor(playerIndex, seed);
    }
}
