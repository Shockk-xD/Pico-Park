using Photon.Pun;

public static class PlayerNicknameStorageService
{
    private readonly static JsonToFileStorageService _storageService = new();
    private const string playerNicknameKey = nameof(playerNicknameKey);

    public static string PlayerNickname => GetPlayerNickname();

    public static void SavePlayerNickname(string nickname) {
        _storageService.Save(playerNicknameKey, nickname);
        PhotonNetwork.NickName = nickname;
    }

    private static string GetPlayerNickname() {
        var playerNickname = _storageService.Load<string>(playerNicknameKey);
        PhotonNetwork.NickName = playerNickname;
        return playerNickname;
    }
}
