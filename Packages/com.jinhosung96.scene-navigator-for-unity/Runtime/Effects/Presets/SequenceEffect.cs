using System;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>Plays inner effects' PlayOut in order, then PlayIn in order.</summary>
    [Serializable]
    public sealed class SequenceEffect : ITransitionEffect
    {
        [SerializeReference] public ITransitionEffect[] Steps;

        public SequenceEffect() { }
        public SequenceEffect(params ITransitionEffect[] steps) { Steps = steps; }

#if UNITASK_SUPPORT
        public async UniTask PlayOut(TransitionContext context)
        {
            if (Steps == null) return;
            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] != null) await Steps[i].PlayOut(context);
            }
        }

        public async UniTask PlayIn(TransitionContext context)
        {
            if (Steps == null) return;
            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] != null) await Steps[i].PlayIn(context);
            }
        }
#else
        public async Task PlayOut(TransitionContext context)
        {
            if (Steps == null) return;
            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] != null) await Steps[i].PlayOut(context);
            }
        }

        public async Task PlayIn(TransitionContext context)
        {
            if (Steps == null) return;
            for (int i = 0; i < Steps.Length; i++)
            {
                if (Steps[i] != null) await Steps[i].PlayIn(context);
            }
        }
#endif
    }
}
