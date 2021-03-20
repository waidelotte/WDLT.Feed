using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using Stylet;
using WDLT.Feed.Database.Entities;
using WDLT.Feed.Interfaces;

namespace WDLT.Feed.Helpers
{
    public class DialogManager
    {
        private readonly IViewManager _viewManager;
        private readonly IDialogFactory _dialogFactory;

        public DialogManager(IViewManager viewManager, IDialogFactory dialogFactory)
        {
            _viewManager = viewManager;
            _dialogFactory = dialogFactory;
        }

        public async Task<string> ShowSimpleDialogAsync(string header, string defaultText = null)
        {
            var viewModel = _dialogFactory.CreateSimpleDialogViewModel();
            viewModel.Header = header;
            viewModel.Answer = defaultText;

            var view = _viewManager.CreateAndBindViewForModelIfNecessary(viewModel);

            return (string) await DialogHost.Show(view);
        }

        public async Task<bool> ShowAcceptDialogAsync(string text)
        {
            var viewModel = _dialogFactory.CreateAcceptDialogViewModel();
            viewModel.Text = text;

            var view = _viewManager.CreateAndBindViewForModelIfNecessary(viewModel);

            return (bool)await DialogHost.Show(view);
        }

        public async Task<List<DBBlacklist>> ShowBlacklistDialogAsync(DBSubscription subscription)
        {
            var viewModel = _dialogFactory.CreateBlacklistViewModel();
            await viewModel.Init(subscription);

            var view = _viewManager.CreateAndBindViewForModelIfNecessary(viewModel);

            await DialogHost.Show(view);
            return viewModel.New;
        }

        public string PickFolderDialog()
        {
            using var dialog = new FolderBrowserDialog();

            return dialog.ShowDialog() == DialogResult.OK ? dialog.SelectedPath : null;
        }

        public string PickFileDialog()
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Json files (*.json)|*.json|Text files (*.txt)|*.txt"
            };

            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
    }
}