using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LevelKey), typeof(PhotonView))]
public class ThrowableKey : MonoBehaviour, IPunObservable {
    [SerializeField] private Vector2 _throwVelocity = new(8f, 5f);
    [SerializeField, Min(0.01f)] private float _pickupReach = 0.45f;
    [SerializeField, Min(0.01f)] private float _carryHeight = 0.8f;
    [SerializeField] private float _fallLimit = -7f;
    [SerializeField] private string _dropHintText = "Press Q to drop the key";

    private readonly RaycastHit2D[] _flightHits = new RaycastHit2D[12];
    private PhotonView _view;
    private BoxCollider2D _collider;
    private PlayerMovement _offlineCarrier;
    private PlayerMovement _lastThrower;
    private PlayerMovement _previousCarrier;
    private TextMeshPro _dropHint;
    private Tween _hintAnimation;
    private Vector2 _velocity;
    private Vector2 _startPosition;
    private Vector2 _networkPosition;
    private float _catchBlockedUntil;
    private int _carrierViewId;
    private bool _isFlying;

    private bool HasAuthority => !PhotonNetwork.InRoom || _view.IsMine;

    private PlayerMovement Carrier {
        get {
            if (!PhotonNetwork.InRoom) {
                return _offlineCarrier;
            }

            var carrierView = _carrierViewId > 0 ? PhotonView.Find(_carrierViewId) : null;
            return carrierView != null ? carrierView.GetComponent<PlayerMovement>() : null;
        }
    }

    private void Awake() {
        _view = GetComponent<PhotonView>();
        _view.Synchronization = ViewSynchronization.UnreliableOnChange;
        _view.FindObservables(true);
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
        _startPosition = _networkPosition = transform.position;
    }

    public bool IsCarriedBy(PlayerMovement player) {
        return Carrier == player;
    }

    public void SetDropHintText(string text) {
        _dropHintText = text;
        if (_dropHint != null) {
            _dropHint.text = text;
        }
    }

    private void Update() {
        var carrier = Carrier;
        if (carrier != _previousCarrier) {
            _previousCarrier = carrier;
            if (carrier != null && carrier.IsLocalPlayer) {
                ShowDropHint(carrier);
            }
        }

        if (GamePauseMenu.BlocksGameplay || carrier == null || !carrier.IsLocalPlayer
            || !carrier.ThrowPressed) {
            return;
        }

        if (HasAuthority) {
            Throw(carrier, carrier.FacingDirection);
        } else {
            _view.RPC(nameof(RequestThrow), RpcTarget.MasterClient, carrier.FacingDirection);
        }
    }

    private void ShowDropHint(PlayerMovement carrier) {
        if (_dropHint == null) {
            var hintObject = new GameObject("Drop Key Hint");
            hintObject.transform.SetParent(transform, false);
            _dropHint = hintObject.AddComponent<TextMeshPro>();
            _dropHint.alignment = TextAlignmentOptions.Center;
            _dropHint.fontSize = 3.5f;
            _dropHint.rectTransform.sizeDelta = new Vector2(10f, 1.5f);
            _dropHint.outlineColor = Color.black;
            _dropHint.outlineWidth = 0.18f;
            hintObject.GetComponent<MeshRenderer>().sortingOrder = 10;
        }

        _hintAnimation?.Kill();
        _dropHint.text = GameLocalization.Get("game.dropKey", carrier.ThrowKeyLabel)
            ?? _dropHintText.Replace("Q", carrier.ThrowKeyLabel);
        _dropHint.color = Color.white;
        var position = carrier.transform.position + Vector3.up * (_carryHeight + 0.55f);
        _dropHint.transform.position = position;
        _dropHint.gameObject.SetActive(true);

        const float duration = 1.4f;
        _hintAnimation = DOTween.Sequence()
            .Append(_dropHint.transform.DOMove(position + Vector3.up * 0.85f, duration).SetEase(Ease.OutQuad))
            .Insert(0.35f, _dropHint.DOFade(0f, duration - 0.35f))
            .OnComplete(() => _dropHint.gameObject.SetActive(false));
    }

