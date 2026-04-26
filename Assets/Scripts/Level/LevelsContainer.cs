using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelsContainer", menuName = "Level/LevelsContainer")]
public class LevelsContainer : ScriptableObject
{
    [field: SerializeField] public List<LevelData> Levels { get; private set; }
}
