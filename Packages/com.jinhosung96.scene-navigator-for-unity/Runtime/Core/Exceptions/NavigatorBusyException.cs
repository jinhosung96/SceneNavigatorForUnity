using System;

namespace SceneNavigator
{
    public sealed class NavigatorBusyException : InvalidOperationException
    {
        public NavigatorBusyException()
            : base("SceneNavigator is already processing a transition. Concurrent navigation calls are rejected by design.") { }

        public NavigatorBusyException(string message) : base(message) { }
    }
}