    private void OnDisable() {
        _hintAnimation?.Kill();
        _previousCarrier = null;
        if (_dropHint != null) {
            _dropHint.gameObject.SetActive(false);
        }
    }

    [PunRPC]
    private void RequestThrow(int direction, PhotonMessageInfo message) {
        var carrier = Carrier;
        if (HasAuthority && carrier != null && (direction == -1 || direction == 1)
            && carrier.GetComponent<PhotonView>().OwnerActorNr == message.Sender.ActorNumber) {
            Throw(carrier, direction);
        }
    }

    private void Throw(PlayerMovement carrier, int direction) {
        _lastThrower = carrier;
        _catchBlockedUntil = Time.time + 0.4f;
        SetCarrier(null);
        _velocity = new Vector2(_throwVelocity.x * direction, _throwVelocity.y);
        _isFlying = true;
    }

    private void FixedUpdate() {
        var carrier = Carrier;
        if (carrier != null && carrier.isActiveAndEnabled) {
            MoveTo((Vector2)carrier.transform.position + Vector2.up * _carryHeight);
            return;
        }

        if (!HasAuthority) {
            var blend = 1f - Mathf.Exp(-20f * Time.fixedDeltaTime);
            MoveTo(Vector2.Lerp(transform.position, _networkPosition, blend));
            return;
        }

        if (_carrierViewId > 0 || _offlineCarrier != null) {
            SetCarrier(null);
            _isFlying = true;
        }

        if (_isFlying) {
            MoveThroughAir();
        }

        TryPickUp();
    }

    private void SetCarrier(PlayerMovement player) {
        _offlineCarrier = PhotonNetwork.InRoom ? null : player;
        _carrierViewId = PhotonNetwork.InRoom && player != null ? player.GetComponent<PhotonView>().ViewID : 0;
        _velocity = Vector2.zero;
        _isFlying = false;
    }

    private void TryPickUp() {
        var pickupDistance = _pickupReach * _pickupReach;
        PlayerMovement closest = null;
        foreach (var player in PlayerMovement.ActivePlayers) {
            if (player.gameObject.scene != gameObject.scene
                || (player == _lastThrower && Time.time < _catchBlockedUntil)) {
                continue;
            }

            var distance = player.Bounds.SqrDistance(transform.position);
            if (distance <= pickupDistance) {
                closest = player;
                pickupDistance = distance;
            }
        }

        if (closest != null) {
            SetCarrier(closest);
        }
    }

    private void MoveThroughAir() {
        var position = (Vector2)transform.position;
        _velocity += Physics2D.gravity * Time.fixedDeltaTime;
        var delta = _velocity * Time.fixedDeltaTime;
        if (delta.sqrMagnitude < 0.000001f) {
            return;
        }

        var filter = new ContactFilter2D { useTriggers = false };
        var hitCount = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, delta.normalized,
            filter, _flightHits, delta.magnitude);
        for (var i = 0; i < hitCount; i++) {
            var hit = _flightHits[i];
            if (hit.collider.TryGetComponent<PlayerMovement>(out _)) {
                continue;
            }

            delta = delta.normalized * Mathf.Max(0f, hit.distance - 0.005f);
            if (hit.normal.y > 0.5f && _velocity.y < 0f) {
                _velocity = Vector2.zero;
                _isFlying = false;
            } else {
                var inwardSpeed = Vector2.Dot(_velocity, hit.normal);
                _velocity -= Mathf.Min(0f, inwardSpeed) * hit.normal;
            }
            break;
        }

        position += delta;
        if (position.y < _fallLimit) {
            position = _startPosition;
            _isFlying = false;
            _velocity = Vector2.zero;
        }

        MoveTo(position);
    }

    private void MoveTo(Vector2 position) {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_carrierViewId);
            stream.SendNext((Vector2)transform.position);
            stream.SendNext(_velocity);
            stream.SendNext(_isFlying);
        } else {
            _carrierViewId = (int)stream.ReceiveNext();
            _networkPosition = (Vector2)stream.ReceiveNext();
            _velocity = (Vector2)stream.ReceiveNext();
            _isFlying = (bool)stream.ReceiveNext();
        }
    }
}
