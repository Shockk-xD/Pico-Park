using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Level/LevelData")]
public class LevelData : ScriptableObject
{
    [field: SerializeField, Min(1)] public int LevelId { get; private set; }
    [field: SerializeField] public Sprite LevelPreview { get; private set; }
    [field: SerializeField] public string SceneId { get; private set; }
}
