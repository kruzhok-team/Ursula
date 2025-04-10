using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Ursula.Core.DI;
using Ursula.Environment.Settings;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Controller;
using Ursula.MapManagers.Model;
using Ursula.MapManagers.Setters;
using Ursula.StartupMenu.Model;
using Ursula.Terrain.Model;
using Ursula.Water.Model;
using VoxLibExample;
using static Godot.TileSet;





public enum PlayMode
{
	buildingMode,
	testMode,
	playGameMode,
}

//[Tool]
public partial class MapManager : Node, IInjectable
{
	public const int CUSTOM_ITEM_INDEX_OFFSET = 1000;
    public const string PATHCATALOG = "/Models/Catalog/";
    public const string PATHCUSTOMGRASS = "/Models/Grass/";
    public const string PATHCUSTOMTREES = "/Models/Trees/";

    public const string PATHAUDIO = "Audio";
    public const string PATHANIMATION = "Animation";
    public const string PATHXML = "Graphs";
    public const string PATHMODEL = "Models";
    public const string GAMEIMAGE = "GameImage";
    public const string GAMEVIDEO = "GameVideo";

    [Export]
	public Control buildControl;

	[Export]
	public Control gameControl;

	[Export]
	public PackedScene CameraFreeGO { get; set; }

	[Export]
	public PackedScene PlayerGO { get; set; }

	[Export]
	public OptionButton IdItemOption;
    [Export]
    public OptionButton IdItemOptionCustom;
    [Export]
	public CheckBox CheckBoxCreateItem;
	[Export]
	public CheckBox CheckBoxDeleteItem;
	[Export]
	public ItemList CreateMode;
    [Export]
    public NavigationRegion3D navigationRegion3D;
	[Export]
	public FileDialog fileDialog;
    [Export]
    public FileDialog fileDialogLoad;
    [Export]
    public MapManagerItemSetter _mapManagerItemSetter;

    [Export(PropertyHint.Range, "0,1")]
	public float grassDensity = 0;
    [Export(PropertyHint.Range, "0,1")]
    public float plantsDensity = 0;
    [Export(PropertyHint.Range, "0,1")]
	public float treesDensity = 0;

	[Export(PropertyHint.Range, "0,1")]
	public float waterOffset = 1;

    [Export]
    public OptionButton TypeWaterOption;
    [Export]
	public VoxDrawTypes TypeWater = VoxDrawTypes.water;
    [Export]
    public CheckButton checkButtonWaterStatic;

    [Export]
	public HSlider sliderGrassTexID;
	[Export]
	public TextureRect ReplaceTexture;
    [Export]
    public CheckBox CheckBoxUsedOnlyCustomItem;

    [Export]
    TextureRect replaceTexUI;


    [Inject]
    private ISingletonProvider<EnvironmentSettingsModel> _settingsModelProvider;

    [Inject]
    private ISingletonProvider<MapManagerController> _mapManagerControllerProvider;

    [Inject]
    private ISingletonProvider<MapManagerModel> _mapManagerModelProvider;

    [Inject]
    private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

    [Inject]
    private ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;

    [Inject]
    private ISingletonProvider<GameObjectCurrentInfoModel> _GameObjectCurrentInfoModelProvider;

    [Inject]
    private ISingletonProvider<TerrainModel> _terrainModelProvider;

    [Inject]
    private ISingletonProvider<WaterModel> _waterModelProvider;

    [Inject]
    private ISingletonProvider<TerrainManager> _terrainManagerProvider;

    [Inject]
    private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;


    public int[] grassItemsID = new int[] { 10, 11, 12, 13, 14, 15, 16, 17, 18 };
	public int[] treesItemsID = new int[] { 0, 1, 3, 5, 6, 7, 8 };
    public int[] plantsItemsID = new int[] { 39, 40, 41, 42, 43 };
    public int[] ShowItemsID = new int[] { 0, 1, 3, 5, 6, 7, 8, 10, 13, 17, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43 };

    public PlayMode playMode = PlayMode.buildingMode;

    // Params
    public int sizeX = 256, sizeY = 256, sizeZ = 256;

	public QuadData phys;

	// Grid
	public VoxGrid _voxGrid;
	public VoxTypesGrid voxTypes;
	public VoxDataGrid voxData;

    public List<ItemPropsScript> gameItems { get { return _mapManagerModel._mapManagerData.gameItems; } }

    public Node3D itemsGO;

    public byte tempRotation = 0;
    public float tempScale = 1;

    public Node3D currentPlayerGO
    {
        get
        {
            return playMode == PlayMode.buildingMode ? playerBuild : playerTest;
        }
        set
        {
            if (playMode == PlayMode.buildingMode) playerBuild = value; else playerTest = value;
        }
    }

    public string IMPORTPROJECTPATH
    {
        get
        {
#if TOOLS
            return ProjectSettings.GlobalizePath(GetProjectFolderPath()) + "/ImportProject";
#else
            string executablePath = OS.GetExecutablePath();
            return System.IO.Path.GetDirectoryName(executablePath) +"/ImportProject";          
#endif
        }
    }

    private MapManagerModel _mapManagerModel { get; set; }
    private GameObjectLibraryManager _gameObjectLibraryManager { get; set; }
    private GameObjectCreateItemsModel _gameObjectCreateItemsModel { get; set; }
    private GameObjectCurrentInfoModel _gameObjectCurrentInfoModel { get; set; }
    private TerrainModel _terrainModel { get; set; }
    private WaterModel _waterModel { get; set; }
    private TerrainManager _terrainManager { get; set; }
    private GameObjectCollectionModel _gameObjectCollectionModel { get; set; }

    bool needSaveMap = false;
	bool usedCustomItemBuild = false;

    int skybox = 0;

    bool initialized = false;

    Node3D playerBuild;
    Node3D playerTest;

	int replaceTexID = 0;

    string pathMap = null;
    string gameImagePath = null;
    string gameVideoPath = null;

    void IInjectable.OnDependenciesInjected()
    {
    }

    public override void _Ready()
	{
		//if (VoxLib.mapManager != null) VoxLib.mapManager.Free();
		VoxLib.mapManager = this;

        GenerateProjectFolder();

        //GenerateNewWorld();
        SetAssetsData();
		InitPlayer();

        _ = SubscribeEvent();
    }

    public void GenerateProjectFolder()
    {
        var path = GetProjectFolderPath();
		CreateDir(path);
		CreateDir(path + "/Audio/");
		CreateDir(path + "/Models/");
		CreateDir(path + "/Graphs/");
        CreateDir(path + PATHCUSTOMGRASS);
        CreateDir(path + PATHCUSTOMTREES);
        CreateDir(path + PATHCATALOG);
        CreateDir(GameObjectAssetsUserSource.CollectionPath);
        CreateDir(GameObjectAssetsEmbeddedSource.CollectionPath);
    }

	public static void CreateDir(string path)
	{
        var dir = DirAccess.Open(path);

        if (dir == null)
            DirAccess.MakeDirRecursiveAbsolute(path);
    }

	public void OpenProjectFolderWithExplorer()
	{
		OpenInExplorer(ProjectSettings.GlobalizePath(GetProjectFolderPath()));
    }

