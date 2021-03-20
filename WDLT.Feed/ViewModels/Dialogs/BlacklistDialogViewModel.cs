using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stylet;
using WDLT.Feed.Database;
using WDLT.Feed.Database.Entities;

namespace WDLT.Feed.ViewModels.Dialogs
{
    public class BlacklistDialogViewModel: BaseDialogViewModel
    {
        private string _input;
        public string Input
        {
            get => _input;
            set
            {
                SetAndNotify(ref _input, value);
                CanOk = ValidateProperty(() => Input);
            }
        }

        public bool CanOk { get; set; }

        public BindableCollection<DBBlacklist> Blacklist { get; }
        public List<DBBlacklist> New { get; set; }

        private DBSubscription _subscription;

        public BlacklistDialogViewModel(IModelValidator<BlacklistDialogViewModel> validator) : base(validator)
        {
            Blacklist = new BindableCollection<DBBlacklist>();
            New = new List<DBBlacklist>();
        }

        public async Task Init(DBSubscription subscription)
        {
            _subscription = subscription;

            await using var db = new FeedDatabase();

            Blacklist.AddRange(await db.Blacklist.Where(w => w.SubscriptionId == subscription.Id).AsNoTracking().ToListAsync());
        }

        public async Task RemoveWord(DBBlacklist word)
        {
            Blacklist.Remove(word);
            New.Remove(word);

            await using var db = new FeedDatabase();
            db.Blacklist.Attach(word);
            db.Blacklist.Remove(word);
            await db.SaveChangesAsync();
        }

        public async Task Ok()
        {
            var formated = Input.Trim().ToLower();

            if (Blacklist.Any(f => f.Word == formated))
            {
                RecordPropertyError(() => Input, new []{ "The word is already added" });
                return;
            }

            await using var db = new FeedDatabase();

            var word = new DBBlacklist
            {
                SubscriptionId = _subscription.Id,
                Word = formated
            };

            await db.Blacklist.AddAsync(word);
            await db.SaveChangesAsync();

            New.Add(word);
            Blacklist.Insert(0, word);
            Input = null;
        }
    }
}