using FluentValidation;
using WDLT.Feed.ViewModels.Dialogs;

namespace WDLT.Feed.Helpers.Validation
{
    public class BlacklistValidator : AbstractValidator<BlacklistDialogViewModel>
    {
        public BlacklistValidator()
        {
            RuleFor(x => x.Input)
                .Cascade(CascadeMode.Stop)
                .MinimumLength(3)
                .WithMessage("Too short")
                .MaximumLength(35)
                .WithMessage("The word is too long");
        }
    }
}