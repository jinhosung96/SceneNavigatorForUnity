using NUnit.Framework;
using UnityEngine;

namespace SceneNavigator.Tests
{
    public sealed class SceneCatalogIntegrityTests
    {
        [Test]
        public void TryResolveReturnsFalseForUnknownType()
        {
            var catalog = ScriptableObject.CreateInstance<SceneCatalog>();
            try
            {
                Assert.IsFalse(catalog.TryResolve(typeof(SceneCatalogIntegrityTests), out var entry));
                Assert.IsNull(entry);
            }
            finally { Object.DestroyImmediate(catalog); }
        }

        [Test]
        public void TryResolveReturnsRegisteredEntry()
        {
            var catalog = ScriptableObject.CreateInstance<SceneCatalog>();
            try
            {
                catalog.EditorEntries.Add(new SceneCatalog.Entry
                {
                    typeAssemblyQualifiedName = typeof(SceneCatalogIntegrityTests).AssemblyQualifiedName,
                    kind = SceneNodeKind.Main,
                    scenePath = "Assets/Foo.unity",
                });
                Assert.IsTrue(catalog.TryResolve(typeof(SceneCatalogIntegrityTests), out var entry));
                Assert.IsNotNull(entry);
                Assert.AreEqual("Assets/Foo.unity", entry.scenePath);
                Assert.AreEqual(SceneNodeKind.Main, entry.kind);
            }
            finally { Object.DestroyImmediate(catalog); }
        }

        [Test]
        public void TryResolveNullTypeReturnsFalse()
        {
            var catalog = ScriptableObject.CreateInstance<SceneCatalog>();
            try
            {
                Assert.IsFalse(catalog.TryResolve(null, out var entry));
                Assert.IsNull(entry);
            }
            finally { Object.DestroyImmediate(catalog); }
        }
    }
}
