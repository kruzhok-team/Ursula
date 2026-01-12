using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Generation;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.ModelPlacement;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Visualization;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Model;
using Godot;
using Ursula.Core.DI;
using System;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.PathFinding;
using System.Collections.Generic;
using Fractural.Tasks;
using bearloga.addons.Ursula.Scripts.NavigationGraph.Controller.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace bearloga.addons.Ursula.Scripts.NavigationGraph.Controller
{
    public partial class NavGraphManager : Node, IInjectable
    {
        private NavGraph navGraph;
        private NavGraphVisualization visualization;
        private RandomNumberGenerator rng;

        // Гиперпараметры
        private float delta;
        private int carsCount;
        private float trafficLightsGreenTime;

        // Внутренние параметры симуляции
        private readonly float connectionProbability = 0.6f;
        private readonly float subdivisionOffset = 0.3f;
        private readonly float modelHegihtOffset = 0.1f;

        private bool initialized = false;

        public static NavGraphManager Instance { get; private set; }

        public override void _Ready()
        {
            base._Ready();
            rng = new RandomNumberGenerator();
            Instance = this;
        }

        public override void _Process(double delta)
        {
            base._Process(delta);
            if (visualization != null)
            {
                //visualization.Update(Time.GetTicksMsec() / 1000f);
            }
        }

        public void Init(float delta, int carsCount, float trafficLightsGreenTime)
        {
            this.delta = delta;
            this.carsCount = carsCount;
            this.trafficLightsGreenTime = trafficLightsGreenTime;
            initialized = true;
        }

        private void CheckInitialized()
        {
            if (!initialized)
                throw new Exception("NavGraphManager wasn't initialized. Call method Init first.");
        }

        public async GDTask Generate(float range, float height)
        {
            CheckInitialized();

            float directionsOffset = delta / 8;
            Vector3 offset = new Vector3(subdivisionOffset * delta - directionsOffset, modelHegihtOffset, directionsOffset);

            // Create undirected graph
            NavGraph navGraphUndirected = NavGraphGenerator.Generate(range, height, delta, connectionProbability);
            // Place road models
            GDTask roadGeneration = NavGraphModelPlacer.Instance.GenerateRoads(navGraphUndirected, delta, modelHegihtOffset);

            // Create directed graph and assign shedules
            navGraph = NavGraphGenerator.PostProcess(navGraphUndirected, subdivisionOffset, directionsOffset, trafficLightsGreenTime);
            // Place traffic lights and car models
            GDTask trafficLightsGeneration = NavGraphModelPlacer.Instance.GenerateTrafficLights(navGraph, delta / 4, offset);
            GDTask carsGeneration = NavGraphModelPlacer.Instance.GenerateCars(navGraph, carsCount, modelHegihtOffset);

            await GDTask.WhenAll(roadGeneration, trafficLightsGeneration, carsGeneration);

            //ShowDebugGraph();

            GD.Print($"Генерация транспортных потоков завершена.");
        }

        public string SaveGraph()
        {
            return NavGraphSerializer.Serialize(navGraph);
        }

        public void LoadGraph(string serializedGraph)
        {
            navGraph = NavGraphSerializer.Deserialize(serializedGraph);

            //ShowDebugGraph();
        }

        public Queue<Vector3> BuildPath(Vector3 from, Vector3 to)
        {
            if (navGraph == null)
                return null;

            float vertexTolerance = 1f;
            NavGraphVertex fromVertex = NavGraphVertexFinder.GetVertex(navGraph, from, vertexTolerance);
            NavGraphVertex toVertex = NavGraphVertexFinder.GetVertex(navGraph, to, vertexTolerance);
            List<NavGraphVertex> path = NavGraphPathFinder.GetPath(fromVertex, toVertex);
            Queue<Vector3> pathPoints = new Queue<Vector3>();
            foreach (NavGraphVertex vertex in path)
            {
                pathPoints.Enqueue(vertex.position);
            }
            return pathPoints;
        }

        public void ShowDebugGraph()
        {
            if (navGraph == null)
                return;

            visualization?.Clear();
            visualization = new NavGraphVisualization();
            visualization.Draw(navGraph, this, modelHegihtOffset);
        }

        public void HideDebugGraph()
        {
            visualization?.Clear();
        }

        public Vector3 GetRandomPoint()
        {
            if (navGraph == null)
            {
                throw new InvalidOperationException("Couldn't get navigation point. Navigation graph is not initialized.");
            }

            int idx = rng.RandiRange(0, navGraph.vertices.Count - 1);
            return navGraph.vertices[idx].position;
        }

        public void OnDependenciesInjected()
        {
            
        }
    }
}
