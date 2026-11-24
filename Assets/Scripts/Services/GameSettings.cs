using UnityEngine;

public static class GameSettings {
    private const string _widthKey = "Settings.Width";
    private const string _heightKey = "Settings.Height";
    private const string _fullscreenKey = "Settings.Fullscreen";
    private const string _languageKey = "Settings.Language";
    private const string _uiVolumeKey = "Settings.UiVolume";
    private const string _effectsVolumeKey = "Settings.EffectsVolume";

    public static Vector2Int Resolution => new(PlayerPrefs.GetInt(_widthKey, Screen.width),
        PlayerPrefs.GetInt(_heightKey, Screen.height));
    public static bool Fullscreen => PlayerPrefs.GetInt(_fullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
    public static string LanguageCode => PlayerPrefs.GetString(_languageKey, "ru");
    public static float UiVolume => PlayerPrefs.GetFloat(_uiVolumeKey, 1f);
    public static float EffectsVolume => PlayerPrefs.GetFloat(_effectsVolumeKey, 1f);

    public static void Save(Vector2Int resolution, bool fullscreen, string language, float uiVolume, float effectsVolume) {
        var applyDisplay = resolution != Resolution || fullscreen != Fullscreen;
        PlayerPrefs.SetInt(_widthKey, resolution.x);
        PlayerPrefs.SetInt(_heightKey, resolution.y);
        PlayerPrefs.SetInt(_fullscreenKey, fullscreen ? 1 : 0);
        var languageChanged = language != LanguageCode;
        PlayerPrefs.SetString(_languageKey, language);
        PlayerPrefs.SetFloat(_uiVolumeKey, Mathf.Clamp01(uiVolume));
        PlayerPrefs.SetFloat(_effectsVolumeKey, Mathf.Clamp01(effectsVolume));
        PlayerPrefs.Save();

        if (languageChanged) {
            GameLocalization.ApplyLanguage();
        }

        if (applyDisplay) {
            ApplyDisplay();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RestoreDisplay() {
#if !UNITY_EDITOR
        if (PlayerPrefs.HasKey(_widthKey) && PlayerPrefs.HasKey(_heightKey)) {
            ApplyDisplay();
        }
#endif
    }

    private static void ApplyDisplay() {
        var resolution = Resolution;
        if (resolution.x > 0 && resolution.y > 0) {
            var mode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(resolution.x, resolution.y, mode);
        }
    }
}
