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
        public event EventHandler RemoveCurrentInfoAudioEvent;
        public event EventHandler RemoveCurrentInfoAnimationEvent;
        

        public GameObjectAssetInfo currentAssetInfo { get; set; }

        public string AudioName { get; set; }
        public string AnimationName { get; set; }

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

        public GameObjectCurrentInfoModel RemoveAudio(string audioName)
        {
            AudioName = audioName;
            InvokeRemoveCurrentInfoAudioEvent();
            return this;
        }

        public GameObjectCurrentInfoModel RemoveAnimation(string animationName)
        {
            AnimationName = animationName;
            InvokeRemoveCurrentInfoAnimationEvent();
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

        private void InvokeRemoveCurrentInfoAudioEvent()
        {
            var handler = RemoveCurrentInfoAudioEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void InvokeRemoveCurrentInfoAnimationEvent()
        {
            var handler = RemoveCurrentInfoAnimationEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        
    }
}
