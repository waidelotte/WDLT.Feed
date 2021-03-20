using WDLT.Feed.Enums;

namespace WDLT.Feed.Events
{
    public class OpenFlyoutEvent
    {
        public EFlyout Flyout { get; }

        public OpenFlyoutEvent(EFlyout flyout)
        {
            Flyout = flyout;
        }
    }
}