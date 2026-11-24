using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LocalClue : MonoBehaviour {
    [SerializeField] private GameObject _clueVisual;

    private readonly List<Collider2D> _overlaps = new();
    private BoxCollider2D _trigger;

    private void Awake() {
        _trigger = GetComponent<BoxCollider2D>();
        _trigger.isTrigger = true;
        if (_clueVisual == null || transform.IsChildOf(_clueVisual.transform)) {
            Debug.LogError("Assign a clue visual that is separate from its trigger.", this);
            enabled = false;
            return;
        }

        _clueVisual.SetActive(false);
    }

    private void FixedUpdate() {
        var isVisible = false;
        _trigger.OverlapCollider(new ContactFilter2D { useTriggers = false }, _overlaps);
        foreach (var collider in _overlaps) {
            if (collider.TryGetComponent<PlayerMovement>(out var player) && player.IsLocalPlayer) {
                isVisible = true;
                break;
            }
        }

        if (_clueVisual.activeSelf != isVisible) {
            _clueVisual.SetActive(isVisible);
        }
    }
}
