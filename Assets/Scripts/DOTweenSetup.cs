using DG.Tweening;
using UnityEngine;

public static class DOTweenSetup
{
    private const int TweenerCapacity = 500;
    private const int SequenceCapacity = 150;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        DOTween.Init().SetCapacity(TweenerCapacity, SequenceCapacity);
    }
}
