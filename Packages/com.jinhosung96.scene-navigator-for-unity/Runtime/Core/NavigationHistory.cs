using System.Collections.Generic;

namespace SceneNavigator
{
    public sealed class NavigationHistory
    {
        private readonly List<SceneNodeData> _stack = new List<SceneNodeData>();

        public IReadOnlyList<SceneNodeData> Items => _stack;
        public int Count => _stack.Count;
        public bool IsEmpty => _stack.Count == 0;

        public void Push(SceneNodeData data)
        {
            if (data == null) return;
            _stack.Add(data);
        }

        public SceneNodeData Pop()
        {
            if (_stack.Count == 0) return null;
            var top = _stack[_stack.Count - 1];
            _stack.RemoveAt(_stack.Count - 1);
            return top;
        }

        public SceneNodeData Peek() =>
            _stack.Count == 0 ? null : _stack[_stack.Count - 1];

        public void Clear() => _stack.Clear();
    }
}
