using Core.UI.Constructor;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.GameObjects.View;

namespace Ursula.StartupMenu.Model
{
    public class CreateGameSourceData
    {
        public string GameName { get; private set; }
        public string GameNameAlias { get; private set; }
        public string GameImagePath { get; private set; }
        public float PowerValue { get; private set; }
        public float ScaleValue { get; private set; }
        public int ReplaceTextureID { get; private set; }
        public int TypeSkyID { get; private set; }
        public float FullDayLength { get; private set; }
        public int TypeWaterID { get; private set; }
        public float WaterOffset { get; private set; }
        public bool IsStaticWater { get; private set; }
        

        public CreateGameSourceData SetGameName(string value)
        {
            GameName = value;
            return this;
        }

        public CreateGameSourceData SetGameNameAlias(string value)
        {
            GameNameAlias = value;
            return this;
        }

        public CreateGameSourceData SetGameImagePath(string value)
        {
            GameImagePath = value;
            return this;
        }

        public CreateGameSourceData SetPowerValue(float value)
        {
            PowerValue = value;
            return this;
        }

        public CreateGameSourceData SetScaleValue(float value)
        {
            ScaleValue = value;
            return this;
        }

        public CreateGameSourceData SetReplaceTextureID(int value)
        {
            ReplaceTextureID = value;
            return this;
        }

        public CreateGameSourceData SetTypeSkyID(int value)
        {
            TypeSkyID = value;
            return this;
        }

        public CreateGameSourceData SetFullDayLength(float value)
        {
            FullDayLength = value;
            return this;
        }

        public CreateGameSourceData SetTypeWaterID(int value)
        {
            TypeWaterID = value;
            return this;
        }

        public CreateGameSourceData SetWaterOffset(float value)
        {
            WaterOffset = value;
            return this;
        }

        public CreateGameSourceData SetStaticWater(bool value)
        {
            IsStaticWater = value;
            return this;
        }
        

    }

    public partial class StartupMenuCreateGameViewModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler CreateGameViewVisible_EventHandler;




        private CreateGameSourceData _createGameSourceData = new CreateGameSourceData();


        void IInjectable.OnDependenciesInjected()
        {

        }

        public StartupMenuCreateGameViewModel SetCreateGameViewVisible(bool value)
        {
            Visible = value;
            InvokeMenuVisibleEvent();
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameName(string value)
        {
            _createGameSourceData.SetGameName(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameNameAlias(string value)
        {
            _createGameSourceData.SetGameNameAlias(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameImagePath(string value)
        {
            _createGameSourceData.SetGameImagePath(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetPowerValue(float value)
        {
            float power = MapRange(value, 0, 100, 0, 40);
            _createGameSourceData.SetPowerValue(power);
            return this;
        }

        public StartupMenuCreateGameViewModel SetScaleValue(float value)
        {
            float scale = MapRange(value, 1, 100, 1, 5);
            _createGameSourceData.SetScaleValue(scale);
            return this;
        }

        public StartupMenuCreateGameViewModel SetReplaceTextureID(int value)
        {
            _createGameSourceData.SetReplaceTextureID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTypeSkyID(int value)
        {
            _createGameSourceData.SetTypeSkyID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetFullDayLength(float value)
        {
            _createGameSourceData.SetFullDayLength(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTypeWaterID(int value)
        {
            _createGameSourceData.SetTypeWaterID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetWaterOffset(float value)
        {
            float offset = MapRange(value, 0, 100, 0, 1);
            _createGameSourceData.SetWaterOffset(offset);
            return this;
        }

        public StartupMenuCreateGameViewModel SetStaticWater(bool value)
        {
            _createGameSourceData.SetStaticWater(value);
            return this;
        }

        

        private void InvokeMenuVisibleEvent()
        {
            var handler = CreateGameViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private float MapRange(float value, float min1, float max1, float min2, float max2)
        {
            return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
        }
    }
}
