using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsPage : MonoBehaviour {
    [Serializable]
    private class OptionRow {
        public TMP_Text Label;
        public TMP_Text Value;
        public Button Previous;
        public Button Next;
    }

    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _title;
    [SerializeField] private Button _backButton;
    [SerializeField] private TMP_Text _backLabel;
    [SerializeField] private Button _applyButton;
    [SerializeField] private TMP_Text _applyLabel;
    [SerializeField] private OptionRow[] _rows;

    private readonly List<Vector2Int> _resolutions = new();
    private CanvasGroup _menuGroup;
    private GameObject _previousSelection;
    private int _resolutionIndex;
    private bool _fullscreen;
    private static readonly string[] _languageCodes = { "ru", "en", "uz", "kaa" };
    private static readonly string[] _languageNames = { "Русский", "English", "Oʻzbekcha", "Qaraqalpaqsha" };
    private int _languageIndex;
    private int _uiVolume;
    private int _effectsVolume;

    public bool IsOpen => _panel != null && _panel.activeSelf;

    private void Awake() {
        _panel.SetActive(false);
        GameAudio.BindButton(_backButton);
        GameAudio.BindButton(_applyButton);
        _backButton.onClick.AddListener(Close);
        _applyButton.onClick.AddListener(Apply);
        for (var index = 0; index < _rows.Length; index++) {
            var row = index;
            GameAudio.BindButton(_rows[row].Previous);
            GameAudio.BindButton(_rows[row].Next);
            _rows[row].Previous.onClick.AddListener(() => ChangeOption(row, -1));
            _rows[row].Next.onClick.AddListener(() => ChangeOption(row, 1));
        }
    }

    private void OnEnable() {
        GameLocalization.Changed += RefreshOpenPage;
    }

    private void OnDisable() {
        GameLocalization.Changed -= RefreshOpenPage;
        if (_panel != null && _panel.activeSelf) {
            Close();
        }
    }

    public void Open(CanvasGroup menuGroup) {
        _menuGroup = menuGroup;
        LoadOptions();
        RefreshText();
        _previousSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        _panel.SetActive(true);
        _menuGroup.interactable = false;
        EventSystem.current?.SetSelectedGameObject(_backButton.gameObject);
    }

    public void Close() {
        if (!IsOpen) {
            return;
        }

        _panel.SetActive(false);
        if (_menuGroup != null) {
            _menuGroup.interactable = true;
        }
        EventSystem.current?.SetSelectedGameObject(_previousSelection);
    }

    private void Apply() {
        GameSettings.Save(_resolutions[_resolutionIndex], _fullscreen, _languageCodes[_languageIndex],
            _uiVolume / 100f, _effectsVolume / 100f);
        Close();
    }

    private void LoadOptions() {
        _resolutions.Clear();
        foreach (var resolution in Screen.resolutions) {
            var size = new Vector2Int(resolution.width, resolution.height);
            if (!_resolutions.Contains(size)) {
                _resolutions.Add(size);
            }
        }

        var current = GameSettings.Resolution;
        if (!_resolutions.Contains(current)) {
            _resolutions.Add(current);
        }
        _resolutions.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
        _resolutionIndex = _resolutions.IndexOf(current);
        _fullscreen = GameSettings.Fullscreen;
        _languageIndex = Mathf.Max(0, Array.IndexOf(_languageCodes, GameSettings.LanguageCode));
        _uiVolume = Mathf.RoundToInt(GameSettings.UiVolume * 100f);
        _effectsVolume = Mathf.RoundToInt(GameSettings.EffectsVolume * 100f);
    }

    private void RefreshText() {
        SetLabel(_title, "menu.settings");
        SetLabel(_rows[0].Label, "settings.resolution");
        var resolution = _resolutions[_resolutionIndex];
        _rows[0].Value.text = $"{resolution.x} x {resolution.y}";
        SetLabel(_rows[1].Label, "settings.display");
        SetLabel(_rows[1].Value, _fullscreen ? "settings.fullscreen" : "settings.windowed");
        SetLabel(_rows[2].Label, "settings.language");
        _rows[2].Value.text = _languageNames[_languageIndex];
        SetLabel(_rows[3].Label, "settings.uiVolume");
        _rows[3].Value.text = $"{_uiVolume}%";
        SetLabel(_rows[4].Label, "settings.effectsVolume");
        _rows[4].Value.text = $"{_effectsVolume}%";
        SetLabel(_backLabel, "ui.back");
        SetLabel(_applyLabel, "ui.apply");
    }

    private void SetLabel(TMP_Text label, string key) {
        var text = GameLocalization.GetForLanguage(_languageCodes[_languageIndex], key);
        if (text != null) {
            label.text = text;
        }
    }

    private void RefreshOpenPage() {
        if (_panel.activeSelf) {
            RefreshText();
        }
    }

    private void ChangeOption(int row, int direction) {
        switch (row) {
            case 0:
                _resolutionIndex = (_resolutionIndex + direction + _resolutions.Count) % _resolutions.Count;
                break;
            case 1:
                _fullscreen = !_fullscreen;
                break;
            case 2:
                _languageIndex = (_languageIndex + direction + _languageCodes.Length) % _languageCodes.Length;
                break;
            case 3:
                _uiVolume = Mathf.Clamp(_uiVolume + 10 * direction, 0, 100);
                break;
            case 4:
                _effectsVolume = Mathf.Clamp(_effectsVolume + 10 * direction, 0, 100);
                break;
        }
        RefreshText();
    }
}
