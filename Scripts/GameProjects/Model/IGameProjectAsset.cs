namespace Ursula.EmbeddedGames.Model
{
    public interface IGameProjectAsset // Provide read only data here
    {
        GameProjectAssetInfo Info { get; }
        object PreviewImage { get; }
    }
}
