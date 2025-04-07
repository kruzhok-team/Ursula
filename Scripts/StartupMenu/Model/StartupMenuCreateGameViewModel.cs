using Core.UI.Constructor;
using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.View;
using Ursula.GameProjects.Model;

namespace Ursula.StartupMenu.Model
{
    public class CreateGameSourceData
    {
        public string GameName { get; private set; }
        public string GameNameAlias { get; private set; }
        public string GameImagePath { get; private set; }
        public float PowerValue { get; private set; }
        public float ScaleValue { get; private set; }
        public int PlatoSize { get; private set; }
        public int PlatoPlatoOffsetX { get; private set; }
        public int PlatoPlatoOffsetZ { get; private set; }
        public int ReplaceTextureID { get; private set; }
        public int TypeSkyID { get; private set; }
        public float FullDayLength { get; private set; }
        public int TypeWaterID { get; private set; }
        public float WaterOffset { get; private set; }
        public bool IsStaticWater { get; private set; }
        public string TreeProviderID { get; private set; }
        public string GrassProviderID { get; private set; }
        public float TreesDensity { get; private set; }
        public float GrassDensity { get; private set; }

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

        public CreateGameSourceData SetPlatoSizeValue(int value)
        {
            PlatoSize = value;
            return this;
        }

        public CreateGameSourceData SetPlatoPlatoOffsetX(int value)
        {
            PlatoPlatoOffsetX = value;
            return this;
        }

        public CreateGameSourceData SetPlatoPlatoOffsetZ(int value)
        {
            PlatoPlatoOffsetZ = value;
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

        public CreateGameSourceData SetTreeProviderID(string value)
        {
            TreeProviderID = value;
            return this;
        }

        public CreateGameSourceData SetGrassProviderID(string value)
        {
            GrassProviderID = value;
            return this;
        }

        public CreateGameSourceData SetTreesDensity(float value)
        {
            TreesDensity = value;
            return this;
        }

        public CreateGameSourceData SetGrassDensity(float value)
        {
            GrassDensity = value;
            return this;
        }
    }

    public partial class StartupMenuCreateGameViewModel : ConstructorViewModel, IInjectable
    {
        public event EventHandler ViewVisible_EventHandler;
        public event EventHandler StartGenerateGame_EventHandler;

        public CreateGameSourceData _CreateGameSourceData = new CreateGameSourceData();

        void IInjectable.OnDependenciesInjected()
        {

        }

        public StartupMenuCreateGameViewModel SetVisibleView(bool value)
        {
            Visible = value;
            InvokeMenuVisibleEvent();
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameName(string value)
        {
            _CreateGameSourceData.SetGameName(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameNameAlias(string value)
        {
            _CreateGameSourceData.SetGameNameAlias(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGameImagePath(string path)
        {
            _CreateGameSourceData.SetGameImagePath(path);
            return this;
        }

        public StartupMenuCreateGameViewModel SetPowerValue(float value)
        {
            float power = MapRange(value, 0, 100, 0, 30);
            _CreateGameSourceData.SetPowerValue(power);
            return this;
        }

        public StartupMenuCreateGameViewModel SetScaleValue(float value)
        {
            float scale = MapRange(value, 1, 100, 1, 5);
            _CreateGameSourceData.SetScaleValue(scale);
            return this;
        }

        public StartupMenuCreateGameViewModel SetPlatoSizeValue(int value)
        {
            //float scale = MapRange(value, 1, 100, 1, 5);
            _CreateGameSourceData.SetPlatoSizeValue(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetPlatoPlatoOffsetX(int value)
        {
            float scale = MapRange(value, 1, 100, 1, 256);
            _CreateGameSourceData.SetPlatoPlatoOffsetX((int)scale);
            return this;
        }

        public StartupMenuCreateGameViewModel SetPlatoPlatoOffsetZ(int value)
        {
            float scale = MapRange(value, 1, 100, 1, 256);
            _CreateGameSourceData.SetPlatoPlatoOffsetZ((int)scale);
            return this;
        }

        public StartupMenuCreateGameViewModel SetReplaceTextureID(int value)
        {
            _CreateGameSourceData.SetReplaceTextureID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTypeSkyID(int value)
        {
            _CreateGameSourceData.SetTypeSkyID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetFullDayLength(float value)
        {
            _CreateGameSourceData.SetFullDayLength(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTypeWaterID(int value)
        {
            _CreateGameSourceData.SetTypeWaterID(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetWaterOffset(float value)
        {
            float offset = MapRange(value, 0, 100, 0, 1);
            _CreateGameSourceData.SetWaterOffset(offset);
            return this;
        }

        public StartupMenuCreateGameViewModel SetStaticWater(bool value)
        {
            _CreateGameSourceData.SetStaticWater(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTreeProviderID(string providerID)
        {
            _CreateGameSourceData.SetTreeProviderID(providerID);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGrassProviderID(string providerID)
        {
            _CreateGameSourceData.SetGrassProviderID(providerID);
            return this;
        }

        public StartupMenuCreateGameViewModel SetTreesDensity(float value)
        {
            _CreateGameSourceData.SetTreesDensity(value);
            return this;
        }

        public StartupMenuCreateGameViewModel SetGrassDensity(float value)
        {
            _CreateGameSourceData.SetGrassDensity(value);
            return this;
        }

        public StartupMenuCreateGameViewModel StartGenerateGame()
        {
            InvokeStartGenerateGameEvent();
            return this;
        }

        private void InvokeMenuVisibleEvent()
        {
            var handler = ViewVisible_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private float MapRange(float value, float min1, float max1, float min2, float max2)
        {
            return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
        }


        public void SetCreateGameViewCreateFolderGame()
        {
            string DestPath = GameProjectAssetsUserSource.CollectionPath + _CreateGameSourceData.GameName;
            Directory.CreateDirectory(ProjectSettings.GlobalizePath(DestPath));
        }


        private void InvokeStartGenerateGameEvent()
        {
            var handler = StartGenerateGame_EventHandler;
            handler?.Invoke(this, EventArgs.Empty);
        }

    }
}
