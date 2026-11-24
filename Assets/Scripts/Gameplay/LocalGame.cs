using UnityEngine;

public static class LocalGame {
    public const int PlayerCount = 2;

    public static bool IsActive { get; private set; }
    public static int ColorSeed { get; private set; }

    public static void Begin() {
        ColorSeed = Random.Range(0, int.MaxValue);
        IsActive = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void End() {
        IsActive = false;
    }
}
