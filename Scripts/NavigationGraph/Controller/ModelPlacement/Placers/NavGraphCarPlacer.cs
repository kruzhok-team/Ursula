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
    public class NavGraphCarPlacer
    {
        private GameObjectCollectionModel gameObjectCollectionModel;
        private GameObjectCreateItemsModel gameObjectCreateItemsModel;

        private float modelRadius;
        private Dictionary<NavGraphEdge, int> edgeAssignments = new Dictionary<NavGraphEdge, int>();

        public NavGraphCarPlacer(GameObjectCollectionModel gameObjectCollectionModel, GameObjectCreateItemsModel gameObjectCreateItemsModel, float scaledModelRadius)
        {
            this.gameObjectCollectionModel = gameObjectCollectionModel;
            this.gameObjectCreateItemsModel = gameObjectCreateItemsModel;
            this.modelRadius = scaledModelRadius;
        }

        public void AssignEdgesUniform(NavGraph navGraph, int carCount, RandomNumberGenerator rng)
        {
            int maxTries = carCount * 2;
            int carsPlaced = 0;
            int tries = 0;

            while (tries < maxTries && carsPlaced < carCount)
            {
                int edgeIdx = rng.RandiRange(0, navGraph.edges.Count - 1);
                NavGraphEdge edge = navGraph.edges[edgeIdx];
                if (AssignEdge(edge))
                    carsPlaced += 1;
                tries += 1;
            }
        }

        public bool AssignEdge(NavGraphEdge edge)
        {
            float edgeLength = edge.v2.position.DistanceTo(edge.v1.position);
            int edgeCapacity = (int)(edgeLength / modelRadius / 2);
            bool exists = edgeAssignments.TryGetValue(edge, out int edgeAssignment);
            if (!exists)
                edgeAssignment = 0;
            if (exists && edgeAssignments[edge] >= edgeCapacity)
                return false;
            edgeAssignments[edge] = edgeAssignment + 1;
            return true;
        }

        public bool PlaceCars(GameObjectAssetInfo car, NavGraphEdge edge, float scale, float modelHegihtOffset)
        {
            int carCount;
            if (!edgeAssignments.TryGetValue(edge, out carCount))
                return false;

            Vector3 edgeDir = edge.v2.position - edge.v1.position;
            float edgeLength = edgeDir.Length();
            edgeDir = edgeDir / edgeLength;

            float t = edgeLength / (carCount + 1);
            Vector3 placementDelta = edgeDir * t;

            for (int i = 1; i < carCount + 1; i++)
            {
                gameObjectCollectionModel.SetGameObjectAssetSelected(car);

                Vector3 pos = edge.v1.position + placementDelta * i + Vector3.Up * modelHegihtOffset;

                gameObjectCreateItemsModel.SetGameObjectCreateItem(pos, scale, NavGraphPlacerUtils.EncodeDirection(edgeDir));
            }
            return true;
        }
    }
}
