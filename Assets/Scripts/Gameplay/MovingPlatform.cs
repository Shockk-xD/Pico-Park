using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(PhotonView))]
public class MovingPlatform : MonoBehaviour, IPunObservable {
    [SerializeField] private Transform _destination;
    [SerializeField, Min(0.01f)] private float _speed = 2f;
    [SerializeField] private bool _startRaised;
    [SerializeField] private PressurePlate[] _controlPlates;

    private Rigidbody2D _body;
    private PhotonView _view;
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private Vector2 _networkPosition;
    private bool _isRaised;

    private void Awake() {
        _view = GetComponent<PhotonView>();
        _view.Synchronization = ViewSynchronization.UnreliableOnChange;
        _view.FindObservables(true);
        _body = GetComponent<Rigidbody2D>();
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.interpolation = RigidbodyInterpolation2D.Interpolate;
        GetComponent<BoxCollider2D>().isTrigger = false;
        _startPosition = _body.position;
        _endPosition = _destination != null ? (Vector2)_destination.position : _startPosition;
        _isRaised = _startRaised;
        if (_startRaised) {
            _body.position = _endPosition;
        }
        _networkPosition = _body.position;
    }

    public void SetRaised(bool isRaised) {
        _isRaised = isRaised;
    }

    private void FixedUpdate() {
        if (PhotonNetwork.InRoom && !_view.IsMine) {
            _body.MovePosition(Vector2.MoveTowards(_body.position, _networkPosition, _speed * 2f * Time.fixedDeltaTime));
            return;
        }

        var isRaised = _isRaised;
        if (_controlPlates is { Length: > 0 }) {
            isRaised = false;
            foreach (var plate in _controlPlates) {
                if (plate != null && plate.IsPressed) {
                    isRaised = true;
                    break;
                }
            }
        }

        var destination = isRaised ? _endPosition : _startPosition;
        _body.MovePosition(Vector2.MoveTowards(_body.position, destination, _speed * Time.fixedDeltaTime));
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_body.position);
        } else {
            _networkPosition = (Vector2)stream.ReceiveNext();
        }
    }
}
