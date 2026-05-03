using System;
using UnityEngine;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>Plays inner effects' PlayOut in parallel, then PlayIn in parallel.</summary>
    [Serializable]
    public sealed class ParallelEffect : ITransitionEffect
    {
        [SerializeReference] public ITransitionEffect[] Steps;

        public ParallelEffect() { }
        public ParallelEffect(params ITransitionEffect[] steps) { Steps = steps; }

#if UNITASK_SUPPORT
        public async UniTask PlayOut(TransitionContext context)
        {
            if (Steps == null || Steps.Length == 0) return;
            var tasks = new System.Collections.Generic.List<UniTask>(Steps.Length);
            for (int i = 0; i < Steps.Length; i++)
                if (Steps[i] != null) tasks.Add(Steps[i].PlayOut(context));
            if (tasks.Count > 0) await UniTask.WhenAll(tasks);
        }

        public async UniTask PlayIn(TransitionContext context)
        {
            if (Steps == null || Steps.Length == 0) return;
            var tasks = new System.Collections.Generic.List<UniTask>(Steps.Length);
            for (int i = 0; i < Steps.Length; i++)
                if (Steps[i] != null) tasks.Add(Steps[i].PlayIn(context));
            if (tasks.Count > 0) await UniTask.WhenAll(tasks);
        }
#else
        public async Task PlayOut(TransitionContext context)
        {
            if (Steps == null || Steps.Length == 0) return;
            var tasks = new System.Collections.Generic.List<Task>(Steps.Length);
            for (int i = 0; i < Steps.Length; i++)
                if (Steps[i] != null) tasks.Add(Steps[i].PlayOut(context));
            if (tasks.Count > 0) await Task.WhenAll(tasks);
        }

        public async Task PlayIn(TransitionContext context)
        {
            if (Steps == null || Steps.Length == 0) return;
            var tasks = new System.Collections.Generic.List<Task>(Steps.Length);
            for (int i = 0; i < Steps.Length; i++)
                if (Steps[i] != null) tasks.Add(Steps[i].PlayIn(context));
            if (tasks.Count > 0) await Task.WhenAll(tasks);
        }
#endif
    }
}
