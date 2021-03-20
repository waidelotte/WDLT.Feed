using System;
using Stylet;
using WDLT.Feed.Enums;

namespace WDLT.Feed.ViewModels
{
    public abstract class BaseTabViewModel : Screen, IDisposable, IHandle
    {
        public ETab Tab { get; }

        protected readonly IEventAggregator EventAggregator;

        protected BaseTabViewModel(ETab tab, IEventAggregator eventAggregator)
        {
            Tab = tab;
            DisplayName = tab.ToString();
            EventAggregator = eventAggregator;
            EventAggregator.Subscribe(this);
        }

        public virtual void Dispose()
        {

        }
    }
}