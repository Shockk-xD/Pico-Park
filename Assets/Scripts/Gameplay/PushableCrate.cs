using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(PhotonView))]
public class PushableCrate : MonoBehaviour, IPunObservable {
    [SerializeField, Min(0.1f)] private float _pushSpeed = 2.5f;
    [SerializeField] private float _fallLimit = -7f;

    private Rigidbody2D _body;
    private BoxCollider2D _collider;
    private PhotonView _view;
    private Vector2 _spawn;
    private Vector2 _networkPosition;

    private bool HasAuthority => !PhotonNetwork.InRoom || _view.IsMine;

    private void Awake() {
        _view = GetComponent<PhotonView>();
        _view.Synchronization = ViewSynchronization.UnreliableOnChange;
        _view.FindObservables(true);
        _body = GetComponent<Rigidbody2D>();
        _body.freezeRotation = true;
        _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = false;
        _spawn = _networkPosition = _body.position;
    }

    private void FixedUpdate() {
        var bodyType = HasAuthority ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        if (_body.bodyType != bodyType) {
            _body.bodyType = bodyType;
        }

        if (!HasAuthority) {
            var blend = 1f - Mathf.Exp(-18f * Time.fixedDeltaTime);
            _body.MovePosition(Vector2.Lerp(_body.position, _networkPosition, blend));
            return;
        }

        if (_body.position.y < _fallLimit) {
            _body.position = _spawn;
            _body.velocity = Vector2.zero;
            return;
        }

        var crate = _collider.bounds;
        var push = 0;
        foreach (var player in PlayerMovement.ActivePlayers) {
            var bounds = player.Bounds;
            if (player.gameObject.scene != gameObject.scene || bounds.min.y + 0.1f >= crate.max.y
                || bounds.max.y <= crate.min.y + 0.1f) {
                continue;
            }

            if (player.MoveInput > 0.1f && bounds.center.x < crate.center.x && bounds.max.x >= crate.min.x - 0.15f) {
                push++;
            } else if (player.MoveInput < -0.1f && bounds.center.x > crate.center.x
                && bounds.min.x <= crate.max.x + 0.15f) {
                push--;
            }
        }

        if (push != 0) {
            _body.velocity = new Vector2(Mathf.Sign(push) * _pushSpeed, _body.velocity.y);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_body.position);
        } else {
            _networkPosition = (Vector2)stream.ReceiveNext();
        }
    }
}
