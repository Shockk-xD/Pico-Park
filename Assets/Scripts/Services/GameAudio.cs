using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameAudio : MonoBehaviour {
    public enum Cue {
        Click,
        Error,
        Jump,
        Respawn,
        KeyPickup,
        PlayerExit,
        LevelClear,
        PlatePress,
        PlateRelease,
        Hover,
        BoostJump
    }

    [Header("UI")]
    [SerializeField] private AudioClip _click;
    [SerializeField] private AudioClip _error;
    [SerializeField, Range(0f, 1f)] private float _uiVolume = 0.55f;

    [Header("Gameplay")]
    [SerializeField] private AudioClip _jump;
    [SerializeField] private AudioClip _boostJump;
    [SerializeField] private AudioClip _respawn;
    [SerializeField] private AudioClip _keyPickup;
    [SerializeField] private AudioClip _playerExit;
    [SerializeField] private AudioClip _levelClear;
    [SerializeField] private AudioClip _platePress;
    [SerializeField] private AudioClip _plateRelease;
    [SerializeField, Range(0f, 1f)] private float _gameplayVolume = 0.65f;

    private static GameAudio _instance;
    private AudioSource _source;

    private void Awake() {
        _instance = this;
        _source = GetComponent<AudioSource>();
        _source.spatialBlend = 0f;
    }

    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start() {
        for (var i = 0; i < SceneManager.sceneCount; i++) {
            BindSceneButtons(SceneManager.GetSceneAt(i));
        }
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy() {
        if (_instance == this) {
            _instance = null;
        }
    }

    public static void Play(Cue cue) {
        if (_instance != null) {
            _instance.PlayClip(cue);
        }
    }

    public static void BindButton(Button button) {
        button.onClick.RemoveListener(PlayClick);
        button.onClick.AddListener(PlayClick);
    }

    private static void PlayClick() {
        Play(Cue.Click);
    }

    private static void PlayToggle(bool _) {
        Play(Cue.Click);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        BindSceneButtons(scene);
    }

    private void BindSceneButtons(Scene scene) {
        foreach (var root in scene.GetRootGameObjects()) {
            foreach (var button in root.GetComponentsInChildren<Button>(true)) {
                BindButton(button);
            }

            foreach (var toggle in root.GetComponentsInChildren<Toggle>(true)) {
                toggle.onValueChanged.RemoveListener(PlayToggle);
                toggle.onValueChanged.AddListener(PlayToggle);
            }
        }
    }

    private void PlayClip(Cue cue) {
        var clip = cue switch {
            Cue.Click => _click,
            Cue.Error => _error,
            Cue.Jump => _jump,
            Cue.BoostJump => _boostJump,
            Cue.Respawn => _respawn,
            Cue.KeyPickup => _keyPickup,
            Cue.PlayerExit => _playerExit,
            Cue.LevelClear => _levelClear,
            Cue.PlatePress => _platePress,
            Cue.PlateRelease => _plateRelease,
            Cue.Hover => _click,
            _ => null
        };

        if (clip != null) {
            var volume = cue is Cue.Click or Cue.Error or Cue.Hover
                ? _uiVolume * GameSettings.UiVolume
                : _gameplayVolume * GameSettings.EffectsVolume;
            if (cue == Cue.Hover) {
                volume *= 0.3f;
            }
            _source.PlayOneShot(clip, volume);
        }
    }
}
