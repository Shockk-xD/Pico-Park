using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhotonView), typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour, IPunObservable {
    private static readonly List<PlayerMovement> _activePlayers = new();

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpSpeed = 8f;
    [SerializeField] private float _fallLimit = -7f;
    [SerializeField] private float _networkSmoothing = 18f;

    private readonly RaycastHit2D[] _castHits = new RaycastHit2D[8];
    private Rigidbody2D _rigidbody;
    private BoxCollider2D _collider;
    private InputActionMap _gameplayInput;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private Vector2 _spawnPosition;
    private Vector2 _networkPosition;
    private bool _jumpRequested;
    private int _keyboardSlot;
    private PhotonView _photonView;
    private TextMeshPro _signal;
    private float _signalUntil;

    public bool IsLocalPlayer => _gameplayInput != null;
    public static IReadOnlyList<PlayerMovement> ActivePlayers => _activePlayers;
    public Bounds Bounds => _collider.bounds;
    public float MoveInput { get; private set; }
    public int FacingDirection { get; private set; } = 1;
    public bool IsWalking { get; private set; }
    public bool InteractPressed => Keyboard.current?[_keyboardSlot == 2 ? Key.DownArrow : Key.E].wasPressedThisFrame == true;
    public bool ThrowPressed => Keyboard.current?[_keyboardSlot == 2 ? Key.RightCtrl : Key.Q].wasPressedThisFrame == true;
    public string ThrowKeyLabel => _keyboardSlot == 2 ? "R Ctrl" : "Q";

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
        _networkPosition = _rigidbody.position;
    }

    public void Initialize(GameInput gameInput, Vector2 spawnPosition) {
        Initialize(gameInput.asset.FindActionMap("Gameplay", true), spawnPosition);
    }

    public void InitializeLocal(int playerNumber, Vector2 spawnPosition) {
        _keyboardSlot = playerNumber;
        var useArrows = playerNumber == 2;
        var input = new InputActionMap("Local Player");
        var move = input.AddAction("Move", InputActionType.Value);
        move.AddCompositeBinding("1DAxis")
            .With("Negative", useArrows ? "<Keyboard>/leftArrow" : "<Keyboard>/a")
            .With("Positive", useArrows ? "<Keyboard>/rightArrow" : "<Keyboard>/d");
        input.AddAction("Jump", InputActionType.Button, useArrows ? "<Keyboard>/upArrow" : "<Keyboard>/w");
        Initialize(input, spawnPosition);
    }

    private void Initialize(InputActionMap input, Vector2 spawnPosition) {
        _spawnPosition = spawnPosition;
        _gameplayInput = input;
        _moveAction = _gameplayInput.FindAction("Move", true);
        _jumpAction = _gameplayInput.FindAction("Jump", true);
        EnableInput();
    }

    private void OnDestroy() {
        if (_keyboardSlot != 0) {
            _gameplayInput.Dispose();
        }
    }

    private void Start() {
        if (_gameplayInput == null) {
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void OnEnable() {
        _activePlayers.Add(this);
        if (_gameplayInput != null) {
            EnableInput();
        }
    }

    private void OnDisable() {
        _activePlayers.Remove(this);
        if (_signal != null) {
            _signal.gameObject.SetActive(false);
        }
        MoveInput = 0f;
        IsWalking = false;
        if (_gameplayInput != null) {
            _jumpAction.performed -= OnJump;
            _gameplayInput.Disable();
            _jumpRequested = false;
        }
    }

    private void Update() {
        if (_signal != null && _signal.gameObject.activeSelf && Time.unscaledTime >= _signalUntil) {
            _signal.gameObject.SetActive(false);
        }

        if (!IsLocalPlayer || GamePauseMenu.BlocksGameplay
            || Keyboard.current?[_keyboardSlot == 2 ? Key.RightShift : Key.F].wasPressedThisFrame != true) {
            return;
        }

        ShowSignal();
        if (PhotonNetwork.InRoom && _photonView.IsMine) {
            _photonView.RPC(nameof(ReceiveSignal), RpcTarget.Others);
        }
    }

    [PunRPC]
    private void ReceiveSignal(PhotonMessageInfo info) {
        if (isActiveAndEnabled && info.Sender != null && info.Sender.ActorNumber == _photonView.OwnerActorNr) {
            ShowSignal();
        }
    }

    private void ShowSignal() {
        if (_signal == null) {
            var bounds = _collider.bounds;
            var position = new Vector3(bounds.center.x, bounds.max.y + 0.3f, transform.position.z);
            _signal = WorldLabel.Create(transform, GetComponentInChildren<SpriteRenderer>(), "Player Signal", position, 4f);
            _signal.text = "!";
        }

        _signalUntil = Time.unscaledTime + 1f;
        _signal.gameObject.SetActive(true);
    }

    private void EnableInput() {
        _jumpAction.performed += OnJump;
        _gameplayInput.Enable();
    }

    private void OnJump(InputAction.CallbackContext context) {
        _jumpRequested = !GamePauseMenu.BlocksGameplay;
    }

    private void FixedUpdate() {
        if (_gameplayInput == null) {
            var blend = 1f - Mathf.Exp(-_networkSmoothing * Time.fixedDeltaTime);
            _rigidbody.MovePosition(Vector2.Lerp(_rigidbody.position, _networkPosition, blend));
            return;
        }

        if (_rigidbody.position.y < _fallLimit) {
            _rigidbody.position = _spawnPosition;
            _rigidbody.velocity = Vector2.zero;
            MoveInput = 0f;
            IsWalking = false;
            _jumpRequested = false;
            GameAudio.Play(GameAudio.Cue.Respawn);
            return;
        }

        var velocity = _rigidbody.velocity;
        var inputBlocked = GamePauseMenu.BlocksGameplay;
        MoveInput = inputBlocked ? 0f : _moveAction.ReadValue<float>();
        if (Mathf.Abs(MoveInput) > 0.1f) {
            FacingDirection = MoveInput > 0f ? 1 : -1;
        }

        var isGrounded = IsGrounded(out var boostBlock);
        IsWalking = isGrounded && Mathf.Abs(MoveInput) > 0.1f && velocity.x * FacingDirection > 0.12f;
        velocity.x = MoveInput * _moveSpeed;

        if (_jumpRequested && isGrounded && !inputBlocked) {
            IsWalking = false;
            velocity.y = _jumpSpeed * (boostBlock != null ? boostBlock.JumpMultiplier : 1f);
            if (boostBlock != null) {
                boostBlock.PlayBoost();
            } else {
                GameAudio.Play(GameAudio.Cue.Jump);
            }
        }

        if ((!isGrounded || velocity.y > 0.1f) && !Mathf.Approximately(velocity.x, 0f)) {
            velocity.x = LimitAirMovementAtWall(velocity.x);
        }

        _jumpRequested = false;
        _rigidbody.velocity = velocity;
    }

    private bool IsGrounded(out JumpBoostBlock boostBlock) {
        boostBlock = null;
        var filter = new ContactFilter2D { useTriggers = false };
        var hitCount = _collider.Cast(Vector2.down, filter, _castHits, 0.06f);
        var isGrounded = false;

        for (var i = 0; i < hitCount; i++) {
            var hit = _castHits[i];
            var isBelowFeet = hit.collider.bounds.max.y <= _collider.bounds.min.y + 0.12f;
            if (hit.normal.y > 0.5f && (isBelowFeet
                || (hit.distance > 0f && !hit.collider.TryGetComponent<PlayerMovement>(out _)))) {
                if (!_jumpRequested) {
                    return true;
                }

                isGrounded = true;
                if (isBelowFeet && hit.collider.TryGetComponent<JumpBoostBlock>(out var block)
                    && block.isActiveAndEnabled && block.JumpMultiplier > (boostBlock != null ? boostBlock.JumpMultiplier : 1f)) {
                    boostBlock = block;
                }
            }
        }

        return isGrounded;
    }

    private float LimitAirMovementAtWall(float speed) {
        var direction = Mathf.Sign(speed);
        var distance = Mathf.Abs(speed) * Time.fixedDeltaTime;
        var clearance = 2f * Physics2D.defaultContactOffset;
        var bounds = _collider.bounds;
        var filter = new ContactFilter2D { useTriggers = false };
        var hitCount = _collider.Cast(Vector2.right * direction, filter, _castHits, distance + clearance);

        for (var i = 0; i < hitCount; i++) {
            var hit = _castHits[i];
            var obstacle = hit.collider.bounds;
            if (hit.normal.x * direction < -0.75f && obstacle.max.y > bounds.min.y + clearance
                && obstacle.min.y < bounds.max.y - clearance) {
                distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - clearance));
            }
        }

        return direction * distance / Time.fixedDeltaTime;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_rigidbody.position);
            stream.SendNext(MoveInput);
            stream.SendNext(FacingDirection);
            stream.SendNext(IsWalking);
        } else {
            _networkPosition = (Vector2)stream.ReceiveNext();
            MoveInput = (float)stream.ReceiveNext();
            FacingDirection = (int)stream.ReceiveNext();
            IsWalking = (bool)stream.ReceiveNext();
        }
    }
}
