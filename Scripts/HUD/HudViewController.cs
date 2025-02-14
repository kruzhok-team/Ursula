using System;
using Ursula.Core.DI;
using Ursula.Core.View;
using Ursula.Environment.Settings;

namespace Ursula.HUD
{
    public class HudViewController : NodeController<global::HUD>
    {
        private ISingletonProvider<EnvironmentSettingsModel> _environmentSettingsProvider;

        public HudViewController(ISingletonProvider<EnvironmentSettingsModel> environmentSettingsProvider)
            : base("HUD")
        {
            _environmentSettingsProvider = environmentSettingsProvider;
        }

    }
}
