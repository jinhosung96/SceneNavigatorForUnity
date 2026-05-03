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
    ///
    /// We deliberately avoid constructing TransitionContext here so this test assembly does
    /// not need a direct reference to R3 (TransitionContext's ctor signature changes between
    /// the R3 / non-R3 branches).
    /// </summary>
    public sealed class NavigatorScaffoldTests
    {
        [UnityTest]
        public IEnumerator NoNavigatorBeforeBootstrap()
        {
            // No Base scene loaded → static Instance must be null and never throw.
            Assert.DoesNotThrow(() => { var _ = SceneNavigator.Instance; });
            yield return null;
        }

        [Test]
        public void TransitionEffectsNoneCompletesImmediately()
        {
            var none = TransitionEffects.None;
            var t1 = none.PlayOut(null);
            var t2 = none.PlayIn(null);
#if UNITASK_SUPPORT
            Assert.IsTrue(t1.Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
            Assert.IsTrue(t2.Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
#else
            Assert.IsTrue(t1.IsCompleted);
            Assert.IsTrue(t2.IsCompleted);
#endif
        }

        [Test]
        public void TransitionEffectsSequenceAcceptsNullSteps()
        {
            var seq = TransitionEffects.Sequence(null, null);
            var t = seq.PlayOut(null);
#if UNITASK_SUPPORT
            Assert.IsTrue(t.Status == Cysharp.Threading.Tasks.UniTaskStatus.Succeeded);
#else
            Assert.IsTrue(t.IsCompleted);
#endif
        }
    }
}
