using Fractural.Tasks;
using Godot;
using System;
using System.Collections.Generic;
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
            //VoxLib.mapManager.DeleteItem(node);
            _mapManagerModel.DeleteItem(node);
        }

        public Node CreateGameItem
            (
            GameObjectAssetInfo assetInfo,
            byte rotation,
            float scale,
            float x, 
            float y, 
            float z,
            int state,
            int id,
            bool isSnapGrid = false,
            string AssetFoderPath = ""
            )
        {

            IGameObjectAsset asset;
            bool isTryGetItem = _gameObjectLibraryManager.TryGetItem(assetInfo.Id, out asset);
            if (asset.Model3d == null) return null;

            Node3D node = asset.Model3d as Node3D;

            if (isTryGetItem == false)
            {
                node.Free();
                return null;
            }

            int _x = Mathf.RoundToInt(x);
            int _y = Mathf.RoundToInt(y);
            int _z = Mathf.RoundToInt(z);

            Vector3 newPos = new Vector3(_x, y, _z);
            if (isSnapGrid) newPos = new Vector3(_x, _y, _z);
            Quaternion rot = GetRotation(rotation);

            node.Position = newPos;
            node.Quaternion = rot;
            node.Scale = Vector3.One * scale;

            Node nodes = node.FindChild("ItemPropsScript", true, true);
            ItemPropsScript ips = nodes as ItemPropsScript;

            if (ips != null)
            {
                ips.AssetInfoId = assetInfo.Id;
                ips.id = id;
                ips.type = 0;
                ips.positionY = newPos.Y;
                ips.rotation = rotation;
                ips.state = state;
                ips.scale = scale;
                ips.GetParent().Name = $"{assetInfo.Name}_{_x}{_y}{_z}";
            }

            if (isTryGetItem)
            {
                _mapManagerModel.SetItemToMap(node, ips);
                GD.Print($"Create item={assetInfo.Id}, position={node.Position}");

                var obj = node.GetNode("InteractiveObject");
                var io = obj as InteractiveObject;
                if (io != null && !string.IsNullOrEmpty(assetInfo.Template.GraphXmlPath))
                {
                    io.xmlPath = assetInfo.Template.GraphXmlPath;
                    if (string.IsNullOrEmpty(AssetFoderPath))
                    {
                        io.xmlPath = $"{_gameObjectLibraryManager.GetAssetCollectionPath(assetInfo.Id)}{assetInfo.Template.Folder}/{io.xmlPath}";
                    }
                    else
                    {
                        io.xmlPath = AssetFoderPath + "/" + assetInfo.Template.GraphXmlPath;
                    }
                    io.ReloadAlgorithm();
                }

                if (io != null && assetInfo.Template.Sources.Audios != null)
                {
                    List<string> audios = new List<string>();
                    for (int j = 0; j < assetInfo.Template.Sources.Audios.Count; j++)
                    {
                        audios.Add($"{_gameObjectLibraryManager.GetAssetCollectionPath(assetInfo.Id)}{assetInfo.Template.Folder}/{assetInfo.Template.Sources.Audios[j]}");
                    }
                    io.SetAudiosPathes(audios);
                }
            }
            else
            {
                node.Free();
            }

            return node;
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