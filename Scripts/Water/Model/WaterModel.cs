using Godot;
using System;
using Ursula.Core.DI;


namespace Ursula.Water.Model
{
    public class WaterData
    {
        public bool IsStaticWater { get; private set; }
        public int Size { get; private set; }
        public float WaterOffset { get; private set; }
        public int TypeWaterID { get; private set; }
        public float WaterLevel { get; private set; }

        public void SetStatic(bool value)
        {
            IsStaticWater = value;
        }

        public void SetSize(int value)
        {
            Size = value;
        }

        public void SetWaterOffset(float value)
        {
            WaterOffset = value;
        }

        public void SetTypeWaterID(int value)
        {
            TypeWaterID = value;
        }

        public void SetWaterLevel(float value)
        {
            WaterLevel = value;
        }

        public WaterData()
        {
            IsStaticWater = true;
            Size = 256;
        }
    }

    public partial class WaterModel : Node, IInjectable
    {
        public event EventHandler StartGenerateWater_EventHandler;
        public event EventHandler StartDeleteWater_EventHandler;
        

        public WaterData _WaterData = new WaterData();

        void IInjectable.OnDependenciesInjected()
        {

        }

        public WaterModel StartGenerateWater()
        {
            InvokeStartGenerateWaterEvent();
            return this;
        }

        public WaterModel StartDeleteWater()
        {
            InvokeStartDeleteWaterEvent();
            return this;
        }
        

        public WaterModel SetStaticWater(bool value)
        {
            _WaterData.SetStatic(value);
            return this;
        }

        public WaterModel SetSize(int value)
        {
            _WaterData.SetSize(value);
            return this;
        }

        public WaterModel SetWaterOffset(float value)
        {
            _WaterData.SetWaterOffset(value);
            return this;
        }

        public WaterModel SetTypeWaterID(int value)
        {
            _WaterData.SetTypeWaterID(value);
            return this;
        }

        public WaterModel SetWaterLevel(float value)
        {
            _WaterData.SetWaterLevel(value);
            return this;
        }

        private void InvokeStartGenerateWaterEvent()
        {
            var handler = StartGenerateWater_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeStartDeleteWaterEvent()
        {
            var handler = StartDeleteWater_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
