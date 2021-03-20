using System;
using MahApps.Metro.Controls;
using Stylet;
using WDLT.Feed.Enums;

namespace WDLT.Feed.ViewModels.Flyouts
{
    public abstract class FlyoutBaseViewModel : ValidatingModelBase
    {
        protected event Action<bool> OnOpenChanged;

        private bool _isOpen;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                SetAndNotify(ref _isOpen, value);
                OnOpenChanged?.Invoke(value);
            }
        }

        public EFlyout Flyout { get; }

        public Position Position { get; }

        protected FlyoutBaseViewModel(EFlyout flyout, Position position)
        {
            Flyout = flyout;
            Position = position;
        }

        protected FlyoutBaseViewModel(IModelValidator validator, EFlyout flyout, Position position) : base(validator)
        {
            Flyout = flyout;
            Position = position;
        }
    }
}