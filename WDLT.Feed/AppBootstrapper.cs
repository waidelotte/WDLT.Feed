using System;
using System.Linq;
using System.Windows;
using CefSharp;
using CefSharp.Wpf;
using EFCore.BulkExtensions;
using FluentValidation;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Stylet;
using StyletIoC;
using WDLT.Feed.Database;
using WDLT.Feed.Helpers;
using WDLT.Feed.Helpers.Validation;
using WDLT.Feed.Interfaces;
using WDLT.Feed.Properties;
using WDLT.Feed.Services;
using WDLT.Feed.ViewModels;
using WDLT.Feed.ViewModels.Flyouts;

namespace WDLT.Feed
{
    public class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
		protected override void ConfigureIoC(IStyletIoCBuilder builder)
        {
            builder.Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));
            builder.Bind(typeof(IValidator<>)).ToAllImplementations();

            builder.Bind<FeedDatabase>().ToSelf();
            builder.Bind<IDialogFactory>().ToAbstractFactory();

            builder.Bind<DialogManager>().ToSelf().InSingletonScope();

            builder.Bind<FlyoutBaseViewModel>().ToAllImplementations();
            builder.Bind<BaseTabViewModel>().ToAllImplementations();

            builder.Bind<ISnackbarMessageQueue>().ToInstance(new SnackbarMessageQueue(TimeSpan.FromSeconds(3)));
            builder.Bind<BaseSubscriptionService>().ToAllImplementations().InSingletonScope();
        }

        protected override void Configure()
        {
            var settings = new CefSettings
            {
                UserAgent = "Mozilla/5.0 (Linux; Android 7.1.1; SM-T555 Build/NMF26X; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/83.0.4103.96 Safari/537.36"
            };

            Cef.Initialize(settings);

            base.Configure();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await using var db = new FeedDatabase();

            var toDelelte = (await db.Cards
                    .Where(w => !w.IsBookmark)
                    .AsNoTracking()
                    .ToListAsync())
                .Where(w => w.Timestamp.AddDays(Settings.Default.CardsRestrictOldInDays) < DateTimeOffset.Now)
                .ToList();

            await db.BulkDeleteAsync(toDelelte);

            Settings.Default.Save();

            base.OnExit(e);
        }
    }
}