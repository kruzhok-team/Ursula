namespace Ursula.GameObjects.Model
{
    public static class CommonGameObjectLibraryItemExtensions
    {
        public static CommonGameObjectLibraryItem ToCommonItem(this IGameObjectAsset asset)
        {
            return asset != null ?new CommonGameObjectLibraryItem(asset.Info.Name, asset.Info.ProviderId) : null;
        }

        public static CommonGameObjectLibraryItem AsCommonItem(this GameObjectAssetInfo assetInfo)
        {
            return assetInfo != null ? new CommonGameObjectLibraryItem(assetInfo.Name, assetInfo.ProviderId) : null;
        }

    }
}
