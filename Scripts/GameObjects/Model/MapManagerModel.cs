using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using VoxLibExample;


namespace Ursula.MapManagers.Model
{
    public class MapManagerData
    {
        public static string MapManagerAssetPath = "res://addons/Ursula/Assets/MapAssets.tres";

        public List<ItemPropsScript> gameItems { get; private set; }
        public Node3D itemsGO { get; private set; }

        public MapManagerData()
        {
            gameItems = new List<ItemPropsScript>();
            itemsGO = new Node3D();

            var mapAssets = ResourceLoader.Load<MapAssets>(MapManagerAssetPath);
            VoxLib.mapAssets = mapAssets;
        }




    }

    public partial class MapManagerModel : IInjectable
    {
        public MapManagerData _mapManagerData = new MapManagerData();

        void IInjectable.OnDependenciesInjected()
        {
        }

        public void SetItemToMap(Node node, ItemPropsScript ips)
        {
            _mapManagerData.itemsGO.AddChild(node);
            _mapManagerData.gameItems.Add(ips);
        }
    }
}