    public string GetProjectFolderPath()
    {
        return "user://Project"; // Путь к папке проекта
    }

    public string GetCurrentProjectFolderPath()
    {
        if (playMode == PlayMode.playGameMode)
            return Path.GetDirectoryName(pathMap);
        else
            return "user://Project";
    }

    public void OpenInExplorer(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (System.Exception ex)
        {
            GD.PrintErr("Ошибка при открытии папки: ", ex.Message);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsKeyPressed(Key.R) && @event.IsPressed())
        {
			tempRotation++;
			if (tempRotation > 3) tempRotation = 0;
        }

        if (Input.IsKeyPressed(Key.Shift) && Input.IsKeyPressed(Key.Q) && @event.IsPressed())
        {
            AdaptItemsToLandscape();
        }
    }

    public void AdaptItemsToLandscape()
    {
        for (int i = 0; i < gameItems.Count; i++)
        {
            ItemPropsScript ips = gameItems[i];
            float heightLandscape = TerrainManager.instance.mapHeight[ips.x, ips.z];

            ChangeWorldBytesItem(ips.x, ips.y, ips.z, (byte)0, (byte)0);
            if (VoxLib.mapManager.voxTypes != null) VoxLib.mapManager.voxTypes[ips.x, ips.y, ips.z] = 0;
            if (VoxLib.mapManager.voxData != null) VoxLib.mapManager.voxData[ips.x, ips.y, ips.z] = 0;
            if (VoxLib.mapManager._voxGrid != null) VoxLib.mapManager._voxGrid.Set(ips.x, ips.y, ips.z, 0);

            float y = heightLandscape + TerrainManager.instance.positionOffset.Y;

            Node3D parent = ips.GetParent() as Node3D;
            parent.Position = new Vector3(ips.x, y, ips.z);

            ChangeWorldBytesItem(ips.x, Mathf.RoundToInt(y), ips.z, itemToVox(ips.type), (byte)(ips.rotation + ips.state * 6));
            ips.positionY = y;
        }
    }

    protected void Init(int _sizeX, int _sizeY, int _sizeZ, bool needCreateTex = false)
	{
		initialized = true;

		GD.Print($"Init: {_sizeX} {_sizeY} {_sizeZ}");

		phys = new QuadData(sizeX, sizeY, sizeZ);

		InitializeGrids();

		if (itemsGO == null)
		{
			itemsGO = new Node3D();
			itemsGO.Name = "ItemsGO";
			this.AddChild(itemsGO);
		}
	}

	void InitializeGrids()
	{
		_voxGrid = new VoxGrid(sizeX, sizeY, sizeZ);
		voxTypes = new VoxTypesGrid(_voxGrid);
		voxData = new VoxDataGrid(_voxGrid);
	}

	public void GenerateNewWorld()
	{
        if (VoxLib.mapManager != this) return;
        StartCoroutineCreateTerrain(true);
	}

    public Node CreateGameItem(int numItem, byte rotation, float scale, float x, float y, float z, int state, int id,
		bool isSnapGrid = false)
	{
		int _x = Mathf.RoundToInt(x);
		int _y = Mathf.RoundToInt(y);
		int _z = Mathf.RoundToInt(z);

		Vector3 newPos = new Vector3(_x, y, _z);
		if (isSnapGrid) newPos = new Vector3(_x, _y, _z);

		PackedScene prefab = null;

		if (numItem < CUSTOM_ITEM_INDEX_OFFSET)
		{
			prefab = VoxLib.mapAssets.gameItemsGO[numItem];

			if (prefab == null)
			{
				GD.Print("No item {0} prefab ", numItem);
				return null;
			}
		}
		else
		{
            prefab = VoxLib.mapAssets.customItemPrefab;
        }

		Quaternion rot = GetRotation(rotation);

        Node instance = prefab.Instantiate();

		Node3D node = instance as Node3D;

		node.Position = newPos;
		node.Quaternion = rot;
		node.Scale = Vector3.One * scale;

        itemsGO.AddChild(instance);

		var itemProp = node.GetScript();
        ItemPropsScript itemPropS = instance as ItemPropsScript;

		if (itemPropS == null)
		{
            Node nodes = instance.FindChild("ItemPropsScript", true, true);
            itemPropS = nodes as ItemPropsScript;
        }

        if (itemPropS != null)
		{
            itemPropS.id = id;
			itemPropS.type = numItem;
            itemPropS.positionY = newPos.Y;
            itemPropS.rotation = rotation;
            itemPropS.state = state;
            itemPropS.scale = scale;
            gameItems.Add(itemPropS);

            //itemPropS.GetParent().Name = Path.GetFileNameWithoutExtension(prefab.ResourcePath) + $"{_x}{_y}{_z}";
        }

		ChangeWorldBytesItem(_x, _y, _z, itemToVox(numItem), (byte)(rotation + state * 6));

        //GD.Print($"Create item={numItem}, position={node.Position}");      

        return instance;

    }

