using Godot;
using System;

public partial class DayNightCycle : Node
{
    public static DayNightCycle instance;

    public float FullDayLength = 120f * 4f; // Полный день в секундах
    private float _currentTime = 0f; // Текущее время суток

    [Export]
    public DirectionalLight3D Sun; // Ссылка на объект Солнца

    [Export]
    public WorldEnvironment World;

    [Export]
    public ProceduralSkyMaterial proceduralSkyMaterial;

    [Export]
    public Color proceduralColorDay;

    [Export]
    public Color proceduralColorNyght;

    [Export]
    public PanoramaSkyMaterial NightPanorama;

    public override void _Ready()
    {
        base._Ready();
        instance = this;
    }


    public override void _Process(double delta)
    {
        _currentTime += (float)delta;

        if (_currentTime >= FullDayLength)
        {
            _currentTime = 0f;
        }

        UpdateSunPosition();
    }

    private void UpdateSunPosition()
    {
        if (World == null) World = (this as Node) as WorldEnvironment;

        float percentOfDay = _currentTime / FullDayLength;
        Sun.RotationDegrees = new Vector3((percentOfDay * 360f) - 90, 170, 0);

        if (percentOfDay < 0.250f || percentOfDay > 0.730f) // Утро 0.28
        {
            proceduralSkyMaterial.SkyCoverModulate = proceduralSkyMaterial.SkyCoverModulate.Lerp(proceduralColorDay, 0.01f);
            proceduralSkyMaterial.SkyEnergyMultiplier = Mathf.Lerp(proceduralSkyMaterial.SkyEnergyMultiplier, 1f, 0.01f);
            proceduralSkyMaterial.GroundEnergyMultiplier = Mathf.Lerp(proceduralSkyMaterial.SkyEnergyMultiplier, 1f, 0.01f);
        }
        else // Вечер
        {
            proceduralSkyMaterial.SkyCoverModulate = proceduralSkyMaterial.SkyCoverModulate.Lerp(proceduralColorNyght, 0.01f);
            proceduralSkyMaterial.SkyEnergyMultiplier = Mathf.Lerp(proceduralSkyMaterial.SkyEnergyMultiplier, 0.15f, 0.01f);
            proceduralSkyMaterial.GroundEnergyMultiplier = Mathf.Lerp(proceduralSkyMaterial.SkyEnergyMultiplier, 0.15f, 0.01f);
        }

        if (percentOfDay < 0.270f || percentOfDay > 0.700f) // Утро 0.28
        {
            //Sun.LightColor = new Color(1, 0.9f, 0.8f).Lerp(new Color(1f, 1f, 0.5f), percentOfDay * 2);
            Sun.LightEnergy = Mathf.Lerp(Sun.LightEnergy, 1f, 0.01f);
            Sun.SkyMode = DirectionalLight3D.SkyModeEnum.LightAndSky;
            World.Environment.Sky.SkyMaterial = proceduralSkyMaterial;

            //proceduralSkyMaterial.SkyCoverModulate.Lerp(proceduralColorDay, 0.01f);

        }
        else // Вечер
        {
            //Sun.LightColor = new Color(1f, 1f, 0.5f).Lerp(new Color(1, 0.5f, 0.5f), (percentOfDay - 0.5f) * 2);
            //Sun.LightEnergy = 0;
            Sun.LightEnergy = Mathf.Lerp(Sun.LightEnergy, 0f, 0.01f);
            //Sun.SkyMode = DirectionalLight3D.SkyModeEnum.SkyOnly;
            //World.Environment.Sky.SkyMaterial = NightPanorama;
            //proceduralSkyMaterial.SkyCoverModulate.Lerp(proceduralColorNyght, 0.01f);

            World.Environment.Sky.SkyMaterial = proceduralSkyMaterial;
        }

        if (percentOfDay < 0.270f || percentOfDay > 0.650f) // Утро 0.28
        {
            NightPanorama.EnergyMultiplier = Mathf.Lerp(NightPanorama.EnergyMultiplier, 50f, 0.001f);
        }
        else // Вечер
        {
            NightPanorama.EnergyMultiplier = Mathf.Lerp(NightPanorama.EnergyMultiplier, 3f, 0.001f);
        }
    }


    public float TimesOfDay()
    {
        //return _currentTime;
        float percentOfDay = _currentTime / FullDayLength;
        //return percentOfDay * 24;

        float a = 12 + percentOfDay * 24;
        if (a >= 24) a = percentOfDay * 24 - 12;
        return a;
    }

    public void SetFullDayLength(float FullDayLength)
    {
        this.FullDayLength = FullDayLength;
        _currentTime = 0;
        //VoxLib.hud.SaveLengthOfDay(FullDayLength);
    }
}
