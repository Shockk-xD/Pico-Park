using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private RoomSettingsPage _roomSettingsPage;
    [SerializeField] private RoomsListPage _roomsPage;
    [SerializeField] private RoomPage _roomPage;

    public void OnJoinedRoomHandler() {
        _roomSettingsPage.HidePage();
        _roomsPage.HidePage();
        _roomPage.ShowPage();
    }
}
