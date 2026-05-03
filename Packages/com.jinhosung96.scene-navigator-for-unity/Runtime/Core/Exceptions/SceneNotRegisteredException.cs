using System;

namespace SceneNavigator
{
    public sealed class SceneNotRegisteredException : InvalidOperationException
    {
        public Type NodeType { get; }

        public SceneNotRegisteredException(Type nodeType)
            : base($"SceneNode type '{nodeType?.FullName}' is not registered in the SceneCatalog. " +
                   "Attach a SceneNode component to a scene and run 'Tools/Scene Navigator/Rebuild Catalog'.")
        {
            NodeType = nodeType;
        }
    }
}
