using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D), typeof(PhotonView))]
public class PressurePlate : MonoBehaviour, IPunObservable {
    [SerializeField] private GameObject _defaultSprite;
    [SerializeField] private GameObject _pressedSprite;
    [SerializeField] private GameObject _target;
    [SerializeField] private bool _hideOnPress;
    [SerializeField, Min(0f)] private float _fadeDuration = 0.25f;
    [SerializeField] private UnityEvent<bool> _onPressedChanged;

    private readonly List<Collider2D> _overlaps = new();
    private readonly List<Collider2D> _targetColliders = new();
    private PhotonView _photonView;
    private BoxCollider2D _trigger;
    private Vector3 _targetScale;
    private Tween _animation;
    private bool _isPressed;

    public bool IsPressed => _isPressed;
    private bool IsTargetVisible => _isPressed != _hideOnPress;

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
        _photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
        _photonView.FindObservables(true);
        _trigger = GetComponent<BoxCollider2D>();
        _trigger.isTrigger = true;
        UpdateSprites();

        if (_target == null) {
            return;
        }

        if (transform.IsChildOf(_target.transform)) {
            Debug.LogError("PressurePlate needs a target outside its own hierarchy of parents.", this);
            enabled = false;
            return;
        }

        _targetScale = _target.transform.localScale;
        foreach (var collider in _target.GetComponentsInChildren<Collider2D>(true)) {
            if (collider.enabled) {
                _targetColliders.Add(collider);
            }
        }

        CompleteTransition();
    }

    private void OnDisable() {
        _animation?.Kill();
    }

    private void FixedUpdate() {
        if (PhotonNetwork.InRoom && !_photonView.IsMine) {
            return;
        }

        _trigger.OverlapCollider(new ContactFilter2D { useTriggers = false }, _overlaps);
        foreach (var collider in _overlaps) {
            if (collider.TryGetComponent<PlayerMovement>(out _) || collider.TryGetComponent<PushableCrate>(out _)) {
                SetPressed(true);
                return;
            }
        }

        SetPressed(false);
    }

    private void SetPressed(bool isPressed) {
        if (!isActiveAndEnabled || _isPressed == isPressed) {
            return;
        }

        _isPressed = isPressed;
        UpdateSprites();
        GameAudio.Play(isPressed ? GameAudio.Cue.PlatePress : GameAudio.Cue.PlateRelease);
        _onPressedChanged?.Invoke(isPressed);
        if (_target == null) {
            return;
        }

        var isVisible = IsTargetVisible;
        _animation?.Kill();
        if (isVisible && !_target.activeSelf) {
            _target.transform.localScale = Vector3.zero;
            _target.SetActive(true);
        }

        if (!isVisible) {
            SetTargetColliders(false);
        }

        if (_fadeDuration <= 0f) {
            CompleteTransition();
            return;
        }

        var scale = isVisible ? _targetScale : Vector3.zero;
        _animation = _target.transform.DOScale(scale, _fadeDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(CompleteTransition);
    }

    private void CompleteTransition() {
        _target.SetActive(IsTargetVisible);
        _target.transform.localScale = _targetScale;
        SetTargetColliders(IsTargetVisible);
    }

    private void UpdateSprites() {
        if (_defaultSprite != null) {
            _defaultSprite.SetActive(!_isPressed);
        }

        if (_pressedSprite != null) {
            _pressedSprite.SetActive(_isPressed);
        }
    }

    private void SetTargetColliders(bool isEnabled) {
        foreach (var collider in _targetColliders) {
            collider.enabled = isEnabled;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_isPressed);
        } else {
            SetPressed((bool)stream.ReceiveNext());
        }
    }
}
