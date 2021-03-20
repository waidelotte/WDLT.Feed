using Stylet;

namespace WDLT.Feed.ViewModels.Dialogs
{
    public abstract class BaseDialogViewModel : ValidatingModelBase
    {
        protected BaseDialogViewModel() { }
        protected BaseDialogViewModel(IModelValidator validator) : base(validator)  {}
    }
}