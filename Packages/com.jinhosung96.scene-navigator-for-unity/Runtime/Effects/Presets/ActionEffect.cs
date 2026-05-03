using System;
#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    /// <summary>Adapter that turns two delegates into an ITransitionEffect. Not [Serializable] because delegates cannot be inspected.</summary>
    public sealed class ActionEffect : ITransitionEffect
    {
#if UNITASK_SUPPORT
        public Func<TransitionContext, UniTask> Out;
        public Func<TransitionContext, UniTask> In;

        public ActionEffect(Func<TransitionContext, UniTask> playOut, Func<TransitionContext, UniTask> playIn)
        {
            Out = playOut; In = playIn;
        }

        public UniTask PlayOut(TransitionContext c) => Out != null ? Out(c) : UniTask.CompletedTask;
        public UniTask PlayIn (TransitionContext c) => In  != null ? In (c) : UniTask.CompletedTask;
#else
        public Func<TransitionContext, Task> Out;
        public Func<TransitionContext, Task> In;

        public ActionEffect(Func<TransitionContext, Task> playOut, Func<TransitionContext, Task> playIn)
        {
            Out = playOut; In = playIn;
        }

        public Task PlayOut(TransitionContext c) => Out != null ? Out(c) : Task.CompletedTask;
        public Task PlayIn (TransitionContext c) => In  != null ? In (c) : Task.CompletedTask;
#endif
    }
}
