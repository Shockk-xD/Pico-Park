using Photon.Pun;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(PhotonView))]
public class PushableWall : MonoBehaviour, IPunObservable {
    private const float _contactDistance = 0.2f;

    [SerializeField, Min(0.01f)] private float _moveSpeed = 1.75f;
    [SerializeField, Min(0.01f)] private float _pushReach = 2f;
    [SerializeField, Min(0f)] private float _travelLeft;
    [SerializeField, Min(0f)] private float _travelRight = 4f;

    private PhotonView _photonView;
    private Rigidbody2D _rigidbody;
    private BoxCollider2D _collider;
    private LevelExit _levelExit;
    private TextMeshPro _pushCounter;
    private float _startX;
    private float _networkX;
    private int _pushingCount;
    private int _requiredCount;

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
        _photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
        _photonView.FindObservables(true);
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = false;
        _startX = _networkX = _rigidbody.position.x;
    }

    private void Start() {
        _levelExit = FindObjectOfType<LevelExit>();
    }

    private void LateUpdate() {
        if (_pushCounter != null && _pushCounter.gameObject.activeSelf) {
            _pushCounter.transform.position = transform.TransformPoint(_collider.offset);
        }
    }

    private void FixedUpdate() {
        var position = _rigidbody.position;
        var canMoveWall = !PhotonNetwork.InRoom || _photonView.IsMine;
        if (canMoveWall) {
            var direction = GetPushDirection();
            position.x = Mathf.Clamp(position.x + direction * _moveSpeed * Time.fixedDeltaTime,
                _startX - _travelLeft, _startX + _travelRight);
            _networkX = position.x;
        } else {
            position.x = Mathf.MoveTowards(position.x, _networkX, _moveSpeed * 2f * Time.fixedDeltaTime);
        }

        _rigidbody.MovePosition(position);
    }

    private int GetPushDirection() {
        var wallBounds = _collider.bounds;
        var rightPushers = 0;
        var leftPushers = 0;
        var playersInScene = 0;
        var rightContact = false;
        var leftContact = false;

        foreach (var player in PlayerMovement.ActivePlayers) {
            if (player.gameObject.scene != gameObject.scene) {
                continue;
            }

            playersInScene++;
            var direction = GetPlayerDirection(player, wallBounds);
            if (direction > 0) {
                rightPushers++;
                rightContact |= wallBounds.min.x - player.Bounds.max.x <= _contactDistance;
            } else if (direction < 0) {
                leftPushers++;
                leftContact |= player.Bounds.min.x - wallBounds.max.x <= _contactDistance;
            }
        }

        var required = PhotonNetwork.InRoom ? CountPlayersInLevel() : playersInScene;
        var pushRight = rightContact ? rightPushers : 0;
        var pushLeft = leftContact ? leftPushers : 0;
        UpdateCounter(Mathf.Max(pushRight, pushLeft), required);

        if (required > 0) {
            if (pushRight == required) {
                return 1;
            }
            if (pushLeft == required) {
                return -1;
            }
        }

        return 0;
    }

    private void UpdateCounter(int pushing, int required) {
        if (_pushingCount == pushing && _requiredCount == required) {
            return;
        }

        _pushingCount = pushing;
        _requiredCount = required;
        if (pushing == 0 || required == 0) {
            if (_pushCounter != null) {
                _pushCounter.gameObject.SetActive(false);
            }
            return;
        }

        if (_pushCounter == null) {
            CreateCounter();
        }

        _pushCounter.text = $"{pushing}/{required}";
        _pushCounter.gameObject.SetActive(true);
    }

    private void CreateCounter() {
        var counterObject = new GameObject("Push Counter");
        counterObject.transform.SetParent(transform, false);
        var scale = transform.lossyScale;
        counterObject.transform.localScale = new Vector3(1f / Mathf.Abs(scale.x), 1f / Mathf.Abs(scale.y), 1f);
        counterObject.transform.position = transform.TransformPoint(_collider.offset);

        _pushCounter = counterObject.AddComponent<TextMeshPro>();
        _pushCounter.alignment = TextAlignmentOptions.Center;
        _pushCounter.rectTransform.sizeDelta = new Vector2(_collider.bounds.size.x * 0.9f, 0.75f);
        _pushCounter.enableAutoSizing = true;
        _pushCounter.fontSizeMin = 0.5f;
        _pushCounter.fontSizeMax = 4.5f;
        _pushCounter.enableWordWrapping = false;
        _pushCounter.outlineColor = Color.black;
        _pushCounter.outlineWidth = 0.18f;

        var labelRenderer = counterObject.GetComponent<MeshRenderer>();
        var wallRenderer = GetComponentInChildren<SpriteRenderer>();
        if (wallRenderer != null) {
            labelRenderer.sortingLayerID = wallRenderer.sortingLayerID;
            labelRenderer.sortingOrder = wallRenderer.sortingOrder + 1;
        }
    }

    private int GetPlayerDirection(PlayerMovement player, Bounds wall) {
        var bounds = player.Bounds;
        if (bounds.min.y >= wall.max.y || bounds.max.y <= wall.min.y) {
            return 0;
        }

        if (bounds.center.x < wall.center.x && player.MoveInput > 0.1f && wall.min.x - bounds.max.x <= _pushReach) {
            return 1;
        }

        if (bounds.center.x > wall.center.x && player.MoveInput < -0.1f && bounds.min.x - wall.max.x <= _pushReach) {
            return -1;
        }

        return 0;
    }

    private int CountPlayersInLevel() {
        var count = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values) {
            if (!player.IsInactive && (_levelExit == null || !_levelExit.HasPlayerFinished(player.ActorNumber))) {
                count++;
            }
        }

        return count;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_networkX);
            stream.SendNext(_pushingCount);
            stream.SendNext(_requiredCount);
        } else {
            _networkX = (float)stream.ReceiveNext();
            UpdateCounter((int)stream.ReceiveNext(), (int)stream.ReceiveNext());
        }
    }
}
