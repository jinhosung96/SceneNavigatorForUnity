using NUnit.Framework;

namespace SceneNavigator.Tests
{
    public sealed class NavigationHistoryTests
    {
        [Test]
        public void EmptyByDefault()
        {
            var h = new NavigationHistory();
            Assert.IsTrue(h.IsEmpty);
            Assert.AreEqual(0, h.Count);
            Assert.IsNull(h.Pop());
            Assert.IsNull(h.Peek());
        }

        [Test]
        public void PushPopOrder()
        {
            var h = new NavigationHistory();
            var a = MakeData("A");
            var b = MakeData("B");
            h.Push(a);
            h.Push(b);
            Assert.AreEqual(2, h.Count);
            Assert.AreSame(b, h.Peek());
            Assert.AreSame(b, h.Pop());
            Assert.AreSame(a, h.Pop());
            Assert.IsTrue(h.IsEmpty);
        }

        [Test]
        public void NullPushIgnored()
        {
            var h = new NavigationHistory();
            h.Push(null);
            Assert.IsTrue(h.IsEmpty);
        }

        [Test]
        public void ClearWipes()
        {
            var h = new NavigationHistory();
            h.Push(MakeData("A"));
            h.Push(MakeData("B"));
            h.Clear();
            Assert.IsTrue(h.IsEmpty);
        }

        private static SceneNodeData MakeData(string name) =>
            new SceneNodeData(typeof(NavigationHistoryTests), SceneNodeKind.Main, "Assets/Fake/" + name + ".unity");
    }
}
