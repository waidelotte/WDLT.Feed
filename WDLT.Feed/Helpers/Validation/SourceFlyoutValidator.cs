using System;
using FluentValidation;
using WDLT.Feed.ViewModels.Flyouts;

namespace WDLT.Feed.Helpers.Validation
{
    public class SourceFlyoutValidator : AbstractValidator<SourceFlyoutViewModel>
    {
        public SourceFlyoutValidator()
        {
            RuleFor(x => x.SourceRaw)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Source must be non-empty")
                .Must(ValidHost)
                .WithMessage("Enter a valid URL");
        }

        private bool ValidHost(string source)
        {
            return Uri.TryCreate(source, UriKind.Absolute, out _);
        }
    }
}