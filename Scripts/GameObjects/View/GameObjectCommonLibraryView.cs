using Fractural.Tasks;
using Godot;
using Ursula.Core.DI;
using UrsulaGoPlatformer3d.addons.Ursula.Scripts.GameObjects.Model;

namespace Ursula.GameObjects
{
    public partial class GameObjectCommonLibraryView : Node, IInjectable
    {
        [Export]
        private GameObjectCollectionView _collectionView;
        [Inject]
        private ISingletonProvider<IGameObjectLibraryManager> _commonLibraryProvider;

        public async GDTask Show()
        {
            await DrawCommonCollection();
        }

        private async GDTask DrawCommonCollection()
        {
            var commonLib = await _commonLibraryProvider.GetAsync();
            _collectionView?.Draw(commonLib.GetAll());
        }

        // Sync embedded assets in case when new objects added

        void IInjectable.OnDependenciesInjected() 
        { 
        }
    }
}
