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

    public void Init(RoomInfo roomInfo, Action<RoomInfo> onJoinHandler) {
        _roomNameText.text = roomInfo.Name;
        _roomOnlineText.text = $"Игроки: {roomInfo.PlayerCount} / {roomInfo.MaxPlayers}";
        _onJoinButton.onClick.AddListener(() => onJoinHandler?.Invoke(roomInfo));
    }
}
