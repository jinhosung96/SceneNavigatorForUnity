using NUnit.Framework;

namespace SceneNavigator.Tests
{
    public sealed class SceneNameNormalizerTests
    {
        [Test]
        public void NormalizeProducesExpectedNames()
        {
            Assert.AreEqual("SceneNode - Base",     SceneNameNormalizer.Normalize(SceneNodeKind.Base));
            Assert.AreEqual("SceneNode - Main",     SceneNameNormalizer.Normalize(SceneNodeKind.Main));
            Assert.AreEqual("SceneNode - Sub",      SceneNameNormalizer.Normalize(SceneNodeKind.Sub));
            Assert.AreEqual("SceneNode - Instance", SceneNameNormalizer.Normalize(SceneNodeKind.Instance));
        }

        [Test]
        public void IsCorrectMatchesNormalize()
        {
            Assert.IsTrue (SceneNameNormalizer.IsCorrect("SceneNode - Main", SceneNodeKind.Main));
            Assert.IsFalse(SceneNameNormalizer.IsCorrect("SceneNode - main", SceneNodeKind.Main));
            Assert.IsFalse(SceneNameNormalizer.IsCorrect("scenenode - Main", SceneNodeKind.Main));
            Assert.IsFalse(SceneNameNormalizer.IsCorrect("",                 SceneNodeKind.Main));
            Assert.IsFalse(SceneNameNormalizer.IsCorrect(null,               SceneNodeKind.Main));
            Assert.IsFalse(SceneNameNormalizer.IsCorrect("SceneNode - Main", SceneNodeKind.Sub));
        }
    }
}
