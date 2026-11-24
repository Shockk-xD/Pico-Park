using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class SymbolSequence : MonoBehaviour, IPunObservable {
    [SerializeField] private string[] _sequence;
    [SerializeField] private TMP_Text _clueText;
    [SerializeField] private UnityEvent _onSolved;
    [SerializeField] private UnityEvent _onMistake;

    private PhotonView _view;
    private int _progress;
    private int _mistakes;
    private bool _isSolved;

    private void Awake() {
        _view = GetComponent<PhotonView>();
        _view.Synchronization = ViewSynchronization.UnreliableOnChange;
        _view.FindObservables(true);
        if (_clueText != null && _sequence != null) {
            _clueText.text = string.Join("   ", _sequence);
        }
    }

    public void Submit(string symbol) {
        if (!PhotonNetwork.InRoom || _view.IsMine) {
            SubmitOnHost(symbol);
        } else {
            _view.RPC(nameof(SubmitOnHost), RpcTarget.MasterClient, symbol);
        }
    }

    [PunRPC]
    private void SubmitOnHost(string symbol) {
        if ((PhotonNetwork.InRoom && !_view.IsMine) || _isSolved || _sequence is not { Length: > 0 }) {
            return;
        }

        if (symbol == _sequence[_progress]) {
            _progress++;
            if (_progress == _sequence.Length) {
                Solve();
            }
        } else {
            _progress = 0;
            _mistakes++;
            _onMistake?.Invoke();
        }
    }

    private void Solve() {
        if (!_isSolved) {
            _isSolved = true;
            _onSolved?.Invoke();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(_progress);
            stream.SendNext(_mistakes);
            stream.SendNext(_isSolved);
        } else {
            _progress = (int)stream.ReceiveNext();
            var mistakes = (int)stream.ReceiveNext();
            if (_mistakes != mistakes) {
                _mistakes = mistakes;
                _onMistake?.Invoke();
            }
            if ((bool)stream.ReceiveNext()) {
                Solve();
            }
        }
    }
}