	public async GDTask StartCoroutineCreateTerrain(bool randomHeight)
	{
        if (VoxLib.mapManager != this) return;

        _mapManagerModel.RemoveAllGameItems();

        initialized = false;

        Init(sizeX, sizeY, sizeZ);

        await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
		GD.Print("1 second passed!");

		if (VoxLib.mapManager == this && VoxLib.terrainManager != null)
		{
            _terrainModel.StartGenerateTerrain(false);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        return;
    }

	private async void SetAssetsData()
	{
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

		if (VoxLib.mapAssets == null) return;

		for (int i = 0; i < VoxLib.mapAssets.gameItemsGO.Length; i++)
		{
			Texture2D texture = null;

			if (i < VoxLib.mapAssets.inventarItemTex.Length) texture = (Texture2D)VoxLib.mapAssets.inventarItemTex[i];

			if (ShowItemsID.Contains(i))
			{
				if (texture != null)
				{
					Image image = texture.GetImage();
					//image.Resize(40, 40);
					var newTexture = new ImageTexture();
					newTexture = ImageTexture.CreateFromImage(image);
					IdItemOption.AddIconItem(newTexture, "id=" + i, i);

				}
				else
				{
					IdItemOption.AddItem("id=" + i, i);
				}
			}
		}

		if (CreateMode != null)
		{
			CreateMode.Select(0);
		}

    }

	public void PlayTest()
	{
        if (currentPlayerGO != null)
        {
            var obj = currentPlayerGO.GetChild(0) as Node3D;

            positionP = obj.Position;
            rotationP = obj.Rotation;

            currentPlayerGO.Free();
			currentPlayerGO = null;
        }

        if (playMode == PlayMode.buildingMode)
		{
			playMode = PlayMode.testMode;
            CustomObject.SetVisibleIndicators(false);
        }
		else
		{
			playMode = PlayMode.buildingMode;
            CustomObject.SetVisibleIndicators(true);
        }

		VoxLib.terrainManager.BakeNavMesh();

		InstancePlayer();

        _gameObjectCurrentInfoModel.SetAssetInfoView(null, playMode == PlayMode.buildingMode);

    }

    public async void PlayGame()
	{
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");

        if (currentPlayerGO != null)
        {
            var obj = currentPlayerGO.GetChild(0) as Node3D;

            positionP = obj.Position;
            rotationP = obj.Rotation;

            currentPlayerGO.Free();
            currentPlayerGO = null;
        }

        playMode = PlayMode.playGameMode;
        CustomObject.SetVisibleIndicators(false);
        VoxLib.terrainManager.BakeNavMesh();
        await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        InstancePlayer();
        VoxLib.log.HideLog();

        await ToSignal(GetTree().CreateTimer(5.0f), "timeout");
        VoxLib.hud.RunAllObjects();

        _gameObjectCurrentInfoModel.SetAssetInfoView(null, false);
    }

    public Camera3D GetPlayerCamera()
	{
        if (playMode == PlayMode.buildingMode)
		{
			return currentPlayerGO.GetChild(0) as Camera3D;

        }
		else
		{
            return currentPlayerGO.GetChild(0).GetNode("Camera3D") as Camera3D;

        }
    }

	public void SetCameraCursorShow(bool state)
	{
		if (playMode == PlayMode.buildingMode)
		{
			var cc = GetPlayerCamera() as CameraController;
			cc.CursorShow(state);
		}
		else
		{
            var cc = GetPlayerCamera().GetParent() as PlayerScript;
			cc.CursorShow(state);
        }

    }

	private async void InitPlayer()
	{
		await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

		float height = VoxLib.mapManager.sizeY / 4;
		//height += 2f;

        positionP = VoxLib.terrainManager.positionOffset + new Vector3(VoxLib.mapManager.sizeX / 2, height + 1, VoxLib.mapManager.sizeZ / 2);
        rotationP = Vector3.Zero;
		positionP.Z = VoxLib.mapManager.sizeZ - 1;

        InstancePlayer();
	}

	Vector3 positionP;
    Vector3 rotationP;

    private void InstancePlayer()
	{
		PackedScene prefab = playMode == PlayMode.buildingMode ? CameraFreeGO : PlayerGO;

		Node instance = prefab.Instantiate();
		Node3D node = new Node3D();
		node.AddChild(instance);

        ((Node3D)instance).Position = positionP + ((playMode == PlayMode.buildingMode) ? new Vector3(0,1,0) : Vector3.Zero);
        ((Node3D)instance).Rotation = rotationP;

		AddChild(node);

		if (playMode == PlayMode.buildingMode) playerBuild = node;
		else playerTest = node;

		if (playMode == PlayMode.testMode && VoxLib.terrainManager.mapHeight != null)
		{
			float height = VoxLib.terrainManager.mapHeight[(int)(((Node3D)instance).Position.X / 2), (int)(((Node3D)instance).Position.Z / 2)] + VoxLib.terrainManager.positionOffset.Y;
			if (((Node3D)instance).Position.Y < height || ((Node3D)instance).Position.Y >= VoxLib.mapManager.sizeY/2)
			{
                ((Node3D)instance).Position = new Vector3(((Node3D)instance).Position.X, height + 2, ((Node3D)instance).Position.Z);
            }
		}
        else if (playMode == PlayMode.playGameMode)
        {
            float height = VoxLib.terrainManager.mapHeight[(int)(((Node3D)instance).Position.X / 2), (int)(((Node3D)instance).Position.Z / 2)] + VoxLib.terrainManager.positionOffset.Y;
            ((Node3D)instance).Position = VoxLib.terrainManager.positionOffset + new Vector3(VoxLib.mapManager.sizeX / 2, height + 1, VoxLib.mapManager.sizeZ / 2);
        }
    }

	public void ChangeWorldBytesItem(int x, int y, int z, int type, byte prop)
	{
		if (_voxGrid == null) return;

		int xtype = itemToVox(type);

		if (xtype != 0)
			_voxGrid.Set(x, y, z, xtype, prop);

		needSaveMap = true;
	}

	private int _xtype = 128;
	private int itemToVox(int type)
	{
		return _xtype + type;
	}
	private int voxToItem(int xtype)
	{
		return xtype - _xtype;
	}

	public Quaternion GetRotation(byte rotation)
	{
		return rotation switch
		{
			(byte)GameItemRotation.forward => LookRotation(Vector3.Forward),
			(byte)GameItemRotation.backward => LookRotation(-Vector3.Forward),
			(byte)GameItemRotation.right => LookRotation(Vector3.Right),
			(byte)GameItemRotation.left => LookRotation(-Vector3.Right),
			(byte)GameItemRotation.up => LookRotation(Vector3.Up),
			(byte)GameItemRotation.down => LookRotation(-Vector3.Up),
			_ => Quaternion.Identity,
		};
	}

    public static Quaternion LookRotation(Vector3 forward)
	{
		forward = forward.Normalized();
		Vector3 up = Vector3.Up.Normalized();

		Vector3 zAxis = forward;
		Vector3 xAxis = up.Cross(zAxis).Normalized();
		Vector3 yAxis = zAxis.Cross(xAxis);

		return new Basis(xAxis, yAxis, zAxis).GetRotationQuaternion();
	}

	public void GenerateNewPlants()
	{
		GD.Print("GenerateNewPlants...");

		VoxLib.RemoveAllChildren(itemsGO);

		if (VoxLib.terrainManager == null || VoxLib.terrainManager.mapHeight == null) return;

		float[,] mapHeight = VoxLib.terrainManager.mapHeight;

		int amountGrass = Mathf.Min((int)(sizeX * sizeY * grassDensity / 64), sizeX * sizeY / 64);
		int amountTrees = Mathf.Min((int)(sizeX * sizeY * treesDensity / 64), sizeX * sizeY / 64);
        int amountPlants = Mathf.Min((int)(sizeX * sizeY * plantsDensity / 64), sizeX * sizeY / 64);

        bool isOnlyCustomItems = CheckBoxUsedOnlyCustomItem.ButtonPressed;
        string[] allGrass = GetAllCustomGrass();
        string[] allTrees = GetAllCustomTrees();

        for (int i = 0; i < amountGrass; i++)
		{
			int x = Mathf.RoundToInt(GD.Randi() % (sizeX - 1));
			int z = Mathf.RoundToInt(GD.Randi() % (sizeZ - 1));
			int y = Mathf.RoundToInt(VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y);
			int id = x + z * 256 + y * 256 * 256;

			float positionY = VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y;

			int type = grassItemsID[GD.Randi() % grassItemsID.Length];
			if (isOnlyCustomItems) type = CUSTOM_ITEM_INDEX_OFFSET + Mathf.RoundToInt(GD.Randi() % (allGrass.Length));

			if ((y > VoxLib.mapManager.WaterLevel || _terrainModel._TerrainData.Power == 0) && _voxGrid.Getdata(x, y, z) == 0 && voxTypes[x, y, z] == 0)
			{
				Node node = CreateGameItem(type, 0, 1, x, positionY, z, 0, id);
                if (isOnlyCustomItems) InitCustomItem(node, type);
            }
		}

		for (int i = 0; i < amountTrees; i++)
		{
			int x = Mathf.RoundToInt(GD.Randi() % (sizeX - 1));
			int z = Mathf.RoundToInt(GD.Randi() % (sizeZ - 1));
			int y = Mathf.RoundToInt(VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y);
			int id = x + z * 256 + y * 256 * 256;

			float positionY = VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y;

			int type = treesItemsID[GD.Randi() % treesItemsID.Length];
            if (isOnlyCustomItems) type = CUSTOM_ITEM_INDEX_OFFSET + Mathf.RoundToInt(GD.Randi() % (allTrees.Length)) + allGrass.Length;

            if ((y > VoxLib.mapManager.WaterLevel || _terrainModel._TerrainData.Power == 0) && _voxGrid.Getdata(x, y, z) == 0 && voxTypes[x, y, z] == 0)
			{
				Node node = CreateGameItem(type, 0, 1, x, positionY, z, 0, id);
                if (isOnlyCustomItems) InitCustomItem(node, type);
            }
		}

        for (int i = 0; i < amountPlants; i++)
        {
            int x = Mathf.RoundToInt(GD.Randi() % (sizeX - 1));
            int z = Mathf.RoundToInt(GD.Randi() % (sizeZ - 1));
            int y = Mathf.RoundToInt(VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y);
            int id = x + z * 256 + y * 256 * 256;

            float positionY = VoxLib.terrainManager.mapHeight[x, z] + VoxLib.terrainManager.positionOffset.Y;

            int type = plantsItemsID[GD.Randi() % plantsItemsID.Length];
            if (isOnlyCustomItems) type = CUSTOM_ITEM_INDEX_OFFSET + Mathf.RoundToInt(GD.Randi() % (allTrees.Length)) + allGrass.Length;

            if ((y > VoxLib.mapManager.WaterLevel || _terrainModel._TerrainData.Power == 0) && _voxGrid.Getdata(x, y, z) == 0 && voxTypes[x, y, z] == 0)
            {
                Node node = CreateGameItem(type, 0, 1, x, positionY, z, 0, id);
                if (isOnlyCustomItems) InitCustomItem(node, type);
            }
        }

    }

    public void CreateWater()
	{
		if (VoxLib.terrainManager == null) return;
		if (VoxLib.terrainManager.mapHeight == null) return;

		var maxHeightTerrain = VoxLib.terrainManager.MaxHeightTerrain;

		Vector3 size = new Vector3(VoxLib.terrainManager.size, maxHeightTerrain - (maxHeightTerrain * waterOffset), VoxLib.terrainManager.size);

		bool isStatic = !checkButtonWaterStatic.ButtonPressed;

		if (waterOffset == 1)
			VoxLib.waterManager.DeleteWater();
		else
			VoxLib.waterManager.GenerateWater(_terrainModel._TerrainData.Size, isStatic);
	}

	public float WaterLevel
	{
		get
		{
            //if (VoxLib.terrainManager == null) return 0;
            //float lvl = (VoxLib.terrainManager.MaxHeightTerrain - (VoxLib.terrainManager.MaxHeightTerrain * VoxLib.mapManager.waterOffset)) / 2;
            //return lvl + VoxLib.terrainManager.positionOffset.Y;

            return _waterModel._WaterData.WaterLevel;
        }
	}

	public void ChangeWaterOffset(float value)
	{
		if (VoxLib.terrainManager != null) waterOffset = 1 - value;
	}
	public void ChangeWaterType(int value)
	{
		TypeWater = (VoxDrawTypes)TypeWaterOption.GetSelectedId();
    }
	public void ChangeGrassDensity(float value)
	{
		if (VoxLib.terrainManager != null) grassDensity = value;
    }
    public void ChangePlantsDensity(float value)
    {
        if (VoxLib.terrainManager != null) plantsDensity = value;
    }
    public void ChangeTreesDensity(float value)
	{
		if (VoxLib.terrainManager != null) treesDensity = value;
	}
	public void SetCreateItem(bool value)
	{
		isCreateItem = true;

		CheckBoxCreateItem.ToggleMode = isCreateItem;
		CheckBoxDeleteItem.ToggleMode = !isCreateItem;
	}
	public void SetDeleteItem(bool value)
	{
		isCreateItem = false;
		CheckBoxCreateItem.ToggleMode = !isCreateItem;
		CheckBoxDeleteItem.ToggleMode = isCreateItem;
	}

	private bool isCreateItem;
	public void StartCreateItem()
	{
		isCreateItem = !isCreateItem;
	}

    public override void _Process(double delta)
    {
        base._Process(delta);

        buildControl.Visible = playMode == PlayMode.buildingMode;
        gameControl.Visible = playMode == PlayMode.testMode;

        if (playMode == PlayMode.buildingMode)
        {
            buildControl.Show();
            gameControl.Hide();
        }
        else
        {
            buildControl.Hide();

            if (playMode == PlayMode.testMode)
                gameControl.Show();
            else
                gameControl.Hide();
        }
    }

    public void DeleteItem(Node item)
	{
		Node3D parentNode = item.GetParentOrNull<Node3D>();
		var script = parentNode.GetScript();

        ItemPropsScript ips = item as ItemPropsScript;
        if (ips == null)
		{
			Node node = parentNode.FindChild("ItemPropsScript", true, true);
            ips = node as ItemPropsScript;
        }
        if (ips == null)
		{
            ips = parentNode as ItemPropsScript;
        }
        if (ips == null)
        {
            Node node = item.FindChild("ItemPropsScript", true, true);
            ips = node as ItemPropsScript;
            if (ips != null) parentNode = null;
        }

        if (ips != null)
        {
            ips.DeleteItem();
            parentNode?.QueueFree();
            item.QueueFree();
        }
    }

    string currentFile = "";
    public void SaveMap()
	{
        DisconnectAll();

        ControlPopupMenu.HideAllMenu();

        //fileDialog.PopupCentered();
        //fileDialog.Connect("file_selected", new Callable(this, "SaveMapToFile"));

        fileDialog.Filters = new string[] { "*.txt ; txt" };
        if (currentFile.Contains(".zip")) currentFile = Path.GetFileNameWithoutExtension(currentFile) + ".txt";

        fileDialog.CurrentFile = currentFile;
        lastDirectory = LoadLastDirectory;

        CheckButton checkButton = fileDialog.FindChild("AccessCheckButton", true, false) as CheckButton;
        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }

        if (!string.IsNullOrEmpty(lastDirectory))
        {
            fileDialog.CurrentDir = lastDirectory;
        }

        if (!fileDialog.IsConnected("file_selected", new Callable(this, nameof(SaveMapToFile))))
        {
            fileDialog.Connect("file_selected", new Callable(this, nameof(SaveMapToFile)));
        }

        fileDialog.Show();
    }

