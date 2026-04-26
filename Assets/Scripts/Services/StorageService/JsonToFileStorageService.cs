using Newtonsoft.Json;
using System.IO;

using UnityEngine;

public class JsonToFileStorageService : IStorageService {
    public void Save<T>(string key, T data) {
        var path = GetPath(key);
        string json = JsonConvert.SerializeObject(data);

        using (var streamWriter = new StreamWriter(path)) {
            streamWriter.Write(json);
        }
    }

    public T Load<T>(string key) {
        var path = GetPath(key);

        if (!File.Exists(path)) {
            return default;
        }

        using (var streamReader = new StreamReader(path)) {
            var json = streamReader.ReadToEnd();
            var data = JsonConvert.DeserializeObject<T>(json);

            return data;
        }
    }

    private string GetPath(string key) {
        return Path.Combine(Application.persistentDataPath, key);
    }
}
