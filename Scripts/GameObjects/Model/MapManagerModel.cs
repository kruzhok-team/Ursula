using Godot;
using System;
using System.Collections.Generic;
using Ursula.Core.DI;
using Ursula.GameObjects.Model;
using VoxLibExample;


namespace Ursula.MapManagers.Model
{
    public class MapManagerData
    {
        public static string MapManagerAssetPath = "res://addons/Ursula/Assets/MapAssets.tres";

        public List<ItemPropsScript> gameItems { get; private set; }
        public Node3D itemsGO { get; private set; }

        public MapAssets MapAssets { get; private set; }

        public bool NeedSaveMap { get; set; }

        // Grid
        public VoxGrid _voxGrid;
        public VoxTypesGrid voxTypes;
        public VoxDataGrid voxData;

        // Params
        public int sizeX = 256, sizeY = 256, sizeZ = 256;

        public MapManagerData()
        {
            gameItems = new List<ItemPropsScript>();
            itemsGO = new Node3D();

            if (VoxLib.mapAssets == null)
            {
                var mapAssets = ResourceLoader.Load<MapAssets>(MapManagerAssetPath);
                VoxLib.mapAssets = mapAssets;
                this.MapAssets = mapAssets;
            }

            _voxGrid = new VoxGrid(sizeX, sizeY, sizeZ);
            voxTypes = new VoxTypesGrid(_voxGrid);
            voxData = new VoxDataGrid(_voxGrid);
        }




    }

    public partial class MapManagerModel : IInjectable
    {
        public MapManagerData _mapManagerData = new MapManagerData();

        [Inject]
        private ISingletonProvider<GameObjectLibraryManager> _gameObjectLibraryManagerProvider;



        private GameObjectLibraryManager _gameObjectLibraryManager;

        private int _xtype = 128;

        void IInjectable.OnDependenciesInjected()
        {
        }

        public void SetItemToMap(Node node, ItemPropsScript ips)
        {
            _mapManagerData.itemsGO.AddChild(node);
            _mapManagerData.gameItems.Add(ips);

            ChangeWorldBytesItem(ips.x, ips.y, ips.z, itemToVox(ips.type), (byte)(ips.rotation + ips.state * 6));
        }

        public void RemoveAllGameItems()
        {
            _mapManagerData.gameItems.Clear();
            VoxLib.RemoveAllChildren(_mapManagerData.itemsGO);
        }

        public void ChangeWorldBytesItem(int x, int y, int z, int type, byte prop)
        {
            if (_mapManagerData._voxGrid == null) return;

            int xtype = itemToVox(type);

            if (xtype != 0)
                _mapManagerData._voxGrid.Set(x, y, z, xtype, prop);

            _mapManagerData.NeedSaveMap = true;
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
                //ips.DeleteItem();

                if (_mapManagerData.gameItems.Contains(ips))
                {
                    _mapManagerData.gameItems.Remove(ips);
                    ChangeWorldBytesItem(ips.x, ips.y, ips.z, 0, 0);
                }

                parentNode?.QueueFree();
                item.QueueFree();
            }
        }


        private int itemToVox(int type)
        {
            return _xtype + type;
        }
        private int voxToItem(int xtype)
        {
            return xtype - _xtype;
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
