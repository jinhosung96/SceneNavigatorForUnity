using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>Crossfade through a full-screen color overlay. PlayOut fades In, PlayIn fades Out.</summary>
    [Serializable]
    public sealed class FadeEffect : ITransitionEffect
    {
        public float DurationOut;
        public float DurationIn;
        public Color Color;

        public FadeEffect() : this(0.3f, 0.3f, Color.black) { }
        public FadeEffect(float duration) : this(duration, duration, Color.black) { }
        public FadeEffect(float duration, Color color) : this(duration, duration, color) { }
        public FadeEffect(float durationOut, float durationIn, Color color)
        {
            DurationOut = durationOut;
            DurationIn  = durationIn;
            Color = color;
        }

#if UNITASK_SUPPORT
        public UniTask PlayOut(TransitionContext context) =>
            FadeUtility.Run(context, Color, 0f, 1f, DurationOut);

        public UniTask PlayIn(TransitionContext context) =>
            FadeUtility.Run(context, Color, 1f, 0f, DurationIn);
#else
        public Task PlayOut(TransitionContext context) =>
            FadeUtility.Run(context, Color, 0f, 1f, DurationOut);

        public Task PlayIn(TransitionContext context) =>
            FadeUtility.Run(context, Color, 1f, 0f, DurationIn);
#endif
    }

    [Serializable]
    public sealed class FadeOutEffect : ITransitionEffect
    {
        public float Duration;
        public Color Color;

        public FadeOutEffect() : this(0.3f, Color.black) { }
        public FadeOutEffect(float duration) : this(duration, Color.black) { }
        public FadeOutEffect(float duration, Color color) { Duration = duration; Color = color; }

#if UNITASK_SUPPORT
        public UniTask PlayOut(TransitionContext context) =>
            FadeUtility.Run(context, Color, 0f, 1f, Duration);
        public UniTask PlayIn(TransitionContext context) => UniTask.CompletedTask;
#else
        public Task PlayOut(TransitionContext context) =>
            FadeUtility.Run(context, Color, 0f, 1f, Duration);
        public Task PlayIn(TransitionContext context) => Task.CompletedTask;
#endif
    }

    [Serializable]
    public sealed class FadeInEffect : ITransitionEffect
    {
        public float Duration;
        public Color Color;

        public FadeInEffect() : this(0.3f, Color.black) { }
        public FadeInEffect(float duration) : this(duration, Color.black) { }
        public FadeInEffect(float duration, Color color) { Duration = duration; Color = color; }

#if UNITASK_SUPPORT
        public UniTask PlayOut(TransitionContext context) => UniTask.CompletedTask;
        public UniTask PlayIn (TransitionContext context) =>
            FadeUtility.Run(context, Color, 1f, 0f, Duration);
#else
        public Task PlayOut(TransitionContext context) => Task.CompletedTask;
        public Task PlayIn (TransitionContext context) =>
            FadeUtility.Run(context, Color, 1f, 0f, Duration);
#endif
    }

    internal static class FadeUtility
    {
#if UNITASK_SUPPORT
        public static async UniTask Run(TransitionContext ctx, Color color, float fromAlpha, float toAlpha, float duration)
#else
        public static async Task Run(TransitionContext ctx, Color color, float fromAlpha, float toAlpha, float duration)
#endif
        {
            var root = ctx.OverlayRoot;
            if (root == null || duration <= 0f) return;

            var go = new GameObject("[Fade]", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(root, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var image = go.GetComponent<Image>();
            image.raycastTarget = true;
            image.color = new Color(color.r, color.g, color.b, fromAlpha);

            try
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    if (ctx.Cancellation.IsCancellationRequested) break;
                    elapsed += Time.unscaledDeltaTime;
                    var t = Mathf.Clamp01(elapsed / duration);
                    var a = Mathf.Lerp(fromAlpha, toAlpha, t);
                    image.color = new Color(color.r, color.g, color.b, a);
#if UNITASK_SUPPORT
                    await UniTask.Yield(PlayerLoopTiming.Update, ctx.Cancellation);
#else
                    await Task.Yield();
#endif
                }
                image.color = new Color(color.r, color.g, color.b, toAlpha);
            }
            finally
            {
                if (go != null) UnityEngine.Object.Destroy(go);
            }
        }
    }
}
