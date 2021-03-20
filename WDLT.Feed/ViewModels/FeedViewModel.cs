using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CefSharp.Wpf;
using EFCore.BulkExtensions;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Stylet;
using Swordfish.NET.Collections;
using WDLT.Feed.Database;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Enums;
using WDLT.Feed.Events;
using WDLT.Feed.Helpers;
using WDLT.Feed.Models;
using WDLT.Feed.Properties;
using WDLT.Feed.Services;
using WDLT.Utils.Extensions;
using DialogManager = WDLT.Feed.Helpers.DialogManager;
using TreeView = System.Windows.Controls.TreeView;

namespace WDLT.Feed.ViewModels
{
    public class FeedViewModel : BaseTabViewModel, IHandle<NewSubscriptionEvent>
    {
        public bool IsCardsLoading { get; private set; }
        public double LoadingValue { get; private set; }

        public BindableCollection<AppSourceList> Subscriptions { get; }

        public ConcurrentObservableSortedCollection<DBCard> Cards { get; }

        private DBCard _selectedCard;
        public DBCard SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (value != null)
                {
                    value.IsViewed = true;
                    if (value.OriginalUrl != null) Browser.Load(value.OriginalUrl);
                }

                SetAndNotify(ref _selectedCard, value);
            }
        }

        private object _selectedSubscriptionTreeItem;
        public object SelectedSubscriptionTreeItem
        {
            get => _selectedSubscriptionTreeItem;
            set
            {
                if (value != null)
                {
                    switch (value)
                    {
                        case DBSubscription sub:
                            ShowCardsBySubscription(sub);
                            break;
                        case AppSourceList list:
                            ShowCardsBySource(list);
                            break;
                    }
                }
                else
                {
                    ShowAll();
                }

                SetAndNotify(ref _selectedSubscriptionTreeItem, value);
            }
        }

        public IWpfWebBrowser Browser { get; set; }

        private readonly List<BaseSubscriptionService> _subServices;
        private readonly ISnackbarMessageQueue _snackQueue;
        private readonly DialogManager _dialogManager;

        public FeedViewModel(IEventAggregator eventAggregator, ISnackbarMessageQueue snackQueue, IEnumerable<BaseSubscriptionService> subServices, DialogManager dialogManager) : base(ETab.Feed, eventAggregator)
        {
            _snackQueue = snackQueue;
            _dialogManager = dialogManager;
            _subServices = new List<BaseSubscriptionService>(subServices);
            Browser = new ChromiumWebBrowser();
            Subscriptions = new BindableCollection<AppSourceList>();
            Cards = new ConcurrentObservableSortedCollection<DBCard>(false, new CardComparer());
        }

        public void ExportSubscriptions()
        {
            var path = _dialogManager.PickFolderDialog();

            if (!string.IsNullOrWhiteSpace(path))
            {
                FileService.Serialize(Subscriptions.SelectMany(s => s.Sources).Select(s => new ExportSubscription(s)), path, "mySubs");
            }
        }

        public async Task ImportSubscriptions()
        {
            var path = _dialogManager.PickFileDialog();

            var des = FileService.Deserialize<IEnumerable<ExportSubscription>>(path);

            if (des != null)
            {
                await using var db = new FeedDatabase();

                foreach (var sub in des.Select(s => new DBSubscription
                {
                    SourceId = s.SourceId,
                    Source = s.Source,
                    IsProtected = s.IsProtected,
                    Username = s.Username
                }))
                {
                    if (!await db.HasSubscription(sub))
                    {
                        await db.Subscriptions.AddAsync(sub);
                        AddSubscription(sub);
                    }
                }

                await db.SaveChangesAsync();
            }
            else
            {
                _snackQueue.Enqueue("Import file error", null, null, null, false, false, TimeSpan.FromSeconds(2));
            }
        }

        public void OnCardMouseEnter(object sender, EventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is DBCard card)
            {
                card.IsViewed = true;
            }
        }

        public void SubTreePreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Right) return;

            if (sender is TreeView treeView)
            {
                if (treeView.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem item)
                {
                    item.IsSelected = true;
                    item.IsSelected = false;
                }
            }
        }

        public async Task AddWordToBlacklist(DBSubscription subscription)
        {
            var newWords = await _dialogManager.ShowBlacklistDialogAsync(subscription);

            if (newWords.Any())
            {
                var removeCards = Cards.Where(w => w.SubscriptionId == subscription.Id)
                    .Where(w => newWords.Any(a => w.Text.Contains(a.Word, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                await using var db = new FeedDatabase();
                await db.BulkDeleteAsync(removeCards);

                Cards.RemoveRange(removeCards);
            }
        }

        public async Task RenameSubscription(DBSubscription subscription)
        {
            if(subscription == null) return;

            var answer = await _dialogManager.ShowSimpleDialogAsync("Enter a new Name", subscription.Username);

            if (!string.IsNullOrWhiteSpace(answer) && !string.Equals(answer, subscription.Username, StringComparison.OrdinalIgnoreCase))
            {
                await using var db = new FeedDatabase();

                db.Attach(subscription);
                subscription.Username = answer;

                await db.SaveChangesAsync();
            }
        }

        public async Task ChangeUriOfSubscription(DBSubscription subscription)
        {
            if (subscription == null) return;

            var answer = await _dialogManager.ShowSimpleDialogAsync("Enter a new URL", subscription.SourceId);

            if (!string.IsNullOrWhiteSpace(answer) && !string.Equals(answer, subscription.SourceId, StringComparison.OrdinalIgnoreCase))
            {
                await using var db = new FeedDatabase();

                if (!await db.HasSubscription(subscription))
                {
                    db.Attach(subscription);
                    subscription.SourceId = answer;
                    await db.SaveChangesAsync();
                }
                else
                {
                    _snackQueue.Enqueue("Source already exist!", null, null, null, true, false, TimeSpan.FromSeconds(2));
                }
            }
        }

        public async Task RemoveSubscription(DBSubscription subscription)
        {
            if (subscription == null) return;

            if (await _dialogManager.ShowAcceptDialogAsync("Are you sure?"))
            {
                Subscriptions.FirstOrDefault(f => f.Source == subscription.Source)?.Sources.Remove(subscription);
                Cards.RemoveRange(Cards.Where(w => w.SubscriptionId == subscription.Id).ToList());

                await using var db = new FeedDatabase();
                db.Subscriptions.Remove(subscription);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateCardsAsync()
        {
            IsCardsLoading = true;
            ShowAll();

            await using var db = new FeedDatabase();

            var subs = await db.Subscriptions.ToListAsync();

            var newCards = new List<DBCard>();

            foreach (var (sub, index) in subs.WithIndex())
            {
                var service = _subServices.First(f => f.Source == sub.Source);

                try
                {
                    var cards = await service.GetCardsAsync(sub);
                    if(cards == null) continue;

                    var filtered = cards
                        .Where(w => w.Timestamp.AddDays(Settings.Default.CardsRestrictOldInDays) >= DateTimeOffset.Now)
                        .Where(w => w.Timestamp >= sub.LastTimestamp)
                        .Where(w => !sub.Blacklist.Any(a => w.Text.Contains(a.Word, StringComparison.OrdinalIgnoreCase)))
                        .Where(w => sub.Cards.All(f => f.CardId != w.CardId))
                        .ToList();

                    if (filtered.Any())
                    {
                        sub.Cards.AddRange(filtered);
                        sub.LastTimestamp = filtered.OrderByDescending(o => o.Timestamp).First().Timestamp;

                        newCards.AddRange(filtered);
                    }

                }
                catch (Exception)
                {
                    _snackQueue.Enqueue($"Error loading source [{sub.Source}][{sub.Username}]", null, null, null, false, true, TimeSpan.FromSeconds(2));
                }

                LoadingValue = 100d * (index + 1) / subs.Count;
            }

            await db.SaveChangesAsync();

            LoadingValue = 0;

            if (newCards.Any())
            {
                Cards.AddRange(newCards);
                Cards.RemoveRange(Cards.Where(w => w.IsViewed && w.Timestamp.AddDays(Settings.Default.CardsRestrictOldInDays) < DateTimeOffset.Now && !w.IsBookmark).ToList());
                SelectedCard = newCards.OrderBy(o => o.Timestamp).FirstOrDefault(f => !f.IsViewed);
            }

            IsCardsLoading = false;
        }

        public async Task DeleteCards()
        {
            await using var db = new FeedDatabase();
            await db.BulkDeleteAsync(Cards);
            await db.Subscriptions.BatchUpdateAsync(u => new DBSubscription {LastTimestamp = DateTimeOffset.MinValue});
            Cards.Clear();
        }

        public async Task MarkCardsAsViewed()
        {
            foreach (var card in Cards.Where(w => !w.IsViewed))
            {
                card.IsViewed = true;
            }

            await using var db = new FeedDatabase();
            await db.Cards.BatchUpdateAsync(new DBCard {IsViewed = true});
        }

        public void ShowBookmarks()
        {
            foreach (var card in Cards)
            {
                card.IsHidden = !card.IsBookmark;
            }
        }

        public void ShowAll()
        {
            foreach (var card in Cards.Where(w => w.IsHidden))
            {
                card.IsHidden = false;
            }
        }

        public void OpenSourceFlyout()
        {
            EventAggregator.Publish(new OpenFlyoutEvent(EFlyout.Source));
        }

        public async Task LoadSubscriptionsAsync()
        {
            await using var db = new FeedDatabase();

            AddSubscription(await db.Subscriptions.AsNoTracking().ToListAsync());
        }

        public async Task LoadCardsAsync()
        {
            await using var db = new FeedDatabase();

            var cards = await db.Cards.Include(i => i.Subscription).AsNoTracking().ToListAsync();
            var filtered = cards.Where(w => w.Timestamp.AddDays(Settings.Default.CardsRestrictOldInDays) >= DateTimeOffset.Now || w.IsBookmark);

            Cards.AddRange(filtered.ToList());
        }

        public async void Handle(NewSubscriptionEvent message)
        {
            await using var db = new FeedDatabase();
            await db.SubscribeAndSaveAsync(message.Subscription);

            AddSubscription(message.Subscription);
        }

        public void OpenCardInBrowser(DBCard card)
        {
            if(!string.IsNullOrWhiteSpace(card.OriginalUrl)) Process.Start("cmd", $"/C start {card.OriginalUrl}");
        }

        public async Task BookmarkCard(DBCard card)
        {
            await using var db = new FeedDatabase();
            db.Attach(card);
            card.IsBookmark = !card.IsBookmark;
            await db.SaveChangesAsync();
        }

        private void AddSubscription(IEnumerable<DBSubscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                AddSubscription(subscription);
            }
        }
        
        private void AddSubscription(DBSubscription subscription)
        {
            var existAppSub = Subscriptions.FirstOrDefault(f => f.Source == subscription.Source);

            if (existAppSub == null)
            {
                existAppSub = new AppSourceList(subscription.Source);
                Subscriptions.Add(existAppSub);
                existAppSub.Sources.Add(subscription);
            }
            else
            {
                var existSub = existAppSub.Sources.FirstOrDefault(f => f.SourceId == subscription.SourceId);
                if (existSub == null)
                {
                    existAppSub.Sources.Add(subscription);
                }
            }
        }

        private void ShowCardsBySubscription(DBSubscription subscription)
        {
            foreach (var card in Cards)
            {
                card.IsHidden = card.SubscriptionId != subscription.Id;
            }
        }

        private void ShowCardsBySource(AppSourceList sourceList)
        {
            foreach (var card in Cards)
            {
                card.IsHidden = card.Subscription.Source != sourceList.Source;
            }
        }

        protected override async void OnClose()
        {
            await using var db = new FeedDatabase();
            await db.BulkUpdateAsync(Cards, new BulkConfig
            {
                PropertiesToInclude = new List<string> { nameof(DBCard.IsViewed), nameof(DBCard.IsHidden) }
            });
        }
    }
}