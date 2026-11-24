using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PhotonView))]
public class JumpBoostBlock : MonoBehaviour {
    [SerializeField, Min(1f)] private float _jumpMultiplier = 1.5f;
    [SerializeField] private GameObject _defaultSprite;
    [SerializeField] private GameObject _boostedSprite;
    [SerializeField, Min(0.01f)] private float _boostedDuration = 0.2f;

    private PhotonView _photonView;
    private float _boostedUntil;

    public float JumpMultiplier => Mathf.Max(1f, _jumpMultiplier);

    private void Awake() {
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable() {
        _boostedUntil = 0f;
        SetBoostedSprite(false);
    }

    private void Update() {
        if (_boostedUntil > 0f && Time.time >= _boostedUntil) {
            _boostedUntil = 0f;
            SetBoostedSprite(false);
        }
    }

    public void PlayBoost() {
        ShowBoostEffect();
        if (PhotonNetwork.InRoom && _photonView.ViewID != 0) {
            _photonView.RPC(nameof(ShowBoostEffect), RpcTarget.Others);
        }
    }

    [PunRPC]
    private void ShowBoostEffect() {
        _boostedUntil = Time.time + _boostedDuration;
        SetBoostedSprite(true);
        GameAudio.Play(GameAudio.Cue.BoostJump);
    }

    private void SetBoostedSprite(bool isBoosted) {
        if (_defaultSprite != null) {
            _defaultSprite.SetActive(!isBoosted || _boostedSprite == null);
        }

        if (_boostedSprite != null) {
            _boostedSprite.SetActive(isBoosted);
        }
    }
}
