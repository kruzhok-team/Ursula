using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;
using Ursula.MapManagers.Model;
using Ursula.Terrain.Model;
using Ursula.Water.Model;


public partial class TerrainManager : TerrainModel, IInjectable
{
    public static string NAMETERRAIN = "Terrain";
    private const string ObstacleGroup = "obstacles";

    public static TerrainManager instance;

    [Inject]
    private ISingletonProvider<TerrainModel> _terrainModelProvider;

    [Inject]
    private ISingletonProvider<WaterModel> _waterModelProvider;

    [Inject]
    private ISingletonProvider<MapManagerModel> _mapManagerModelProvider;

    public int countChunk = 1;
    public int countBlock = 256;

    public float[,] mapHeight;

    public Vector3 positionOffset = new Vector3(-0.5f, 36.0f, -0.5f); // new Vector3(0f, 0f, 0f);// 

    public Vector3 startPoint = new Vector3(0f, 0f, 0f);

    public struct Chunk
    {
        //public GameObject go;
        //public MeshRenderer mr;
        //public MeshFilter mf;
        //public MeshCollider mc;
    }
    public Chunk[,] _chunks;


    private TerrainModel _terrainModel { get; set; }
    private WaterModel _waterModel { get; set; }
    private MapManagerModel _mapManagerModel { get; set; }

    private bool isNavMeshUpdater = false;
    private MeshInstance3D meshInstance;
    private NavigationMesh navMesh;
    private StaticBody3D staticBody;

    private FastNoiseLite noise;
    private float plateauHeight;

    private float exponent = 2;

    void IInjectable.OnDependenciesInjected()
    {

    }

    public override void _Ready()
    {
        VoxLib.terrainManager ??= this;

        base._Ready();
        _ = SubscribeEvent();
    }

    public int size
    {
        get
        {
            return countChunk * countBlock + 1;
        }
    }

    public void ProcCreateTerrainRandom(bool randomHeight)
    {
        ProcCreateTerrain(randomHeight);
    }

    public void ProcCreateTerrain(bool randomHeight)
    {
        VoxLib.RemoveAllChildren(VoxLib.mapManager.navigationRegion3D);
        _mapManagerModel.RemoveAllGameItems();

        _ProcCreateTerrain(randomHeight);
    }

    public void _ProcCreateTerrain(bool randomHeight)
    {
        instance ??= this;
        VoxLib.terrainManager ??= this;

        //if (navMesh != null)
        //{
        //    navMesh.Clear();
        //    navMesh.Free();
        //}

        //if (VoxLib.mapManager.navigationRegion3D.NavigationMesh != null)
        //{
        //    VoxLib.mapManager.navigationRegion3D.NavigationMesh.Clear();
        //}

        ClearNavMesh();

        navMesh = new NavigationMesh();

        //VoxLib.mapManager.navigationRegion3D.NavigationMesh.ClearPolygons();
        //navMesh = VoxLib.mapManager.navigationRegion3D.NavigationMesh;

        navMesh.AgentMaxClimb = 1f;
        navMesh.AgentMaxSlope = 85f;
        navMesh.AgentHeight = 1.5f;

        //navMesh.AgentRadius = 0.25f;
        navMesh.CellSize = 0.5f;
        //navMesh.CellHeight = 1.0f;

        navMesh.RegionMergeSize = 5;
        navMesh.DetailSampleDistance = 2;
        navMesh.EdgeMaxLength = 2;

        if (randomHeight)
        {
            Vector2 rnd = new Vector2(0, 0);
            GenerateMapHeight(rnd);
        }

        _chunks = new Chunk[countChunk, countChunk];

        for (int x = 0; x < countChunk; x++)
        {
            for (int z = 0; z < countChunk; z++)
            {
                startPoint[0] = (x == positionOffset[0]) ? 0 : x * countBlock + positionOffset[0];
                startPoint[2] = (z == positionOffset[2]) ? 0 : z * countBlock + positionOffset[2];
                startPoint[1] = positionOffset[1];

                Generate(startPoint, x, z);
            }
        }
    }

