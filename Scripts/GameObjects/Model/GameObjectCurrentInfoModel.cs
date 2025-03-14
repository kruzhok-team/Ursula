using Fractural.Tasks;
using Godot;
using System;
using Ursula.Core.DI;


namespace Ursula.GameObjects.Model
{
    public partial class GameObjectCurrentInfoModel : IInjectable
    {
        public bool isVisibleAssetInfo { get; set; }

        public event EventHandler VisibleCurrentAssetInfoEvent;
        public event EventHandler RemoveCurrentInfoGraphXmlEvent;


        GameObjectAssetInfo currentAssetInfo { get; set; }


        void IInjectable.OnDependenciesInjected()
        {
        }

        public GameObjectCurrentInfoModel SetAssetInfoView(GameObjectAssetInfo currentAssetInfo, bool visible)
        {
            if (currentAssetInfo != null) this.currentAssetInfo = currentAssetInfo;
            isVisibleAssetInfo = visible && this.currentAssetInfo != null;
            InvokeVisibleCurrentAssetInfoEvent();
            return this;
        }

        public GameObjectCurrentInfoModel RemoveGraphXml()
        {
            InvokeRemoveCurrentInfoGraphXmlEvent();
            return this;
        }

        private void InvokeRemoveCurrentInfoGraphXmlEvent()
        {
            var handler = RemoveCurrentInfoGraphXmlEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeVisibleCurrentAssetInfoEvent()
        {
            var handler = VisibleCurrentAssetInfoEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
