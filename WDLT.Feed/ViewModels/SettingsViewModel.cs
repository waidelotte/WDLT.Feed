using Stylet;
using WDLT.Feed.Enums;
using WDLT.Feed.Properties;

namespace WDLT.Feed.ViewModels
{
    public class SettingsViewModel : BaseTabViewModel
    {
        public SettingsViewModel(IEventAggregator eventAggregator) : base(ETab.Settings, eventAggregator)
        {
        }

        protected override void OnDeactivate()
        {
            Settings.Default.Save();
            base.OnDeactivate();
        }
    }
}