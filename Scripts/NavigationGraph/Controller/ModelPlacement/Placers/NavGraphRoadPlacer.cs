using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ursula.GameObjects.Model;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement.Placers
{
    public class NavGraphRoadPlacer
    {
        private GameObjectCollectionModel gameObjectCollectionModel;
        private GameObjectCreateItemsModel gameObjectCreateItemsModel;

        public NavGraphRoadPlacer(GameObjectCollectionModel gameObjectCollectionModel, GameObjectCreateItemsModel gameObjectCreateItemsModel)
        {
            this.gameObjectCollectionModel = gameObjectCollectionModel;
            this.gameObjectCreateItemsModel = gameObjectCreateItemsModel;
        }

        public void PlaceRoadStraightOrTurn(GameObjectAssetInfo roadStraight, GameObjectAssetInfo roadTurn, NavGraphVertex vertex, float roadStraightScale, float roadTurnScale, float heightOffset)
        {
            if (NavGraphPlacerUtils.IsRoadStraight(vertex))
                PlaceRoadStraight(roadStraight, vertex, roadStraightScale, heightOffset);
            else
                PlaceRoadTurn(roadTurn, vertex, roadTurnScale, heightOffset);
        }

        public void PlaceRoadStraight(GameObjectAssetInfo roadStraight, NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadStraight);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge = vertex.edges[0];
            Vector3 dir = edge.v2.position - edge.v1.position;

            if (Mathf.Abs(dir.Z) > Mathf.Abs(dir.X))
                gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, (byte)GameItemRotation.forward);
            else
                gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, (byte)GameItemRotation.right);
        }

        public void PlaceRoadTurn(GameObjectAssetInfo roadTurn, NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadTurn);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge1 = NavGraphPlacerUtils.OrientFromVertex(vertex.edges[0], vertex);
            NavGraphEdge edge2 = NavGraphPlacerUtils.OrientFromVertex(vertex.edges[1], vertex);

            Vector3 dir1 = edge1.v2.position - edge1.v1.position;
            Vector3 dir2 = edge2.v2.position - edge2.v1.position;
            Vector3 normal = dir1.Cross(dir2);

            Vector3 mainDir;
            if (normal.Y < 0)
                mainDir = dir1;
            else
                mainDir = dir2;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, NavGraphPlacerUtils.EncodeDirection(mainDir));
        }

        public void PlaceRoadT(GameObjectAssetInfo roadT, NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadT);

            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            NavGraphEdge edge1 = NavGraphPlacerUtils.OrientFromVertex(vertex.edges[0], vertex);
            NavGraphEdge edge2 = NavGraphPlacerUtils.OrientFromVertex(vertex.edges[1], vertex);
            NavGraphEdge edge3 = NavGraphPlacerUtils.OrientFromVertex(vertex.edges[2], vertex);
            Vector3 dir1 = edge1.v2.position - edge1.v1.position;
            Vector3 dir2 = edge2.v2.position - edge2.v1.position;
            Vector3 dir3 = edge3.v2.position - edge3.v1.position;

            Vector3 mainDir;
            if (Mathf.Abs(dir1.Dot(dir3)) > 0.5f)
                mainDir = dir2;
            else if (Mathf.Abs(dir1.Dot(dir2)) > 0.5f)
                mainDir = dir3;
            else
                mainDir = dir1;

            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, NavGraphPlacerUtils.EncodeDirection(mainDir));
        }

        public void PlaceRoadCross(GameObjectAssetInfo roadCross, NavGraphVertex vertex, float scale, float heightOffset)
        {
            gameObjectCollectionModel.SetGameObjectAssetSelected(roadCross);
            Vector3 pos = vertex.position + Vector3.Up * heightOffset;
            gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, 0);
        }
    }
}
