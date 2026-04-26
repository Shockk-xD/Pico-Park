using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneLoadingView 
{
    private readonly Image _blockoutImage;
    private readonly TMP_Text _loadingText;

    private const float _doColorDuration = 0.75f;
    private const float _doScaleDuration = 1.5f;
    private const float _fadeOutScaleValue = 0;
    private const float _fadeInScaleValue = 100;
    private const float _textFadeDuration = 0.5f;

    public SceneLoadingView(Image blockoutImage, TMP_Text loadingText) {
        _blockoutImage = blockoutImage;
        _loadingText = loadingText;
    }

    public async UniTask FadeIn() {
        Sequence animationSequence = DOTween.Sequence();

        var doColorTween = _blockoutImage.DOColor(Color.black, _doColorDuration).From(Color.white);
        var doScaleTween = _blockoutImage.transform.DOScale(_fadeInScaleValue, _doScaleDuration).From(_fadeOutScaleValue);
        var doFadeTween = _loadingText.DOFade(1, _textFadeDuration).From(0);

        animationSequence.Append(doColorTween);
        animationSequence.Join(doScaleTween);
        animationSequence.Join(doFadeTween);

        await animationSequence.AsyncWaitForCompletion();
    }

    public async UniTask FadeOut() {
        Sequence animationSequence = DOTween.Sequence();

        var doColorTween = _blockoutImage.DOColor(Color.white, _doColorDuration).From(Color.black);
        var doScaleTween = _blockoutImage.transform.DOScale(_fadeOutScaleValue, _doScaleDuration * 0.5f).From(_fadeInScaleValue);
        var doFadeTween = _loadingText.DOFade(0, _textFadeDuration * 2).From(1);

        animationSequence.Append(doColorTween);
        animationSequence.Join(doScaleTween);
        animationSequence.Join(doFadeTween);

        await animationSequence.AsyncWaitForCompletion();
    }
}
