using System;
using System.IO;
using UnityEngine.SceneManagement;

namespace SceneNavigator
{
    public sealed class SceneNodeData
    {
        public Type NodeType { get; }
        public SceneNodeKind Kind { get; }
        public string ScenePath { get; }
        public string SceneName { get; }

        public int BuildIndex => SceneUtility.GetBuildIndexByScenePath(ScenePath);

        public SceneNode Node { get; internal set; }
        public bool IsAlive => Node != null;

        public Scene? UnityScene { get; internal set; }

        public SceneNodeData(Type nodeType, SceneNodeKind kind, string scenePath)
        {
            NodeType = nodeType ?? throw new ArgumentNullException(nameof(nodeType));
            Kind = kind;
            ScenePath = scenePath ?? throw new ArgumentNullException(nameof(scenePath));
            SceneName = Path.GetFileNameWithoutExtension(scenePath);
        }

        public override string ToString() =>
            $"SceneNodeData({Kind} {NodeType.Name} -> {SceneName}, alive={IsAlive})";
    }
}
