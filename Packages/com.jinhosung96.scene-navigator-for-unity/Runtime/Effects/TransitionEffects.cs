using System;
using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>Static factory of common transition effect presets.</summary>
    public static class TransitionEffects
    {
        public static ITransitionEffect None { get; } = new NoneEffect();

        public static ITransitionEffect Fade(float duration = 0.3f) =>
            new FadeEffect(duration);

        public static ITransitionEffect Fade(float duration, Color color) =>
            new FadeEffect(duration, color);

        public static ITransitionEffect FadeOut(float duration = 0.3f) =>
            new FadeOutEffect(duration);

        public static ITransitionEffect FadeOut(float duration, Color color) =>
            new FadeOutEffect(duration, color);

        public static ITransitionEffect FadeIn(float duration = 0.3f) =>
            new FadeInEffect(duration);

        public static ITransitionEffect FadeIn(float duration, Color color) =>
            new FadeInEffect(duration, color);

        public static ITransitionEffect Sequence(params ITransitionEffect[] steps) =>
            new SequenceEffect(steps);

        public static ITransitionEffect Parallel(params ITransitionEffect[] steps) =>
            new ParallelEffect(steps);

#if UNITASK_SUPPORT
        public static ITransitionEffect FromAction(
            Func<TransitionContext, UniTask> playOut,
            Func<TransitionContext, UniTask> playIn) => new ActionEffect(playOut, playIn);
#else
        public static ITransitionEffect FromAction(
            Func<TransitionContext, Task> playOut,
            Func<TransitionContext, Task> playIn) => new ActionEffect(playOut, playIn);
#endif
    }
}
