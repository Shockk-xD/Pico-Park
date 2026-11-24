using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class GameLocalization {
    private const string _tableName = "Game";
    private static readonly Dictionary<string, StringTable> _tables = new();
    private static readonly List<AsyncOperationHandle<StringTable>> _tableHandles = new();
    private static bool _isReady;

    public static event Action Changed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset() {
        foreach (var handle in _tableHandles) {
            if (handle.IsValid()) {
                Addressables.Release(handle);
            }
        }
        _tableHandles.Clear();
        _tables.Clear();
        _isReady = false;
        Changed = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() {
        LocalizationSettings.InitializationOperation.Completed += LoadTables;
    }

    private static async void LoadTables(AsyncOperationHandle<LocalizationSettings> initialization) {
        if (initialization.Status != AsyncOperationStatus.Succeeded) {
            Debug.LogError("Could not initialize Unity Localization.");
            return;
        }

        try {
            foreach (var locale in LocalizationSettings.AvailableLocales.Locales) {
                var handle = LocalizationSettings.StringDatabase.GetTableAsync(_tableName, locale);
                Addressables.ResourceManager.Acquire(handle);
                _tableHandles.Add(handle);
                var table = await handle.Task;
                if (table != null) {
                    _tables.Add(locale.Identifier.Code, table);
                }
            }

            _isReady = true;
            ApplyLanguage();
        } catch (Exception exception) {
            Debug.LogException(exception);
        }
    }

    public static void ApplyLanguage() {
        if (!_isReady) {
            return;
        }

        var locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(GameSettings.LanguageCode));
        if (locale != null) {
            LocalizationSettings.SelectedLocale = locale;
        }

        Changed?.Invoke();
    }

    public static string Get(string key, params object[] args) {
        return GetForLanguage(GameSettings.LanguageCode, key, args);
    }

    public static string GetForLanguage(string language, string key, params object[] args) {
        if (!_tables.TryGetValue(language, out var table) && !_tables.TryGetValue("ru", out table)) {
            return null;
        }

        var entry = table.GetEntry(key);
        if (entry == null) {
            Debug.LogWarning($"Missing localization entry: {key}");
            return null;
        }

        return args.Length == 0 ? entry.GetLocalizedString() : entry.GetLocalizedString(args);
    }
}
