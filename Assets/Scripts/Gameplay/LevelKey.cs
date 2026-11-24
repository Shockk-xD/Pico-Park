using DG.Tweening;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LevelKey : MonoBehaviour {
    private enum AnimationType {
        Random,
        RotateY,
        Float,
        Pulse
    }

    [SerializeField] private LevelExit _exit;
    [SerializeField] private Transform _visual;
    [SerializeField] private AnimationType _animationType;

    private Tween _animation;

    public ThrowableKey Throwable { get; private set; }

    private void Awake() {
        Throwable = GetComponent<ThrowableKey>();
    }

    private void Start() {
        var animationType = _animationType == AnimationType.Random ? ChooseAnimation() : _animationType;

        switch (animationType) {
            case AnimationType.RotateY:
                _animation = _visual.DOLocalRotate(new Vector3(0f, 360f, 0f), 2f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
                break;
            case AnimationType.Float:
                _animation = _visual.DOLocalMoveY(_visual.localPosition.y + 0.25f, 0.75f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
            case AnimationType.Pulse:
                _animation = _visual.DOScale(_visual.localScale * 1.2f, 0.65f)
                    .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
                break;
        }
    }

    private AnimationType ChooseAnimation() {
        var count = System.Enum.GetValues(typeof(AnimationType)).Length;
        var room = PhotonNetwork.CurrentRoom;
        if (room != null && room.CustomProperties.TryGetValue(RoomManager.MatchSeedProperty, out var value)
            && value is int seed) {
            return (AnimationType)new System.Random(seed ^ gameObject.scene.buildIndex).Next(1, count);
        }

        return (AnimationType)Random.Range(1, count);
    }

    private void OnDisable() {
        _animation?.Kill();
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (Throwable == null && other.TryGetComponent<PlayerMovement>(out var player) && player.IsLocalPlayer) {
            _exit.PickUpKey();
        }
    }
}
