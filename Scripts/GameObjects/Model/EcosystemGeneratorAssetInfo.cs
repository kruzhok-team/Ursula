using System;
using Ursula.GameObjects.Model;

namespace ursula.addons.Ursula.Scripts.GameObjects.Model
{
    [Serializable]
    public class EcosystemGeneratorAssetInfo : GameObjectAssetInfo
    {
        public string Type = "Травоядное";
        public string Sex = "Мужской";
        public int PopulationCount = 35;
        public int Famine = 300;
        public int ChildCount = 1;

        public EcosystemGeneratorAssetInfo(string name, string providerId, GameObjectTemplate template) : base(name, providerId, template)
        {
        }

        public EcosystemGeneratorAssetInfo(GameObjectAssetInfo info) : base(info.Name, info.ProviderId, info.Template)
        {
        }
    }
}
