using Fractural.Tasks;
using Godot;
using System;
using System.IO;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using Ursula.MapManagers.Model;

namespace Ursula.MapManagers.Setters
{
    public partial class MapManagerItemSetter : Node, IInjectable
    {

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;

        [Inject]
        private ISingletonProvider<MapManager> _mapManagerProvider;

        [Inject]
        private ISingletonProvider<MapManagerModel> _mapManagerModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectCreateItemsModel> _gameObjectCreateItemsModelProvider;

        [Inject]
        private ISingletonProvider<GameObjectCollectionModel> _gameObjectCollectionModelProvider;



        private GameObjectLibraryManager _gameObjectLibraryManager;
        private MapManager _mapManager;
        private MapManagerModel _mapManagerModel;
        private GameObjectCreateItemsModel _gameObjectCreateItemsModel;
        private GameObjectCollectionModel _gameObjectCollectionModel;


        void IInjectable.OnDependenciesInjected()
        {

        }

        public override void _Ready()
        {
            base._Ready();
            _ = SubscribeEvent();
        }

        private async GDTask SubscribeEvent()
        {
            _gameObjectLibraryManager = await _gameObjectLibraryManagerProvider.GetAsync();
            _mapManager = await _mapManagerProvider.GetAsync();
            _mapManagerModel = await _mapManagerModelProvider.GetAsync();

            _gameObjectCreateItemsModel = await _gameObjectCreateItemsModelProvider.GetAsync();
            _gameObjectCollectionModel = await _gameObjectCollectionModelProvider.GetAsync();

            _gameObjectCreateItemsModel.GameObjectCreateItem_EventHandler += GameObjectCreateItems_CreateItemEventHandler;
            _gameObjectCreateItemsModel.GameObjectDeleteItem_EventHandler += GameObjectCreateItems_DeleteItemEventHandler;
        }

        private void GameObjectCreateItems_CreateItemEventHandler(object sender, EventArgs e)
        {
            CreateItem();
        }

        private void GameObjectCreateItems_DeleteItemEventHandler(object sender, EventArgs e)
        {
            DeleteItem();
        }

        private void CreateItem()
        {
            GameObjectAssetInfo asset = _gameObjectCollectionModel.AssetSelected;

            if (asset == null) return;

            Vector3 positionNode = _gameObjectCreateItemsModel.PositionNode;
            float scaleNode = _gameObjectCreateItemsModel.ScaleNode;
            byte rotationNode = _gameObjectCreateItemsModel.RotationNode;

            int _x = Mathf.RoundToInt(positionNode.X);
            int _y = Mathf.RoundToInt(positionNode.Y);
            int _z = Mathf.RoundToInt(positionNode.Z);

            int id = _x + _y * 256 + _z * 256 * 256;

            Node item = CreateGameItem(asset, rotationNode, scaleNode, _x, _y, _z, 0, id, true);


        }

        private void DeleteItem()
        {
            Node node = _gameObjectCreateItemsModel.Collider;
            VoxLib.mapManager.DeleteItem(node);
        }

        private Node CreateGameItem
            (
            GameObjectAssetInfo asset,
            byte rotation,
            float scale,
            float x, float y, float z,
            int state,
            int id,
            bool isSnapGrid = false
            )
        {

            PackedScene prefab = null;

            if (asset.ProviderId == GameObjectAssetsEmbeddedSource.LibId)
            {
                int numItem = -1;
                int.TryParse(asset.Sources.Model3dFilePath, out numItem);
                prefab = numItem >= 0 ? VoxLib.mapAssets.gameItemsGO[numItem] : null;

            }
            else if (asset.ProviderId == GameObjectAssetsUserSource.LibId)
            {
                prefab = VoxLib.mapAssets.customItemPrefab;
            }

            if (prefab == null) return null;

            int _x = Mathf.RoundToInt(x);
            int _y = Mathf.RoundToInt(y);
            int _z = Mathf.RoundToInt(z);

            Vector3 newPos = new Vector3(_x, y, _z);
            if (isSnapGrid) newPos = new Vector3(_x, _y, _z);

            Quaternion rot = GetRotation(rotation);

            Node instance = prefab.Instantiate();

            Node3D node = instance as Node3D;

            node.Position = newPos;
            node.Quaternion = rot;
            node.Scale = Vector3.One * scale;

            var itemProp = node.GetScript();
            ItemPropsScript ips = instance as ItemPropsScript;

            if (ips == null)
            {
                Node nodes = instance.FindChild("ItemPropsScript", true, true);
                ips = nodes as ItemPropsScript;
            }

            if (ips != null)
            {
                ips.id = id;
                ips.type = 0;
                ips.positionY = newPos.Y;
                ips.rotation = rotation;
                ips.state = state;
                ips.scale = scale;
                ips.itemId = asset.Id;
                //VoxLib.mapManager.gameItems.Add(ips);

                ips.GetParent().Name = Path.GetFileNameWithoutExtension(prefab.ResourcePath) + $"{_x}{_y}{_z}";
            }

            IGameObjectAsset asset1;
            bool isReady = _gameObjectLibraryManager.TryGetItem(asset.Id, out asset1);

            Node3D Model3d = asset1.Model3d as Node3D;
            instance.AddChild(Model3d);

            //VoxLib.mapManager.itemsGO.AddChild(instance);

            //_mapManager.ChangeWorldBytesItem(_x, _y, _z, itemToVox(numItem), (byte)(rotation + state * 6));

            //GD.Print($"Create item={numItem}, position={node.Position}");      

            _mapManagerModel.SetItemToMap(instance, ips);

            return instance;
        }

        private Quaternion GetRotation(byte rotation)
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

        private Quaternion LookRotation(Vector3 forward)
        {
            forward = forward.Normalized();
            Vector3 up = Vector3.Up.Normalized();

            Vector3 zAxis = forward;
            Vector3 xAxis = up.Cross(zAxis).Normalized();
            Vector3 yAxis = zAxis.Cross(xAxis);

            return new Basis(xAxis, yAxis, zAxis).GetRotationQuaternion();
        }
    }

}