    private void OnCheckButtonToggled(bool buttonPressed)
    {
        if (buttonPressed)
        {
            fileDialog.Access = FileDialog.AccessEnum.Filesystem;
            GD.Print("Access: Filesystem");
        }
        else
        {
            fileDialog.Access = FileDialog.AccessEnum.Userdata;
            GD.Print("Access: Userdata");
        }

        if (buttonPressed)
        {
            fileDialogLoad.Access = FileDialog.AccessEnum.Filesystem;
            GD.Print("Access: Filesystem");
        }
        else
        {
            fileDialogLoad.Access = FileDialog.AccessEnum.Userdata;
            GD.Print("Access: Userdata");
        }
    }

    public void SaveMapToFile(string file)
    {
        fileDialog.Disconnect("file_selected", new Callable(this, nameof(SaveMapToFile)));

        string savePath = file;
        if (savePath.Contains("user://Project")) savePath = ProjectSettings.GlobalizePath(savePath);

        //string mapData = SaveWorld();
        Dictionary<string, string> mapData = SaveWorld();

        using (StreamWriter writer = new StreamWriter(savePath))
		{
            //writer.Write(mapData);
            foreach (var line in mapData)
            {
                writer.WriteLine(line);
            }
        }

        lastDirectory = fileDialog.CurrentDir;
        SaveLastDirectory(lastDirectory);
        currentFile = fileDialog.CurrentFile;
        pathMap = file;
        GD.Print("Map saved to: " + Path.GetFileName(savePath));

        VoxLib.ShowMessage("Проект сохранен в файл: " + savePath);
	}

