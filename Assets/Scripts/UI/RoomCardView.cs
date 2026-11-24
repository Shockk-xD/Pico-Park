using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomCardView : MonoBehaviour
{
    [SerializeField] private Button _onJoinButton;
    [SerializeField] private TMP_Text _roomNameText;
    [SerializeField] private TMP_Text _roomOnlineText;

    private RoomInfo _roomInfo;

    private void OnEnable() {
        GameLocalization.Changed += RefreshPlayers;
        RefreshPlayers();
    }

    private void OnDisable() {
        GameLocalization.Changed -= RefreshPlayers;
    }

    private void RefreshPlayers() {
        if (_roomInfo != null) {
            _roomOnlineText.text = GameLocalization.Get("lobby.roomPlayers", _roomInfo.PlayerCount, _roomInfo.MaxPlayers)
                ?? $"{_roomInfo.PlayerCount} / {_roomInfo.MaxPlayers}";
        }
    }

    public void Init(RoomInfo roomInfo, Action<RoomInfo> onJoinHandler) {
        _roomNameText.text = roomInfo.Name;
        _roomInfo = roomInfo;
        RefreshPlayers();
        GameAudio.BindButton(_onJoinButton);
        _onJoinButton.onClick.AddListener(() => onJoinHandler?.Invoke(roomInfo));
    }
}
