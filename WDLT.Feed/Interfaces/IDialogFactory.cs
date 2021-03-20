using WDLT.Feed.ViewModels.Dialogs;

namespace WDLT.Feed.Interfaces
{
    public interface IDialogFactory
    {
        BlacklistDialogViewModel CreateBlacklistViewModel();
        SimpleDialogViewModel CreateSimpleDialogViewModel();
        AcceptDialogViewModel CreateAcceptDialogViewModel();
    }
}