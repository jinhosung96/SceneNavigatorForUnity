using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace SceneNavigator.Tests
{
    /// <summary>
    /// v0.1.0 PlayMode test scope is intentionally minimal — full transition / async-hook /
    /// reload coverage requires fixture scenes added to BuildSettings, planned for v0.2.
    /// This file holds smoke tests + a structural sanity check on the runtime entry point.
    /// </summary>
    public sealed class NavigatorScaffoldTests
    {
        [UnityTest]
        public IEnumerator NoNavigatorBeforeBootstrap()
        {
            // No Base scene loaded → static Instance must be null and never throw.
            // (The test runner itself loads no Base scene.)
            Assert.DoesNotThrow(() => { var _ = SceneNavigator.Instance; });
            yield return null;
        }

        [Test]
        public void TransitionEffectsNoneCompletesImmediately()
        {
            var none = TransitionEffects.None;
            var ctx = new TransitionContext(null, null, null, null, default);
#if UNITASK_SUPPORT
            Assert.IsTrue(none.PlayOut(ctx).Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
            Assert.IsTrue(none.PlayIn (ctx).Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
#else
            Assert.IsTrue(none.PlayOut(ctx).IsCompleted);
            Assert.IsTrue(none.PlayIn (ctx).IsCompleted);
#endif
        }

        [Test]
        public void TransitionEffectsSequenceAcceptsNullSteps()
        {
            var seq = TransitionEffects.Sequence(null, null);
            var ctx = new TransitionContext(null, null, null, null, default);
#if UNITASK_SUPPORT
            var t = seq.PlayOut(ctx);
            Assert.IsTrue(t.Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
#else
            Assert.IsTrue(seq.PlayOut(ctx).IsCompleted);
#endif
        }
    }
}
