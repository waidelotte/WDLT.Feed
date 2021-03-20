using System.Windows;
using System.Windows.Controls;

namespace WDLT.Feed.Helpers.Controls
{
    public class TreeViewEx : TreeView
    {
        public static readonly DependencyProperty SelectedItemExProperty = DependencyProperty.Register("SelectedItemEx", typeof(object), typeof(TreeViewEx), new FrameworkPropertyMetadata(default(object))
        {
            BindsTwoWayByDefault = true
        });

        public object SelectedItemEx
        {
            get => GetValue(SelectedItemExProperty);
            set => SetValue(SelectedItemExProperty, value);
        }

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedItemEx = e.NewValue;
        }
    }
}