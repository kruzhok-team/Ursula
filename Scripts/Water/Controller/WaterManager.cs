using Fractural.Tasks;
using Godot;
using System;
using System.Drawing;
using Ursula.Core.DI;
using Ursula.Terrain.Model;
using Ursula.Water.Model;
using static Godot.TileSet;

public partial class WaterManager : WaterModel, IInjectable
{
    [Inject]
    private ISingletonProvider<WaterModel> _waterModelProvider;

    [Inject]
    private ISingletonProvider<TerrainModel> _terrainModelProvider;

    [Inject]
    private ISingletonProvider<TerrainManager> _terrainManagerProvider;

    private WaterModel _waterModel { get; set; }
    private TerrainModel _terrainModel { get; set; }
    private TerrainManager _terrainManager { get; set; }

    MeshInstance3D meshInstance;
    private float scaleWater = 4f;



    void IInjectable.OnDependenciesInjected()
    {

    }

    public override void _Ready()
    {
        VoxLib.waterManager ??= this;

        base._Ready();
        _ = SubscribeEvent();
    }

    private async GDTask SubscribeEvent()
    {
        _waterModel = await _waterModelProvider.GetAsync();
        _terrainModel = await _terrainModelProvider.GetAsync();
        _terrainManager = await _terrainManagerProvider.GetAsync();

        _waterModel.StartGenerateWater_EventHandler += WaterModel_StartGenerateWater_EventHandler;
    }

    public void GenerateStaticWater(int size)
    {
        VoxLib.RemoveAllChildren(this);

        meshInstance = new MeshInstance3D();

        //PlaneMesh planeMesh = new PlaneMesh
        //{
        //    Size = new Vector2(size, size),
        //    SubdivideDepth = 1,
        //    SubdivideWidth = 1
        //};

        CylinderMesh cylinderMesh = new CylinderMesh();
        cylinderMesh.RadialSegments = 32;
        cylinderMesh.Height = 0.01f;
        cylinderMesh.BottomRadius = size;
        cylinderMesh.TopRadius = size;

        meshInstance.Mesh = cylinderMesh;

        var shaderMaterial = new ShaderMaterial();
        shaderMaterial = VoxLib.mapAssets.WaterMat;
        switch ((VoxDrawTypes)_waterModel._WaterData.TypeWaterID)
        {
            case VoxDrawTypes.water:
                shaderMaterial = VoxLib.mapAssets.WaterMatPlane;
                break;
            case VoxDrawTypes.lava:
                shaderMaterial = VoxLib.mapAssets.LavaMatPlane;
                break;
            case VoxDrawTypes.ice:
                shaderMaterial = VoxLib.mapAssets.IceMat;
                break;
            case VoxDrawTypes.swamp:
                shaderMaterial = VoxLib.mapAssets.SwampMat;
                break;
        }

        meshInstance.MaterialOverride = shaderMaterial;

        meshInstance.Position = new Vector3(_terrainManager.positionOffset.X + size / 2, _waterModel._WaterData.WaterLevel, _terrainManager.positionOffset.Z + size / 2);
        meshInstance.Scale = new Vector3(scaleWater, scaleWater, scaleWater);

        AddChild(meshInstance);
    }

    public void GenerateWater(int size, bool isStatic = true)
    {
        float lvl = (_terrainManager.MaxHeightTerrain - (_terrainManager.MaxHeightTerrain * _waterModel._WaterData.WaterOffset)) / 2;
        lvl += _terrainManager.positionOffset.Y;

        _waterModel.SetWaterLevel(lvl);

        if (isStatic)
            GenerateStaticWater(size);
        else
            GenerateDinamicWater(size);
    }

    void GenerateDinamicWater(int countBlock)
    {
        VoxLib.RemoveAllChildren(this);

        int width = countBlock + 1;
        int height = countBlock + 1;

        ArrayMesh mesh = new ArrayMesh();

        Vector3[] vertices = new Vector3[height * width];
        Vector2[] UVs = new Vector2[height * width];
        Vector4[] tangs = new Vector4[height * width];
        Vector3[] normals = new Vector3[height * width];

        int index;

        for (int x = 0; x < height; x++)
        {
            for (int z = 0; z < width; z++)
            {
                index = x * width + z; 
                int indexX = countBlock + x;
                int indexZ = countBlock + z;

                vertices[index] = new Vector3(x, 0, z);
                UVs[index] = new Vector2(x, z);
                normals[index] = new Vector3(0, 1, 0);
            }
        }

        index = 0;
        int[] triangles = new int[(height - 1) * (width - 1) * 6];
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                triangles[index++] = (y * width) + x;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = (y * width) + x + 1;
                triangles[index++] = ((y + 1) * width) + x;
                triangles[index++] = ((y + 1) * width) + x + 1;
                triangles[index++] = (y * width) + x + 1;
            }
        }

        var arrays = new Godot.Collections.Array();
        arrays.Resize((int)ArrayMesh.ArrayType.Max);
        arrays[(int)ArrayMesh.ArrayType.Vertex] = vertices;
        arrays[(int)ArrayMesh.ArrayType.Index] = triangles;
        arrays[(int)ArrayMesh.ArrayType.Normal] = normals;
        arrays[(int)ArrayMesh.ArrayType.TexUV] = UVs;

        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        StaticBody3D staticBody = new StaticBody3D();

        meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;
        meshInstance.Name = "Water";

        meshInstance.Position = new Vector3(_terrainManager.positionOffset.X - width - width/2, _waterModel._WaterData.WaterLevel, _terrainManager.positionOffset.Z - height - height/2);
        meshInstance.Scale = new Vector3(scaleWater, scaleWater, scaleWater);

        var shaderMaterial = new ShaderMaterial();
        shaderMaterial = VoxLib.mapAssets.WaterMat;
        switch ((VoxDrawTypes)_waterModel._WaterData.TypeWaterID)
        {
            case VoxDrawTypes.water:
                shaderMaterial = VoxLib.mapAssets.WaterMat;
                break;
            case VoxDrawTypes.lava:
                shaderMaterial = VoxLib.mapAssets.LavaMat;
                break;
            case VoxDrawTypes.ice:
                shaderMaterial = VoxLib.mapAssets.IceMat;
                break;
            case VoxDrawTypes.swamp:
                shaderMaterial = VoxLib.mapAssets.SwampMat;
                break;
        }
        meshInstance.MaterialOverride = shaderMaterial;

        AddChild(meshInstance);

        GD.Print("Create water position=" + meshInstance.Position);
    }

    public void DeleteWater()
    {
        try
        {
            if (meshInstance != null) meshInstance.Free();
        }
        catch
        {

        }
    }

    private void WaterModel_StartGenerateWater_EventHandler(object sender, EventArgs e)
    {
        DeleteWater();
        GenerateWater(_waterModel._WaterData.Size, _waterModel._WaterData.IsStaticWater);
    }
}
