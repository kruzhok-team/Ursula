﻿using Godot;

namespace VoxLibExample
{
    [Tool]
    public partial class MapAssets : Resource
	{
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
    }
}
