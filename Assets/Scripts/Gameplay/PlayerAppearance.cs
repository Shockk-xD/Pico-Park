using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerAppearance : MonoBehaviour, IPunInstantiateMagicCallback {
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite[] _catSprites;
    [SerializeField, Min(0f)] private float _stepFrequency = 4f;
    [SerializeField, Min(0f)] private float _stepHeight = 0.04f;
    [SerializeField, Min(0f)] private float _stepTilt = 3f;

    private PlayerMovement _movement;
    private Transform _visual;
    private Vector3 _restPosition;
    private Quaternion _restRotation;
    private float _stepPhase;
    private float _walkBlend;

    private void Awake() {
        _movement = GetComponent<PlayerMovement>();
        _visual = _spriteRenderer.transform;
        _restPosition = _visual.localPosition;
        _restRotation = _visual.localRotation;
    }

    private void LateUpdate() {
        _spriteRenderer.flipX = _movement.FacingDirection < 0;
        _walkBlend = Mathf.MoveTowards(_walkBlend, _movement.IsWalking ? 1f : 0f, 10f * Time.deltaTime);
        if (_walkBlend <= 0f) {
            _stepPhase = 0f;
            _visual.localPosition = _restPosition;
            _visual.localRotation = _restRotation;
            return;
        }

        _stepPhase = Mathf.Repeat(_stepPhase + _stepFrequency * Time.deltaTime, 1f);
        var angle = _stepPhase * 2f * Mathf.PI;
        var lift = (1f - Mathf.Cos(angle)) * 0.5f * _stepHeight * _walkBlend;
        var tilt = Mathf.Sin(angle) * _stepTilt * _walkBlend;
        _visual.localPosition = _restPosition + Vector3.up * lift;
        _visual.localRotation = _restRotation * Quaternion.Euler(0f, 0f, tilt);
    }

    public int PickColor(int slot, int seed) {
        var colors = new int[_catSprites.Length];
        for (var i = 0; i < colors.Length; i++) {
            colors[i] = i;
        }

        var random = new System.Random(seed);
        for (var i = colors.Length - 1; i > 0; i--) {
            var other = random.Next(i + 1);
            (colors[i], colors[other]) = (colors[other], colors[i]);
        }

        return colors[slot % colors.Length];
    }

    public void PickLocalColor() {
        SetColor(Random.Range(0, _catSprites.Length));
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info) {
        if (info.photonView.InstantiationData is { Length: > 0 } data && data[0] is int color) {
            SetColor(color);
        }
    }

    public void SetColor(int color) {
        if (color >= 0 && color < _catSprites.Length) {
            _spriteRenderer.sprite = _catSprites[color];
        }
    }
}
