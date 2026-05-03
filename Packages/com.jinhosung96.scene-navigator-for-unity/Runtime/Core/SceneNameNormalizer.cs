namespace SceneNavigator
{
    /// <summary>Pure function used by editor save hook to enforce GameObject names ("SceneNode - {Kind}").</summary>
    public static class SceneNameNormalizer
    {
        public const string Prefix = "SceneNode - ";

        public static string Normalize(SceneNodeKind kind) => Prefix + kind;

        public static bool IsCorrect(string gameObjectName, SceneNodeKind kind) =>
            !string.IsNullOrEmpty(gameObjectName) && gameObjectName == Normalize(kind);
    }
}
