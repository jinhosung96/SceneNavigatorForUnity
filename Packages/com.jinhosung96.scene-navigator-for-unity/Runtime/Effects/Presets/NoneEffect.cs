using System;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    [Serializable]
    public sealed class NoneEffect : ITransitionEffect
    {
#if UNITASK_SUPPORT
        public UniTask PlayOut(TransitionContext context) => UniTask.CompletedTask;
        public UniTask PlayIn (TransitionContext context) => UniTask.CompletedTask;
#else
        public Task PlayOut(TransitionContext context) => Task.CompletedTask;
        public Task PlayIn (TransitionContext context) => Task.CompletedTask;
#endif
    }
}
