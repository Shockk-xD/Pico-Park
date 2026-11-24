using UnityEngine;

[RequireComponent(typeof(Camera))]
public class LevelCamera : MonoBehaviour {
    [SerializeField] private BoxCollider2D _cameraBounds;
    [SerializeField, Min(0.01f)] private float _followTime = 0.2f;
    [SerializeField, Min(0f)] private float _verticalLead = 1f;

    private Camera _camera;
    private Transform _target;
    private Transform _secondTarget;
    private bool _followTogether;
    private bool _isSpectating;
    private Vector2 _followVelocity;
    private float _startingY;
    private float _startingSize;
    private float _zoomVelocity;
    private SpriteRenderer _background;
    private Vector3 _backgroundScale;
    private Vector2 _backgroundSize;
    private Vector2 _backgroundOffset;
    private float _nextTargetSearch;

    private void Awake() {
        _camera = GetComponent<Camera>();
        _startingY = transform.position.y;
        _startingSize = _camera.orthographicSize;
        var background = transform.Find("Background");
        if (background != null && background.TryGetComponent(out _background)) {
            _backgroundScale = background.localScale;
            _backgroundSize = _background.bounds.size;
            _backgroundOffset = _background.bounds.center - transform.position;
        }
    }

    public void Follow(Transform target) {
        _target = target;
    }

    public void FollowTogether(Transform first, Transform second) {
        _target = first;
        _secondTarget = second;
        _followTogether = true;
    }

    public void Spectate() {
        _isSpectating = true;
        _target = null;
        _nextTargetSearch = 0f;
    }

    private void LateUpdate() {
        if (_isSpectating && (_target == null || !_target.gameObject.activeInHierarchy)
            && Time.unscaledTime >= _nextTargetSearch) {
            _nextTargetSearch = Time.unscaledTime + 0.2f;
            var players = FindObjectsOfType<PlayerMovement>();
            if (players.Length > 0) {
                Follow(players[Random.Range(0, players.Length)].transform);
            }
        }

        var position = transform.position;
        var target = _target != null && _target.gameObject.activeInHierarchy ? _target : _secondTarget;
        var targetX = target != null ? target.position.x : position.x;
        var targetY = target != null ? Mathf.Max(_startingY, target.position.y - _verticalLead) : position.y;
        if (_followTogether) {
            var zoom = _startingSize;
            if (target == _target && target != null && _secondTarget != null && _secondTarget.gameObject.activeInHierarchy) {
                var center = (target.position + _secondTarget.position) * 0.5f;
                var separation = target.position - _secondTarget.position;
                targetX = center.x;
                targetY = Mathf.Max(_startingY, center.y - _verticalLead);
                zoom = Mathf.Max(_startingSize, (Mathf.Abs(separation.x) * 0.5f + 1.5f) / _camera.aspect,
                    Mathf.Abs(separation.y) * 0.5f + 1.5f);
                targetY = Mathf.Min(targetY, Mathf.Min(target.position.y, _secondTarget.position.y) + zoom - 0.75f);
                targetY = Mathf.Max(targetY, Mathf.Max(target.position.y, _secondTarget.position.y) - zoom + 0.75f);
            }
            _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, zoom, ref _zoomVelocity, _followTime);
            ResizeBackground();
        }
        var bounds = _cameraBounds.bounds;
        var halfHeight = _camera.orthographicSize;
        var halfWidth = halfHeight * _camera.aspect;
        position.x = FollowAxis(position.x, targetX, bounds.min.x, bounds.max.x, halfWidth, ref _followVelocity.x);
        position.y = FollowAxis(position.y, targetY, bounds.min.y, bounds.max.y, halfHeight, ref _followVelocity.y);
        transform.position = position;
    }

    private void ResizeBackground() {
        if (_background == null || _backgroundSize.x <= 0f || _backgroundSize.y <= 0f) {
            return;
        }

        var halfHeight = _camera.orthographicSize;
        var horizontal = 2f * (halfHeight * _camera.aspect + Mathf.Abs(_backgroundOffset.x)) / _backgroundSize.x;
        var vertical = 2f * (halfHeight + Mathf.Abs(_backgroundOffset.y)) / _backgroundSize.y;
        _background.transform.localScale = _backgroundScale * Mathf.Max(1f, horizontal, vertical);
    }

    private float FollowAxis(float current, float target, float min, float max, float halfView, ref float velocity) {
        if (max - min <= 2f * halfView) {
            return (min + max) * 0.5f;
        }

        var lower = min + halfView;
        var upper = max - halfView;
        var position = Mathf.SmoothDamp(current, Mathf.Clamp(target, lower, upper), ref velocity, _followTime);
        return Mathf.Clamp(position, lower, upper);
    }
}
