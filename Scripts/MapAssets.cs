using Godot;
using System;
using Ursula.GameObjects.Model;

namespace VoxLibExample
{
    [Tool]
    public partial class MapAssets : Resource
	{
        public const string GameObjectGroups = "Деревья,Трава,Камни,Строения,Животные,Предметы,Освещение,Другое";

        [Export]
		public Texture[] terrainTex { get; set; }

        [Export]
		public Texture[] terrainTexDefault { get; set; }

        [Export]
        public Texture[] terrainTexReplace { get; set; }

        [Export]
        public PackedScene[] gameItemsGO { get; set; }

        [Export]
		public Texture[] inventarItemTex { get; set; }

        [Export(PropertyHint.Enum, GameObjectGroups)]
        public string[] inventarGameObjectGroups { get; set; } = Array.Empty<string>();

        [Export]
        public ShaderMaterial TerrainMat { get; set; }

        [Export]
        public StandardMaterial3D TerrainMat3D { get; set; }

        [Export]
        public ShaderMaterial TriplanarMat { get; set; }

        [Export]
        public ShaderMaterial WaterMat { get; set; }
        [Export]
        public ShaderMaterial LavaMat { get; set; }
        [Export]
        public ShaderMaterial IceMat { get; set; }
        [Export]
        public ShaderMaterial SwampMat { get; set; }

        [Export]
        public ShaderMaterial WaterMatPlane { get; set; }
        [Export]
        public ShaderMaterial LavaMatPlane { get; set; }

        [Export]
        public PackedScene customItemPrefab;

        [Export]
        public PackedScene customObjectPrefab;

        [Export]
        public PackedScene customPlatformPrefab;

        [Export]
        public PackedScene InteractiveObjectDetectorPrefab;
        [Export]
        public PackedScene InteractiveObjectAudioPrefab;
        [Export]
        public PackedScene InteractiveObjectMovePrefab;
        [Export]
        public PackedScene InteractiveObjectTimerPrefab;
        [Export]
        public PackedScene InteractiveObjectCounterPrefab;
    }
}