    public void BakeNavMesh()
    {
        //await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        VoxLib.mapManager.navigationRegion3D.NavigationMesh = navMesh;
        VoxLib.mapManager.navigationRegion3D.Position = positionOffset;

        Rid map = VoxLib.mapManager.navigationRegion3D.GetNavigationMap();

        VoxLib.mapManager.navigationRegion3D.BakeNavigationMesh();

        //for (int i = 0; i < VoxLib.mapManager.gameItems.Count; i++)
        //{
        //	NavigationObstacle3D obstacle3D = VoxLib.mapManager.gameItems[i].FindChild("NavigationObstacle3D", true, true) as NavigationObstacle3D;
        //	if (obstacle3D != null)
        //	{
        //		var RID = NavigationServer3D.ObstacleCreate();

        //		NavigationServer3D.ObstacleSetMap(RID, map);
        //		NavigationServer3D.ObstacleSetPosition(RID, obstacle3D.Position);
        //		//NavigationServer3D.ObstacleSetRadius(RID, obstacle3D.Radius);

        //		NavigationServer3D.ObstacleSetVertices(RID, obstacle3D.Vertices);
        //		NavigationServer3D.ObstacleSetHeight(RID, obstacle3D.Height);

        //		NavigationServer3D.ObstacleSetAvoidanceEnabled(RID, true);
        //	}
        //}

        //var obstacles = NavigationServer3D.MapGetObstacles(map);
        //GD.Print($"Added nav mesh obstacles: count= {obstacles.Count}");

        GD.Print($"NavMesh {navMesh} baked.");
    }