    public Dictionary<string, string> SaveWorld(bool import = false)
	{
        //List<String> mapData = new List<string>();

        int version = 0x0A02;
		//mapData.Add(version.ToString());
		//mapData.Add(skybox.ToString());

		Dictionary<string, string> mapData = new Dictionary<string, string>();
		mapData.Add("version", version.ToString());
        mapData.Add("sizeX", sizeX.ToString());
        mapData.Add("sizeY", sizeY.ToString());
        mapData.Add("sizeZ", sizeZ.ToString());
        mapData.Add("skybox", skybox.ToString());
        mapData.Add("grassTexID", _terrainModel._TerrainData.ReplaceTexID.ToString());
        mapData.Add("waterOffset", _waterModel._WaterData.WaterOffset.ToString());
        mapData.Add("TypeWater", ((int)_waterModel._WaterData.TypeWaterID).ToString());
        bool isStatic = _waterModel._WaterData.IsStaticWater;
        mapData.Add("StaticWater", ((bool)isStatic).ToString());
        mapData.Add("FullDayLength", DayNightCycle.instance.FullDayLength.ToString());

        List<ItemPropsScript> saveItems = new List<ItemPropsScript>(_mapManagerModel._mapManagerData.gameItems);
        mapData.Add("saveItems", saveItems.Count.ToString());

        for (int i = 0; i < saveItems.Count; i++)
        {
            if (saveItems[i] == null) continue;

            Dictionary<string, string> itemData = new Dictionary<string, string>();
            itemData.Add("AssetInfoId", saveItems[i].AssetInfoId);
            itemData.Add("id", saveItems[i].id.ToString());
            itemData.Add("type", saveItems[i].type.ToString());
            itemData.Add("positionY", saveItems[i].positionY.ToString());
            itemData.Add("rotation", saveItems[i].rotation.ToString());
            itemData.Add("state", saveItems[i].state.ToString());
            itemData.Add("x", saveItems[i].x.ToString());
            itemData.Add("y", saveItems[i].y.ToString());
            itemData.Add("z", saveItems[i].z.ToString());
            itemData.Add("scale", saveItems[i].scale.ToString());

            //Node item = saveItems[i] as Node;
            //var obj = item.GetNodeOrNull("InteractiveObject");
            //if (obj == null) obj = item.GetParent().FindChild("InteractiveObject", true, true);
            //var io = obj as InteractiveObject;
            //if (io != null && io.xmlPath != null)
            //{
            //    itemData.Add("xmlPath", io.xmlPath);
            //}

            //var custom = item.GetNodeOrNull("CustomObjectScript");
            //if (custom == null) custom = item.GetParent().FindChild("CustomObjectScript", true, true);
            //var co = custom as CustomObject;
            //if (co != null && !string.IsNullOrEmpty(co.objPath))
            //{
            //    itemData.Add("objPath", co.objPath);
            //}

            //var customItem = item.GetNodeOrNull("CustomItemScript");
            //if (customItem == null) customItem = item.GetParent().FindChild("CustomItemScript", true, true);
            //var ci = customItem as CustomItem;
            //if (ci != null && !string.IsNullOrEmpty(ci.objPath))
            //{
            //    itemData.Add("objPath", ci.objPath);
            //}

            string ips = JsonSerializer.Serialize(itemData);
            mapData.Add("item" + i, ips);
        }

        float[,] mapHeight = VoxLib.terrainManager.mapHeight;
        if (mapHeight == null) mapHeight = new float[0, 0];

        mapData.Add("sizeTerrain", (VoxLib.terrainManager.size).ToString());

        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < VoxLib.terrainManager.size; y++)
        {
			string str = "";

            for (int x = 0; x < VoxLib.terrainManager.size; x++)
            {
                sb.Append(mapHeight[y, x]);
                if (x < mapHeight.GetLength(1) - 1)
                {
                    sb.Append(";");
                }
            }
            if (y < mapHeight.GetLength(1) - 1)
            {
                sb.Append(";");
            }
        }
        mapData.Add("mapHeight", sb.ToString());

        if (!string.IsNullOrEmpty(gameImagePath))
            mapData.Add("gameImagePath", gameImagePath);
        else gameImagePath = null;

        if (!string.IsNullOrEmpty(gameVideoPath))
            mapData.Add("gameVideoPath", gameVideoPath);
        else gameVideoPath = null;

