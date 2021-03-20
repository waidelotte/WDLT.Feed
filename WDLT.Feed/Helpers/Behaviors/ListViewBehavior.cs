using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace WDLT.Feed.Helpers.Behaviors
{
    public static class ListViewBehavior
    {
        public static readonly DependencyProperty ScrollSelectedIntoViewProperty =
            DependencyProperty.RegisterAttached("ScrollSelectedIntoView", typeof(bool), typeof(ListViewBehavior),
                new UIPropertyMetadata(false, OnScrollSelectedIntoViewChanged));

        public static bool GetScrollSelectedIntoView(ListView listView)
        {
            return (bool)listView.GetValue(ScrollSelectedIntoViewProperty);
        }

        public static void SetScrollSelectedIntoView(ListView listView, bool value)
        {
            listView.SetValue(ScrollSelectedIntoViewProperty, value);
        }

        private static void OnScrollSelectedIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Selector selector)) return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                selector.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(ListViewSelectionChangedHandler));
            }
            else
            {
                selector.RemoveHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(ListViewSelectionChangedHandler));
            }
        }

        private static void ListViewSelectionChangedHandler(object sender, RoutedEventArgs e)
        {
            var listView = sender as ListView;
            if (listView?.SelectedItem != null)
            {
                listView.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        listView.UpdateLayout();
                        if (listView.SelectedItem != null)
                            listView.ScrollIntoView(listView.SelectedItem);
                    }));
            }
        }
    }
}