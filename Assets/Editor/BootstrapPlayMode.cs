using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class BootstrapPlayMode {
    private const string _menuPath = "Tools/Play From Bootstrap";
    private const string _bootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
    private static string PreferenceKey => $"{Application.dataPath}.PlayFromBootstrap";

    static BootstrapPlayMode() {
        EditorApplication.delayCall += ApplyStartScene;
    }

    [MenuItem(_menuPath)]
    private static void Toggle() {
        var isEnabled = EditorPrefs.GetBool(PreferenceKey, true);
        EditorPrefs.SetBool(PreferenceKey, !isEnabled);
        ApplyStartScene();
    }

    [MenuItem(_menuPath, true)]
    private static bool ValidateToggle() {
        Menu.SetChecked(_menuPath, EditorPrefs.GetBool(PreferenceKey, true));
        return !EditorApplication.isPlayingOrWillChangePlaymode;
    }

    private static void ApplyStartScene() {
        EditorSceneManager.playModeStartScene = EditorPrefs.GetBool(PreferenceKey, true)
            ? AssetDatabase.LoadAssetAtPath<SceneAsset>(_bootstrapScenePath)
            : null;
    }
}