    public float MaxHeightTerrain
    {
        get
        {
            int size = VoxLib.terrainManager.size;

            if (VoxLib.terrainManager.mapHeight == null) return 0;

            float height = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    height = (height > VoxLib.terrainManager.mapHeight[i, j]) ? height : VoxLib.terrainManager.mapHeight[i, j];
                }
            }
            height += VoxLib.terrainManager.positionOffset.Y;
            return height;
        }
    }

    private async GDTask SubscribeEvent()
    {
        _terrainModel = await _terrainModelProvider.GetAsync();
        _waterModel = await _waterModelProvider.GetAsync();
        _mapManagerModel = await _mapManagerModelProvider.GetAsync();

        _terrainModel.StartGenerateTerrain_EventHandler += TerrainModel_StartGenerateTerrain_EventHandler;
    }

    private void TerrainModel_StartGenerateTerrain_EventHandler(object sender, EventArgs e)
    {
        ProcCreateTerrainRandom(_terrainModel._TerrainData.RandomHeight);
    }

    // Генерация карты высот
    private void GenerateMapHeight(Vector2 pos)
    {
        float power = _terrainModel._TerrainData.Power;
        float scale = _terrainModel._TerrainData.Scale;

        noise = new FastNoiseLite();
        Godot.RandomNumberGenerator rng = new Godot.RandomNumberGenerator();
        noise.Seed = (int)(rng.Randi() % (1000000 - 1) + 1);

        //size = countBlock * countChunk + 1;
        mapHeight = new float[size, size];

        int centerX = (int)pos[0] + (size - 1) / 2;
        int centerY = (int)pos[1] + (size - 1) / 2;
        float maxDistance = Mathf.Sqrt(centerX * centerX + centerY * centerY);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i * (5 - scale); // (pos[0] + i * scale)/ countBlock; // X координата вершины
                float z = j * (5 - scale); // (pos[1] + j * scale)/ countBlock; // Z координата вершины
                float e = 0;

                //1
                //e = (Mathf.PerlinNoise(x, z - 0.5f)) * power;

                //2
                e = (float)((noise.GetNoise2D(x, z) - 1f)
                    + 0.48 * noise.GetNoise2D(2 * x, 2 * z)
                    + 0.24 * noise.GetNoise2D(4 * x, 2 * z));

                e = (float)Math.Pow(e, exponent);
                e *= power;

                float distance = Mathf.Sqrt(Mathf.Abs(centerX - i) * Mathf.Abs(centerX - i) + Mathf.Abs(centerY - j) * Mathf.Abs(centerY - j));
                float normalizedDistance = distance / maxDistance;
                float heightValue = Mathf.Clamp(1.0f - normalizedDistance, 0, 1); // Высота уменьшается к краям

                mapHeight[i, j] = e * heightValue;  // Задаём высоту для точки с вышеуказанными координатами
            }
        }

        // Плато
        int platoSize = _terrainModel._TerrainData.PlatoSize;
        if (platoSize > 0)
        {
            int platoOffsetX = _terrainModel._TerrainData.PlatoOffsetX;
            int platoOffsetZ = _terrainModel._TerrainData.PlatoOffsetZ;

            plateauHeight = MaxHeightTerrain / 2;

            int startX = (int)(platoOffsetX - platoSize);
            int endX = (int)(platoOffsetX + platoSize);
            int startY = (int)(platoOffsetZ - platoSize);
            int endY = (int)(platoOffsetZ + platoSize);

            startX = Mathf.Clamp(startX, 0, mapHeight.GetLength(0) - 1);
            endX = Mathf.Clamp(endX, 0, mapHeight.GetLength(0) - 1);
            startY = Mathf.Clamp(startY, 0, mapHeight.GetLength(1) - 1);
            endY = Mathf.Clamp(endY, 0, mapHeight.GetLength(1) - 1);

            int count = 0;
            int rounding = (int)(platoSize / 3);
            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    count++;
                    plateauHeight += mapHeight[i, j];
                }
            }

            plateauHeight = Mathf.RoundToInt(plateauHeight / count);
            float maxDistanceP = platoSize;

            // Создаем плато на карте
            for (int i = startX; i <= endX; i++)
            {
                for (int j = startY; j <= endY; j++)
                {
                    float distance = Mathf.Sqrt(Mathf.Abs(platoOffsetX - i) * Mathf.Abs(platoOffsetX - i) + Mathf.Abs(platoOffsetZ - j) * Mathf.Abs(platoOffsetZ - j));
                    mapHeight[i, j] = (distance > maxDistanceP) ? mapHeight[i, j] : plateauHeight;
                }
            }
            // Сглаживаем края плато
            mapHeight = SmoothPlateauEdges(mapHeight, smoothFactor: 0.5, (int)(platoSize / 10));
        }

        int rows = mapHeight.GetLength(0); // Количество строк
        int columns = mapHeight.GetLength(1); // Количество столбцов

        _terrainModel.SetMapHeight(mapHeight);

        GD.Print($"Generate mapHeight. sizeX: {rows}, sizeY: {columns}"); // Вывод размеров массива
    }

    private float[,] SmoothPlateauEdges(float[,] heightMap, double smoothFactor, int sizeSmooth)
    {
        float[,] newHeightMap = new float[heightMap.GetLength(0), heightMap.GetLength(1)];

        for (int i = 0; i < heightMap.GetLength(0); i++)
        {
            for (int j = 0; j < heightMap.GetLength(1); j++)
            {
                if (IsPlateauEdge(i, j))
                {
                    newHeightMap[i, j] = CalculateSmoothedValue(heightMap, i, j, smoothFactor, sizeSmooth);
                }
                else
                {
                    newHeightMap[i, j] = heightMap[i, j];
                }
            }
        }

        return newHeightMap;
    }

    private bool IsPlateauEdge(int x, int y)
    {
        if (mapHeight[x, y] == plateauHeight) return true;
        return false;
    }

    private int CalculateSmoothedValue(float[,] heightMap, int x, int y, double smoothFactor, int sizeSmooth)
    {
        float sum = 0;
        int count = 0;

        for (int i = -sizeSmooth; i <= sizeSmooth; i++)
        {
            for (int j = -sizeSmooth; j <= sizeSmooth; j++)
            {
                if (x + i >= 0 && x + i < heightMap.GetLength(0) &&
                    y + j >= 0 && y + j < heightMap.GetLength(1))
                {
                    if (x + i < 0 || x + i > heightMap.GetLength(0)) continue;
                    if (y + j < 0 || y + j > heightMap.GetLength(0)) continue;

                    sum += heightMap[x + i, y + j];
                    count++;
                }
            }
        }

        int smoothedValue = (int)(sum / count * smoothFactor + heightMap[x, y] * (1 - smoothFactor));
        return smoothedValue;
    }

    private void Generate(Vector3 position, int numChunkX, int numChunkZ)
    {
        VoxLib.RemoveAllChildren(VoxLib.mapManager.navigationRegion3D);

        int width = countBlock + 1;
        int height = countBlock + 1;

        ArrayMesh mesh = new ArrayMesh();

        Vector3[] vertices = new Vector3[height * width];
        Vector2[] UVs = new Vector2[height * width];
        Vector4[] tangs = new Vector4[height * width];
        Vector3[] normals = new Vector3[height * width];

        int index;
        float pixelHeight;

        for (int x = 0; x < height; x++)
        {
            for (int z = 0; z < width; z++)
            {
                index = x * width + z;  // получаем индекс
                int indexX = numChunkX * countBlock + x;
                int indexZ = numChunkZ * countBlock + z;

                pixelHeight = mapHeight[indexX, indexZ];

                vertices[index] = new Vector3(x, pixelHeight, z); // задаём вершину
                UVs[index] = new Vector2(x, z); // задаём uv координату

                float heightOld = (x > 0) ? mapHeight[x - 1, z] : mapHeight[x, z];
                float heightNext = (x < countBlock - 1) ? mapHeight[x + 1, z] : mapHeight[x, z];

                Vector3 leftV = new Vector3(x - 1, heightOld, z);
                Vector3 rightV = new Vector3(x + 1, heightNext, z);
                Vector3 tang = (rightV - leftV).Normalized();
                tangs[index] = new Vector4(tang[0], tang[1], tang[2], 1);

                normals[index] = new Vector3(0, 1, 0);
                //normals[index] = new Vector3(tang[0], tang[1], tang[2]);
            }
        }

        index = 0;
        int[] triangles = new int[(height - 1) * (width - 1) * 6];
        for (int y = 0; y < height - 1; y++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // создаём полигон                 
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

        // Добавляем данные в mesh
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        mesh.RegenNormalMaps();

        staticBody = new StaticBody3D();
        staticBody.Name = NAMETERRAIN;

        // Создаем MeshInstance
        meshInstance = new MeshInstance3D();
        meshInstance.Mesh = mesh;

        var shaderMaterial = new ShaderMaterial();
        meshInstance.MaterialOverride = VoxLib.mapAssets.TerrainMat;

        // Коллизии
        CollisionShape3D collisionShape = new CollisionShape3D();
        ArrayMesh arrayMesh = new ArrayMesh();
        ConcavePolygonShape3D concaveShape = new ConcavePolygonShape3D();

        Vector3[] faces = mesh.GetFaces();
        concaveShape.SetFaces(faces);
        concaveShape.BackfaceCollision = true;

        collisionShape.Shape = concaveShape;
        collisionShape.Orthonormalize();

        staticBody.CollisionLayer = 1;
        staticBody.CollisionMask = 1;

        staticBody.AddChild(meshInstance);
        staticBody.AddChild(collisionShape);
        VoxLib.mapManager.navigationRegion3D.AddChild(staticBody);
        VoxLib.mapManager.navigationRegion3D.Position = positionOffset;

        GD.Print("meshInstance position=" + position);

        navMesh.AddPolygon(triangles);

        CreateNewMaterialTerrain();

        BakeNavMesh();

        GenerateEdgesMap();
    }

    private void CreateNewMaterialTerrain()
    {
        Material material = VoxLib.mapAssets.TerrainMat;

        Texture texture = VoxLib.mapAssets.terrainTexReplace[_terrainModel._TerrainData.ReplaceTexID];

        ShaderMaterial _shader = (ShaderMaterial)material;
        _shader.SetShaderParameter("_GrassTex", texture);
        _shader.SetShaderParameter("_GroundTex", texture);

        meshInstance.MaterialOverride = material;
    }

    private void GenerateEdgesMap()
    {
        VoxLib.RemoveAllChildren(VoxLib.terrainManager);

        float range = 1024;

        for (int n = 0; n < 4; n++)
        {
            int width = (n == 0 || n == 1) ? countBlock + 1 : 2;
            int height = (n == 2 || n == 3) ? countBlock + 1 : 2;

            ArrayMesh mesh = new ArrayMesh();

            Vector3[] vertices = new Vector3[height * width];
            Vector2[] UVs = new Vector2[height * width];
            Vector4[] tangs = new Vector4[height * width];
            Vector3[] normals = new Vector3[height * width];

            int index;
            float pixelHeight;

            for (int x = 0; x < height; x++)
            {
                for (int z = 0; z < width; z++)
                {
                    float _x = (n == 0 || n == 1) ? x * range : x;
                    float _z = (n == 2 || n == 3) ? z * range : z;

                    if (n == 0 && x == 0)
                    {
                        float t = z / (float)(width - 1);
                        _z = _z + Mathf.Lerp(-range, range, t);
                    }
                    else if (n == 1 && x == 1)
                    {
                        float t = z / (float)(width - 1);
                        _z = _z + Mathf.Lerp(-range, range, t);
                    }
                    if (n == 2 && z == 0)
                    {
                        float t = x / (float)(height - 1);
                        _x = _x + Mathf.Lerp(-range, range, t);
                    }
                    else if (n == 3 && z == 1)
                    {
                        float t = x / (float)(height - 1);
                        _x = _x + Mathf.Lerp(-range, range, t);
                    }

                    index = x * width + z; // получаем индекс

                    int indexX = x;
                    if (n == 0) indexX = 0;
                    if (n == 1) indexX = width - 1;
                    int indexZ = z;
                    if (n == 2) indexZ = 0;
                    if (n == 3) indexZ = height - 1;

                    pixelHeight = mapHeight[indexX, indexZ];
                    if (n == 0 && x == 0) pixelHeight -= 512;
                    else if (n == 1 && x == 1) pixelHeight -= 512;
                    if (n == 2 && z == 0) pixelHeight -= 512;
                    else if (n == 3 && z == 1) pixelHeight -= 512;

                    float uvx = (n == 0 || n == 1) ? x * range : x;
                    float uvz = (n == 2 || n == 3) ? z * range : z;

                    vertices[index] = new Vector3(_x, pixelHeight, _z); // задаём вершину
                    UVs[index] = new Vector2(uvx, uvz); // задаём uv координату
                    normals[index] = new Vector3(0, 1, 0);

                    float heightOld = (x > 0) ? mapHeight[x - 1, z] : mapHeight[x, z];
                    float heightNext = (x < countBlock - 1) ? mapHeight[x + 1, z] : mapHeight[x, z];

                    Vector3 leftV = new Vector3(x - 1, heightOld, z);
                    Vector3 rightV = new Vector3(x + 1, heightNext, z);
                    Vector3 tang = (rightV - leftV).Normalized();
                    tangs[index] = new Vector4(tang[0], tang[1], tang[2], 1);
                }
            }

            index = 0;
            int[] triangles = new int[(height) * (width) * 6];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // создаём полигон                 
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
            staticBody.Name = "EdgeMap" + n;

            MeshInstance3D EdgeMap = new MeshInstance3D();
            EdgeMap.Mesh = mesh;

            var shaderMaterial = new ShaderMaterial();
            EdgeMap.MaterialOverride = VoxLib.mapAssets.TerrainMat;

            staticBody.AddChild(EdgeMap);
            VoxLib.terrainManager.AddChild(staticBody);
            float tmpRangeX = 0;
            float delta = range / countBlock;

            if (n == 0) tmpRangeX = -range;
            if (n == 1) tmpRangeX = range / delta;
            float tmpRangeZ = 0;
            if (n == 2) tmpRangeZ = -range;
            if (n == 3) tmpRangeZ = range / delta;

            staticBody.Position = staticBody.Position + new Vector3(tmpRangeX, 0, tmpRangeZ) + positionOffset;
        }
    }

    private void ClearNavMesh()
    {
        if (navMesh != null)
        {
            navMesh.Clear();
            navMesh = null;
        }
        if (meshInstance != null)
        {
            if (meshInstance.Mesh != null) meshInstance.Mesh.Free();
            meshInstance.Free();
        }
        if (staticBody != null) staticBody.Free();
    }
}
