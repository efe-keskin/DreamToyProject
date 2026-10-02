using DG.Tweening;
using UnityEngine;

public static class DOTweenSetup
{
    private const int TweenerCapacity = 2000;
    private const int SequenceCapacity = 250;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Configure()
    {
        DOTween.Init().SetCapacity(TweenerCapacity, SequenceCapacity);
    }
}
