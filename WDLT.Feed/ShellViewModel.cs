using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.Feed.Enums;
using WDLT.Feed.Events;
using WDLT.Feed.Services;
using WDLT.Feed.ViewModels;
using WDLT.Feed.ViewModels.Flyouts;

namespace WDLT.Feed
{
    public class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IHandle<OpenFlyoutEvent>
    {
        public bool IsSnackbarOpen { get; set; }
        public string SnackbarText { get; private set; }
        public bool IsLoading { get; private set; }

        public BindableCollection<FlyoutBaseViewModel> Flyouts { get; }
        public ISnackbarMessageQueue SnackbarMessageQueue { get; }

        private readonly IEventAggregator _eventAggregator;
        private readonly List<BaseSubscriptionService> _subServices;

        public ShellViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackQueue, IEnumerable<BaseTabViewModel> tabs, IEnumerable<FlyoutBaseViewModel> flyouts, IEnumerable<BaseSubscriptionService> subServices)
        {
            _eventAggregator = eventAggregator;
            _subServices = new List<BaseSubscriptionService>(subServices);
            _eventAggregator.Subscribe(this);

            DisplayName = "The Feed v.0.1";

            Items.AddRange(tabs);
            ActiveItem = Items.First(f => ((BaseTabViewModel) f).Tab == ETab.Feed);

            Flyouts = new BindableCollection<FlyoutBaseViewModel>(flyouts);
            SnackbarMessageQueue = snackQueue;
        }

        protected override async void OnInitialActivate()
        {
            IsLoading = true;
            SnackbarText = "Loading Application...";
            IsSnackbarOpen = true;

            var initTasks = _subServices.Select(s => s.Init());
            var initResults = await Task.WhenAll(initTasks);

            var feedViewModel = (FeedViewModel)Items.First(f => ((BaseTabViewModel) f).Tab == ETab.Feed);
            await feedViewModel.LoadSubscriptionsAsync();
            await feedViewModel.LoadCardsAsync();

            if (initResults.Any(a => !a))
            {
                SnackbarText = "Please restart or try again later";
                return;
            }

            IsSnackbarOpen = false;
            SnackbarText = null;

            IsLoading = false;

            base.OnInitialActivate();
        }

        public void Handle(OpenFlyoutEvent message)
        {
            var flyout = Flyouts.FirstOrDefault(f => f.Flyout == message.Flyout);

            if (flyout != null)
            {
                flyout.IsOpen = true;
            }
        }

        public void Screenshot(FrameworkElement element)
        {
            var renderTargetBitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);

            renderTargetBitmap.Render(element);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            var bitmapImage = new BitmapImage();

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Seek(0, SeekOrigin.Begin);

                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            Clipboard.SetImage(bitmapImage);

            SnackbarMessageQueue.Enqueue("Screenshot copied to clipboard", null, null, null, false, false, TimeSpan.FromSeconds(2));
        }
    }
}