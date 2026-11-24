using TMPro;
using UnityEngine;

public static class WorldLabel {
    public static TextMeshPro Create(Transform parent, SpriteRenderer sprite, string name, Vector3 position, float fontSize) {
        var labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);
        labelObject.transform.position = position;
        var scale = parent.lossyScale;
        labelObject.transform.localScale = new Vector3(1f / Mathf.Abs(scale.x), 1f / Mathf.Abs(scale.y), 1f);

        var label = labelObject.AddComponent<TextMeshPro>();
        label.alignment = TextAlignmentOptions.Center;
        label.rectTransform.sizeDelta = new Vector2(2f, 0.5f);
        label.fontSize = fontSize;
        label.enableWordWrapping = false;
        label.outlineColor = Color.black;
        label.outlineWidth = 0.2f;
        if (sprite != null) {
            var renderer = labelObject.GetComponent<MeshRenderer>();
            renderer.sortingLayerID = sprite.sortingLayerID;
            renderer.sortingOrder = sprite.sortingOrder + 1;
        }

        return label;
    }
}
