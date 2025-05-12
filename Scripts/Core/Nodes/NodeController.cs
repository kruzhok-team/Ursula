using Godot;

namespace Ursula.Core.View
{
    public class NodeController<T> where T : Node
    {
        public readonly string NodeName;

        private T _cachedNode; // Don't use this field directly!! Use GetNode method instead!

        public NodeController(string nodeName)
        {
            NodeName = nodeName;
        }

        public T GetNode()
        {
            if (_cachedNode == null)
                _cachedNode = FindNode();
            return _cachedNode;
        }

        public SceneTree GetSceneTree()
        {
            return Engine.GetMainLoop() as SceneTree;
        }

        private T FindNode()
        {
            var sceneTree = GetSceneTree();

            if (sceneTree == null)
            {
                GD.PrintErr($"{GetType().Name} Can't extract a SceneTree.");
                return null;
            }

            var node = sceneTree.Root.FindChild(NodeName, recursive: true);
            var result = node as T;

            if (result == null)
                GD.PrintErr($"{GetType().Name} Cant find a node named '{NodeName}'!");

            return result;
        }
    }
}
