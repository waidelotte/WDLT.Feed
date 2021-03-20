using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using MahApps.Metro.Controls;
using PropertyChanged;
using Stylet;
using WDLT.Feed.Database;
using WDLT.Feed.Enums;
using WDLT.Feed.Events;
using WDLT.Feed.Helpers;
using WDLT.Feed.Services;

namespace WDLT.Feed.ViewModels.Flyouts
{
    public class SourceFlyoutViewModel : FlyoutBaseViewModel
    {
        private string _sourceRaw;

        public string SourceRaw
        {
            get => _sourceRaw;
            set
            {
                SetAndNotify(ref _sourceRaw, value);
                CanOk = ValidateProperty(() => SourceRaw);
            }
        }

        public string ErrorMessage { get; set; }
        public bool IsLoading { get; set; }

        private readonly IEventAggregator _eventAggregator;
        private readonly List<BaseSubscriptionService> _subServices;

        public SourceFlyoutViewModel(IEventAggregator eventAggregator, IModelValidator<SourceFlyoutViewModel> validator, IEnumerable<BaseSubscriptionService> subServices) : base(validator, EFlyout.Source, Position.Bottom)
        {
            _eventAggregator = eventAggregator;
            _subServices = new List<BaseSubscriptionService>(subServices);
            OnOpenChanged += FlyoutOpenChanged;
        }

        private void FlyoutOpenChanged(bool value)
        {
            if (!value)
            {
                SourceRaw = null;
                ErrorMessage = null;
                IsLoading = false;
                CanOk = false;
                ClearAllPropertyErrors();
            }
        }

        public bool CanOk { get; set; }

        public async Task Ok()
        {
            IsLoading = true;

            var uri = new Uri(SourceRaw, UriKind.Absolute);
            var src = uri.Host.HostToSource();

            if (src != null)
            {
                var service = _subServices.First(f => f.Source == src);

                try
                {
                    var subscription = await service.CreateSubscriptionAsync(uri);

                    if (subscription != null)
                    {
                        if (!subscription.IsProtected)
                        {
                            await using var db = new FeedDatabase();

                            if (!await db.HasSubscription(subscription))
                            {
                                _eventAggregator.Publish(new NewSubscriptionEvent(subscription));
                                IsOpen = false;
                            }
                            else
                            {
                                RecordPropertyError(() => SourceRaw, new[] { "Already subscribed" });
                            }
                        }
                        else
                        {
                            RecordPropertyError(() => SourceRaw, new[] { "Source is protected" });
                        }
                    }
                    else
                    {
                        RecordPropertyError(() => SourceRaw, new[] { "Invalid URL" });
                    }
                }
                catch (XmlException)
                {
                    RecordPropertyError(() => SourceRaw, new[] { "XML read Error" });
                }

            }
            else
            {
                RecordPropertyError(() => SourceRaw, new[] { "Invalid Source" });
            }

            IsLoading = false;
        }
    }
}