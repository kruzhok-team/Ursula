namespace Ursula.GameObjects.Model
{
    public class GameObjectAssetsUserSource : GameObjectAssetJsonCollection
    {
        public const string LibId = "UserGameObjectAssets";
        public const string JsonDataPath = "";

        public GameObjectAssetsUserSource() : base(LibId, JsonDataPath)
        {
        }
    }
}
