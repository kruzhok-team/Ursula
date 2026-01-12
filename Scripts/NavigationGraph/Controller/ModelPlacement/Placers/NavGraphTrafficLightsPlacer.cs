using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Ursula.GameObjects.Model;
using Godot;
using System;
using Ursula.MapManagers.Setters;
using System.Threading;
using Fractural.Tasks;
using bearloga.addons.Ursula.Modules.LogicInjector;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers
{
    public class NavGraphTrafficLightsPlacer
    {
        private GameObjectCollectionModel gameObjectCollectionModel;
        private GameObjectCreateItemsModel gameObjectCreateItemsModel;
        private MapManagerItemSetter mapManagerItemSetter;

        public NavGraphTrafficLightsPlacer(GameObjectCollectionModel gameObjectCollectionModel, GameObjectCreateItemsModel gameObjectCreateItemsModel, MapManagerItemSetter mapManagerItemSetter)
        {
            this.gameObjectCollectionModel = gameObjectCollectionModel;
            this.gameObjectCreateItemsModel = gameObjectCreateItemsModel;
            this.mapManagerItemSetter = mapManagerItemSetter;
        }

        public Node PlaceTrafficLights(GameObjectAssetInfo trafficLights, NavGraphEdge edge, float scale, Vector3 offset, Injector injector)
        {
            if (edge.v2.shedule == null)
                throw new ArgumentException($"Edge second vertex should be containing shedule");

            gameObjectCollectionModel.SetGameObjectAssetSelected(trafficLights);
            
            Vector3 dir = (edge.v2.position - edge.v1.position).Normalized();
            Quaternion rotation = new Quaternion(Vector3.Forward, dir);
            offset = rotation * offset;
            Vector3 pos = edge.v2.position + offset;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, NavGraphPlacerUtils.EncodeDirection(-dir), false);

            Vector3 positionNode = gameObjectCreateItemsModel.PositionNode;
            float scaleNode = gameObjectCreateItemsModel.ScaleNode;
            byte rotationNode = gameObjectCreateItemsModel.RotationNode;

            int _x = Mathf.RoundToInt(positionNode.X);
            int _y = Mathf.RoundToInt(positionNode.Y);
            int _z = Mathf.RoundToInt(positionNode.Z);

            int id = _x + _y * 256 + _z * 256 * 256;

            Node item = mapManagerItemSetter.CreateGameItem(trafficLights, rotationNode, scaleNode, positionNode.X, positionNode.Y, positionNode.Z, 0, id, false, injector: injector);
            //InjectShedule(item, edge.v2.shedule);
            return item;
        }

        public void InjectShedule(Node trafficLight, NavGraphVertexShedule shedule)
        {
            Node nodes = trafficLight.FindChild("InteractiveObject", true, true);
            InteractiveObject io = nodes as InteractiveObject;
            CyberiadaLogic logic = io.hsmLogic;
        }
    }
}