        return mapData;
	}

    public void LoadMap()
	{
        DisconnectAll();

        ControlPopupMenu.HideAllMenu();

        //fileDialogLoad.PopupCentered();

        fileDialogLoad.Filters = new string[] { "*.txt ; txt" };

        lastDirectory = LoadLastDirectory;

        CheckButton checkButton = fileDialogLoad.FindChild("AccessCheckButton", true, false) as CheckButton;
        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }

        if (!string.IsNullOrEmpty(lastDirectory))
        {
            fileDialogLoad.CurrentDir = lastDirectory;
        }

        if (!fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(LoadMapFromFile))))
        {
            fileDialogLoad.Connect("file_selected", new Callable(this, nameof(LoadMapFromFile)));
        }

        fileDialogLoad.Show();
    }

	public void OnReloadMap()
	{
		if (pathMap != null) LoadMapFromFile(pathMap);
    }

    public string lastDirectory = "";
    private string settingsPath
	{
		get { return VoxLib.SETTINGPATH; }
	}

    // Сохранение пути в файл конфигурации
    public void SaveLastDirectory(string path)
    {
        if (TryGetSettingsModel(out var settingsModel))
            settingsModel.SetLastMapDirectory(path).Save();
    }

    // Загрузка пути из файла конфигурации
    public string LoadLastDirectory
    {
        get
        {
            if (!TryGetSettingsModel(out var settingsModel))
                return "";
            return settingsModel.LastMapDirectory;
        }
    }

    public async GDTask LoadMapFromFile(string fileName)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        //LoadingUI.instance.ShowLoading();

        lastDirectory = fileDialogLoad.CurrentDir;
        SaveLastDirectory(lastDirectory);
        currentFile = fileDialogLoad.CurrentFile;

        string loadPath = fileName;
        if (loadPath.Contains("user://Project")) loadPath = ProjectSettings.GlobalizePath(loadPath);
        string dirMap = Path.GetDirectoryName(ProjectSettings.GlobalizePath(fileName));

        string mapDataS = null;
        Dictionary<string, string> mapData = new Dictionary<string, string>();

        if (File.Exists(loadPath))
        {
            pathMap = loadPath;

            using (StreamReader reader = new StreamReader(loadPath))
            {
                //mapDataS = reader.ReadToEnd();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    AddStringToDictionary(mapData, line);
                }
            }
        }
        else
        {
            GD.Print("File not found: " + loadPath);
            return;
        }

        if (mapData == null) return;

        int version = 0x0A01;
        if (mapData.ContainsKey("version")) version = int.Parse(mapData["version"]);

        bool isImport = mapData.ContainsKey("ImportName");

        if (mapData.ContainsKey("sizeX")) sizeX = int.Parse(mapData["sizeX"]);
        if (mapData.ContainsKey("sizeY")) sizeY = int.Parse(mapData["sizeY"]);
        if (mapData.ContainsKey("sizeZ")) sizeZ = int.Parse(mapData["sizeZ"]);
        if (mapData.ContainsKey("skybox")) skybox = int.Parse(mapData["skybox"]);
        int sizeTerrain = 0;
        if (mapData.ContainsKey("sizeTerrain")) sizeTerrain = int.Parse(mapData["sizeTerrain"]);
        _terrainModel.SetSize(sizeTerrain);

        float[,] mapHeight = new float[sizeTerrain, sizeTerrain];
        if (mapData.ContainsKey("mapHeight"))
        {
            string[] _mapHeight = mapData["mapHeight"].Split(';');

            int i = 0;

            for (int y = 0; y < VoxLib.terrainManager.size; y++)
            {
                string str = "";

                for (int x = 0; x < VoxLib.terrainManager.size; x++)
                {
                    mapHeight[y, x] = float.Parse(_mapHeight[i]);
                    i++;
                }
            }

            VoxLib.terrainManager.mapHeight = mapHeight;
            _terrainModel.SetMapHeight(mapHeight);

            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
            await StartCoroutineCreateTerrain(false);
            await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        }

        if (mapData.ContainsKey("grassTexID")) replaceTexID = int.Parse(mapData["grassTexID"]);
        _terrainModel.SetReplaceTexID(replaceTexID);

        if (mapData.ContainsKey("waterOffset")) waterOffset = float.Parse(mapData["waterOffset"]);
        _waterModel.SetWaterOffset(waterOffset);

        if (mapData.ContainsKey("TypeWater")) TypeWater = (VoxDrawTypes)int.Parse(mapData["TypeWater"]);
        _waterModel.SetTypeWaterID((int)TypeWater);

        bool isStatic = false;
        if (mapData.ContainsKey("StaticWater")) isStatic = (bool)bool.Parse(mapData["StaticWater"]);
        _waterModel.SetStaticWater(isStatic);

        if (mapData.ContainsKey("FullDayLength"))
        {
            float fullDayLength = float.Parse(mapData["FullDayLength"]);
            DayNightCycle.instance.SetFullDayLength(fullDayLength);
            _terrainModel.SetFullDayLength(fullDayLength);
        }
        if (waterOffset == 1)
        {
            VoxLib.waterManager.DeleteWater();
        }
        else
        {
            VoxLib.waterManager.GenerateWater(sizeX, isStatic);
        }

        int saveItems = 0;
        if (mapData.ContainsKey("saveItems")) saveItems = int.Parse(mapData["saveItems"]);

        for (int i = 0; i < saveItems; i++)
        {
            if (mapData.ContainsKey("item" + i))
            {
                string items = mapData["item" + i];
                Dictionary<string, string> itemData = new Dictionary<string, string>();
                itemData = JsonSerializer.Deserialize<Dictionary<string, string>>(items);
                if (itemData.Count > 0)
                {
                    int numItem = int.Parse(itemData["type"]);
                    byte rotation = byte.Parse(itemData["rotation"]);
                    int state = int.Parse(itemData["state"]);
                    int x = int.Parse(itemData["x"]);
                    int y = int.Parse(itemData["y"]);
                    int z = int.Parse(itemData["z"]);

                    float scale = itemData.ContainsKey("scale") ? float.Parse(itemData["scale"]) : 1f;

                    float positionY = y;
                    if (itemData.ContainsKey("positionY"))
                    {
                        positionY = float.Parse(itemData["positionY"]);
                    }

                    int id = x + z * 256 + y * 256 * 256;

                    //Node item = CreateGameItem(numItem, rotation, scale, x, positionY, z, state, id);

                    //var obj = item.GetNode("InteractiveObject");
                    //var io = obj as InteractiveObject;

                    //if (io != null && itemData.ContainsKey("xmlPath"))
                    //{
                    //    {
                    //        io.xmlPath = itemData["xmlPath"];
                    //        if (isImport) io.xmlPath = dirMap + "/" + io.xmlPath;
                    //        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
                    //        io.ReloadAlgorithm();
                    //        //io.StartAlgorithm();
                    //    }
                    //}

                    ////await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

                    //var custom = item.GetNodeOrNull("CustomObjectScript");
                    //if (custom == null) custom = item.GetParent().FindChild("CustomObjectScript", true, true);
                    //var co = custom as CustomObject;
                    //if (co != null && itemData.ContainsKey("objPath"))
                    //{
                    //    co.objPath = itemData["objPath"];
                    //    if (isImport) co.objPath = dirMap + "/" + co.objPath;
                    //    co.InitModel();
                    //}

                    //var customItem = item.GetNodeOrNull("CustomItemScript");
                    //if (customItem == null) customItem = item.GetParent().FindChild("CustomItemScript", true, true);
                    //var ci = customItem as CustomItem;
                    //if (ci != null && itemData.ContainsKey("objPath"))
                    //{
                    //    ci.objPath = itemData["objPath"];
                    //    if (isImport) ci.objPath = dirMap + "/" + ci.objPath;
                    //    ci.InitModel();
                    //}



                    if (itemData.ContainsKey("AssetInfoId"))
                    {
                        GameObjectAssetInfo assetInfo = _gameObjectLibraryManager.GetItemInfo(itemData["AssetInfoId"]);

                        string assetFolder = dirMap;
                        if (!isImport)
                        {
                            assetFolder = $"{assetInfo.GetAssetPath()}";
                        }

                        Node item = _mapManagerItemSetter.CreateGameItem
                        (
                            assetInfo,
                            rotation,
                            scale,
                            x,
                            y,
                            z,
                            state,
                            id,
                            false,
                            assetFolder
                        );

                    }
                }
            }
        }

        if (mapData.ContainsKey("gameImagePath"))
            gameImagePath = mapData["gameImagePath"];

        if (mapData.ContainsKey("gameVideoPath"))
            gameVideoPath = mapData["gameVideoPath"];

        VoxLib.hud?.SetNameMap(Path.GetFileName(pathMap));

        //LoadingUI.instance.HideLoading();

        return;
    }

    public void AddStringToDictionary(Dictionary<string, string> dict, string input)
    {
        string[] parts = input.Trim('[', ']').Split(new[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            dict[key] = value;
        }
    }

    public void SetGrassTexture(float id)
    {
        replaceTexID = (int)id;
        replaceTexUI.Texture = (Texture2D)VoxLib.mapAssets.terrainTexReplace[replaceTexID];
        _terrainModel.SetReplaceTexID(replaceTexID);
    }

    string[] GetAllCustomGrass()
	{
        string directoryGrassPath = ProjectSettings.GlobalizePath(GetProjectFolderPath() + PATHCUSTOMGRASS);
        string[] allGrass = Directory.GetFiles(directoryGrassPath, "*.obj").Concat(Directory.GetFiles(directoryGrassPath, "*.glb")).ToArray();
		return allGrass;
    }

    string[] GetAllCustomTrees()
    {
        string directoryTreesPath = ProjectSettings.GlobalizePath(GetProjectFolderPath() + PATHCUSTOMTREES);
        string[] allTrees = Directory.GetFiles(directoryTreesPath, "*.obj").Concat(Directory.GetFiles(directoryTreesPath, "*.glb")).ToArray();
        return allTrees;
    }

	void InitCustomItem(Node item, int numItem)
	{
        var custom = item.FindChild("CustomItemScript", true, true);
        if (custom != null)
        {
            CustomItem customItem = custom as CustomItem;
            int idx = IdItemOptionCustom.GetItemIndex(numItem);
            string nameItem = IdItemOptionCustom.GetItemText(idx);
            string objPath = GetProjectFolderPath() + nameItem;
            customItem.objPath = objPath;
            customItem.InitModel();
        }
    }

	public void SetUsedStandartItemBuild()
	{
		usedCustomItemBuild = false;
	}
    public void SetUsedCustomItemBuild()
    {
        usedCustomItemBuild = true;
    }

    public void ExportProject()
    {
        DisconnectAll();

        fileDialog.Filters = new string[] { "*.zip ; zip" };
        fileDialog.CurrentDir = IMPORTPROJECTPATH;

        if (string.IsNullOrEmpty(pathMap))
        {
            VoxLib.ShowMessage("Нет сохраненного проекта для импорта.");
            return;
        }

        if (fileDialog.CurrentPath.Length > 0)
        {
            string name = Path.GetFileNameWithoutExtension(pathMap);
            fileDialog.CurrentPath = Path.GetDirectoryName(fileDialog.CurrentPath) + "/" + name + ".zip";
        }

        if (!fileDialog.IsConnected("file_selected", new Callable(this, nameof(ExportProject))))
        {
            fileDialog.Connect("file_selected", new Callable(this, nameof(ExportProject)));
        }

        fileDialog.Show();
    }

    public void ImportProject()
	{
        DisconnectAll();

        fileDialogLoad.Filters = new string[] { "*.zip ; zip" };
        fileDialogLoad.CurrentDir = IMPORTPROJECTPATH;

        CheckButton checkButton = fileDialogLoad.FindChild("AccessCheckButton", true, false) as CheckButton;
        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }

        if (!fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(ImportProject))))
        {
            fileDialogLoad.Connect("file_selected", new Callable(this, nameof(ImportProject)));
        }

        fileDialogLoad.Show();
    }

    public void OpenGameImage()
    {
        DisconnectAll();

        if (string.IsNullOrEmpty(pathMap))
        {
            VoxLib.ShowMessage("Нет открытого проекта игры.");
            return;
        }

        fileDialogLoad.Filters = new string[] { "*.png ; png", "*.jpg ; jpg" };
        fileDialogLoad.CurrentDir = "";
        fileDialogLoad.CurrentFile = "";

        CheckButton checkButton = fileDialogLoad.FindChild("AccessCheckButton", true, false) as CheckButton;
        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }

        if (!fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(SaveGameImage))))
        {
            fileDialogLoad.Connect("file_selected", new Callable(this, nameof(SaveGameImage)));
        }

        fileDialogLoad.Show();
    }

    public void OpenGameVideo()
    {
        DisconnectAll();

        if (string.IsNullOrEmpty(pathMap))
        {
            VoxLib.ShowMessage("Нет открытого проекта игры.");
            return;
        }

        fileDialogLoad.Filters = new string[] { "*.ogv ; ogv" };
        fileDialogLoad.CurrentDir = "";
        fileDialogLoad.CurrentFile = "";

        CheckButton checkButton = fileDialogLoad.FindChild("AccessCheckButton", true, false) as CheckButton;
        if (checkButton != null)
        {
            checkButton.Connect("toggled", new Callable(this, nameof(OnCheckButtonToggled)));
        }
        else
        {
            GD.Print("CheckButton не найден среди дочерних узлов.");
        }

        if (!fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(SaveGameVideo))))
        {
            fileDialogLoad.Connect("file_selected", new Callable(this, nameof(SaveGameVideo)));
        }

        fileDialogLoad.Show();
    }

    public bool isDialogsOpen
    {
        get
        {
            bool isDialogsOpen = fileDialog.Visible || fileDialogLoad.Visible;
            return isDialogsOpen;
        }
    }

    public Texture2D GetGameImage(string folderPath)
    {
        List<string> images = FindImageFiles(folderPath);
        if (images.Count > 0)
        {
            string pathTex = images[0];
            Image img = new Image();
            var err = img.Load(pathTex);

            if (err != Error.Ok)
            {
                GD.Print("Failed to load image from path: " + gameImagePath);
            }
            else
            {
                ImageTexture texture = ImageTexture.CreateFromImage(img);
                return texture;
            }
        }

        return null;
    }

    private async GDTask SubscribeEvent()
    {
        _mapManagerModel = await _mapManagerModelProvider.GetAsync();
        _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
        _gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();
        _gameObjectCurrentInfoModel = await _GameObjectCurrentInfoModelProvider.GetAsync();
        _terrainModel = await _terrainModelProvider.GetAsync();
        _waterModel = await _waterModelProvider.GetAsync();
        _terrainManager = await _terrainManagerProvider.GetAsync();
        _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();
    }

    private List<string> FindImageFiles(string folderPath)
    {
        string[] img = Directory.GetFiles(folderPath, "*.jpg").Concat(Directory.GetFiles(folderPath, "*.png")).ToArray();
        var imgFiles = new List<string>(img);
        return imgFiles;
    }

    private bool TryGetSettingsModel(out EnvironmentSettingsModel model, bool errorIfNotExist = false)
    {
        model = null;

        if (!(_settingsModelProvider?.TryGet(out model) ?? false))
        {
            if (errorIfNotExist)
                GD.PrintErr($"{typeof(MapManager).Name}: {typeof(EnvironmentSettingsModel).Name} is not instantiated!");
        }
        return model != null;
    }

    private void DisconnectAll()
    {
        if (fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(SaveMapToFile)))) fileDialogLoad.Disconnect("file_selected", new Callable(this, nameof(SaveMapToFile)));
        if (fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(LoadMapFromFile)))) fileDialogLoad.Disconnect("file_selected", new Callable(this, nameof(LoadMapFromFile)));
        if (fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(SaveGameImage)))) fileDialogLoad.Disconnect("file_selected", new Callable(this, nameof(SaveGameImage)));
        if (fileDialogLoad.IsConnected("file_selected", new Callable(this, nameof(ImportProject)))) fileDialogLoad.Disconnect("file_selected", new Callable(this, nameof(ImportProject)));

        if (fileDialog.IsConnected("file_selected", new Callable(this, nameof(ExportProject)))) fileDialog.Disconnect("file_selected", new Callable(this, nameof(ExportProject)));
        if (fileDialog.IsConnected("file_selected", new Callable(this, nameof(SaveMapToFile)))) fileDialog.Disconnect("file_selected", new Callable(this, nameof(SaveMapToFile)));
    }

    private async void SaveGameImage(string fileName)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        gameImagePath = fileName;
        SaveMapToFile(pathMap);
    }

    private async void SaveGameVideo(string fileName)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        gameVideoPath = fileName;
        SaveMapToFile(pathMap);
    }


    private async void ImportProject(string fileName)
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        string pathImport = IMPORTPROJECTPATH;
        string pathImportProject = pathImport + "/" + Path.GetFileNameWithoutExtension(fileName);

        if (Directory.Exists(pathImport)) Directory.CreateDirectory(pathImport);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (Directory.Exists(pathImportProject)) Directory.Delete(pathImportProject, true);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (fileName.Contains("user://Project")) fileName = ProjectSettings.GlobalizePath(fileName);

        ZipFile.ExtractToDirectory(fileName, pathImportProject);

        VoxLib.ShowMessage($"Проект {Path.GetFileName(fileName)} распакован в папку {pathImportProject}");

        fileDialogLoad.Disconnect("file_selected", new Callable(this, nameof(ImportProject)));

        VoxLib.instance.CGP.Instantiate();
    }


    private async void ExportProject(string fileName)
    {
        DisconnectAll();

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        string loadPath = pathMap;
        string importName = Path.GetFileName(fileName); // Path.GetFileNameWithoutExtension(pathMap) + ".txt";

        string pathImport = IMPORTPROJECTPATH;
        string pathImportProject = pathImport + "/" + Path.GetFileNameWithoutExtension(pathMap);
        string pathImportMap = pathImportProject + "/" + Path.GetFileName(pathMap);

        if (File.Exists(loadPath))
        {
            //pathMap = loadPath;			
            if (!File.Exists(pathImport)) Directory.CreateDirectory(pathImport);
            if (!File.Exists(pathImportProject)) Directory.CreateDirectory(pathImportProject);
            //File.Copy(pathMap, pathImportMap, true);
        }

        Dictionary<string, string> mapData = SaveWorld();

        if (mapData == null || string.IsNullOrEmpty(pathImportMap)) return;

        mapData.Add("ImportName", importName);

        int saveItems = 0;
        if (mapData.ContainsKey("saveItems")) saveItems = int.Parse(mapData["saveItems"]);

        for (int i = 0; i < saveItems; i++)
        {
            if (mapData.ContainsKey("item" + i))
            {
                string items = mapData["item" + i];
                Dictionary<string, string> itemData = new Dictionary<string, string>();
                itemData = JsonSerializer.Deserialize<Dictionary<string, string>>(items);
                if (itemData.Count > 0)
                {
                    if (itemData.ContainsKey("xmlPath"))
                    {
                        string path;
                        if (itemData["xmlPath"].Contains("user:")) path = ProjectSettings.GlobalizePath(itemData["xmlPath"]);
                        else
                            path = Path.GetFullPath(itemData["xmlPath"]);

                        string name = Path.GetFileName(path);
                        if (!Directory.Exists(pathImportProject + "/" + PATHXML)) Directory.CreateDirectory(pathImportProject + "/" + PATHXML);
                        string xmlPath = Path.GetFullPath(pathImportProject + "/" + PATHXML + "/" + name);
                        itemData["xmlPath"] = PATHXML + "/" + name;
                        if (!File.Exists(xmlPath)) File.Copy(path, xmlPath, true);
                    }

                    if (itemData.ContainsKey("objPath"))
                    {
                        string path;
                        if (itemData["objPath"].Contains("user:")) path = ProjectSettings.GlobalizePath(itemData["objPath"]);
                        else
                            path = Path.GetFullPath(itemData["objPath"]);

                        string name = Path.GetFileName(path);
                        if (!Directory.Exists(pathImportProject + "/" + PATHMODEL)) Directory.CreateDirectory(pathImportProject + "/" + PATHMODEL);
                        string objPath = Path.GetFullPath(pathImportProject + "/" + PATHMODEL + "/" + name);
                        itemData["objPath"] = PATHMODEL + "/" + name;
                        if (!File.Exists(objPath)) ModelLoader.CopyModel(path, pathImportProject + "/" + PATHMODEL + "/");
                    }
                }

                mapData.Remove("item" + i);
                string ips = JsonSerializer.Serialize(itemData);
                mapData.Add("item" + i, ips);
            }
        }

        if (!Directory.Exists(pathImportProject + "/" + PATHAUDIO)) Directory.CreateDirectory(pathImportProject + "/" + PATHAUDIO);
        string[] audios = Directory.GetFiles(ProjectSettings.GlobalizePath(InteractiveObjectAudio.PATH_AUDIO));
        foreach (string audio in audios)
        {
            string pathAudio = pathImportProject + "/" + PATHAUDIO + "/" + Path.GetFileName(audio);
            File.Copy(audio, pathAudio, true);
        }

        if (mapData.ContainsKey("gameImagePath"))
        {
            string gameImagePath = mapData["gameImagePath"];
            mapData.Remove("gameImagePath");

            string imageName = Path.GetFileName(gameImagePath);

            string ext = Path.GetExtension(gameImagePath);
            string gameImagePathImport = pathImportProject + "/" + GAMEIMAGE + ext;
            mapData.Add("gameImagePath", GAMEIMAGE + ext);

            File.Copy(gameImagePath, gameImagePathImport, true);
        }

        if (mapData.ContainsKey("gameVideoPath"))
        {
            string gameVideoPath = mapData["gameVideoPath"];
            mapData.Remove("gameVideoPath");

            string imageName = Path.GetFileName(gameVideoPath);

            string ext = Path.GetExtension(gameVideoPath);
            string gameVideoPathImport = pathImportProject + "/" + GAMEVIDEO + ext;
            mapData.Add("gameVideoPath", GAMEVIDEO + ext);

            File.Copy(gameVideoPath, gameVideoPathImport, true);
        }

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        if (File.Exists(pathImportMap)) File.Delete(pathImportMap);

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        using (StreamWriter writer = new StreamWriter(pathImportMap))
        {
            foreach (var line in mapData)
            {
                writer.WriteLine(line);
            }
        }

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        string zipFile = ProjectSettings.GlobalizePath(fileName);// pathImport + "/" + Path.GetFileNameWithoutExtension(pathMap) + ".zip";
        if (File.Exists(zipFile)) File.Delete(zipFile);
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        ZipFile.CreateFromDirectory(pathImportProject, zipFile);

        VoxLib.ShowMessage($"Проект {Path.GetFileNameWithoutExtension(pathMap)} упакован в архив {zipFile}");

        OpenInExplorer(pathImport);
    }


}
