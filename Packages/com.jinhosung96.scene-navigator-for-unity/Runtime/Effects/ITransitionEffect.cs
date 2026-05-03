#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace SceneNavigator
{
    public interface ITransitionEffect
    {
#if UNITASK_SUPPORT
        UniTask PlayOut(TransitionContext context);
        UniTask PlayIn (TransitionContext context);
#else
        Task PlayOut(TransitionContext context);
        Task PlayIn (TransitionContext context);
#endif
    }
}
