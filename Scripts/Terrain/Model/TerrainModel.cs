using Godot;
using System;
using Ursula.Core.DI;

namespace Ursula.Terrain.Model
{
    public class TerrainData
    {
        public bool RandomHeight { get; private set; }
        public int Size { get; private set; } = 256;
        public int PlatoSize { get; private set; }
        public int PlatoOffsetX { get; private set; }
        public int PlatoOffsetZ { get; private set; }
        public float Power { get; private set; }
        public float Scale { get; private set; }
        public float Exponent { get; private set; }
        public int ReplaceTexID { get; private set; }
        public float[,] MapHeight { get; private set; }

        public void SetRandomHeight(bool value)
        {
            RandomHeight = value;
        }

        public void SetSize(int value)
        {
            Size = value;
        }

        public void SetPlatoSize(int value)
        {
            PlatoSize = value;
        }

        public void SetPlatoOffsetX(int value)
        {
            PlatoOffsetX = value;
        }

        public void SetPlatoOffsetZ(int value)
        {
            PlatoOffsetZ = value;
        }

        public void SetPower(float value)
        {
            Power = value;
        }

        public void SetScale(float value)
        {
            Scale = value;
        }

        public void SetExponent(float value)
        {
            Exponent = value;
        }

        public void SetReplaceTexID(int value)
        {
            ReplaceTexID = value;
        }

        public void SetMapHeight(float[,] mapHeight)
        {
            MapHeight = mapHeight;
        }
    }


    public partial class TerrainModel : Node, IInjectable
    {
        public event EventHandler StartGenerateTerrain_EventHandler;

        public TerrainData _TerrainData = new TerrainData();

        void IInjectable.OnDependenciesInjected()
        {

        }

        public TerrainModel StartGenerateTerrain(bool value = false)
        {
            SetRandomHeight(value);
            InvokeStartGenerateTerrainEvent();
            return this;
        }

        public TerrainModel SetRandomHeight(bool value)
        {
            _TerrainData.SetRandomHeight(value);
            return this;
        }

        public TerrainModel SetSize(int value)
        {
            _TerrainData.SetSize(value);
            return this;
        }

        public TerrainModel SetPlatoSize(int value)
        {
            _TerrainData.SetPlatoSize(value);
            return this;
        }

        public TerrainModel SetPlatoOffsetX(int value)
        {
            _TerrainData.SetPlatoOffsetX(value);
            return this;
        }

        public TerrainModel SetPlatoOffsetZ(int value)
        {
            _TerrainData.SetPlatoOffsetZ(value);
            return this;
        }

        public TerrainModel SetPower(float value)
        {
            _TerrainData.SetPower(value);
            return this;
        }

        public TerrainModel SetScale(float value)
        {
            _TerrainData.SetScale(value);
            return this;
        }

        public TerrainModel SetExponent(float value)
        {
            _TerrainData.SetExponent(value);
            return this;
        }

        public TerrainModel SetReplaceTexID(int value)
        {
            _TerrainData.SetReplaceTexID(value);
            return this;
        }

        public TerrainModel SetMapHeight(float[,] mapHeight)
        {
            _TerrainData.SetMapHeight(mapHeight);
            return this;
        }

        private void InvokeStartGenerateTerrainEvent()
        {
            var handler = StartGenerateTerrain_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
