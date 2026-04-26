using UnityEngine;

[CreateAssetMenu(fileName = "Room Preferences", menuName = "Room/Preferences")]
public class RoomPreferences : ScriptableObject
{
    [field: SerializeField] public string DefaultName { get; private set; } = "{playerNickname}'s lobby";
    [field: SerializeField] public int MinPlayerCount { get; private set; } = 2;
    [field: SerializeField] public int MaxPlayerCount { get; private set; } = 4;
    [field: SerializeField] public bool IsHost { get; set; } = false;
}
