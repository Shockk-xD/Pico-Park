using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class InteractionZone : MonoBehaviour {
    [SerializeField] private UnityEvent _onInteract;

    private readonly List<Collider2D> _overlaps = new();
    private BoxCollider2D _trigger;

    private void Awake() {
        _trigger = GetComponent<BoxCollider2D>();
        _trigger.isTrigger = true;
    }

    private void Update() {
        if (GamePauseMenu.BlocksGameplay) {
            return;
        }

        _trigger.OverlapCollider(new ContactFilter2D { useTriggers = false }, _overlaps);
        foreach (var collider in _overlaps) {
            if (collider.TryGetComponent<PlayerMovement>(out var player) && player.IsLocalPlayer && player.InteractPressed) {
                _onInteract?.Invoke();
                return;
            }
        }
    }
}
