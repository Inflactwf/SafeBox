using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SafeBox.Behaviors
{
    public class ListBoxAddingScrollBehavior : Behavior<ListBox>
    {
        public readonly static DependencyProperty ScrollProperty =
            DependencyProperty.Register("ScrollProperty", typeof(bool), typeof(ListBoxAddingScrollBehavior), new PropertyMetadata(false));

        private INotifyCollectionChanged _currentCollection;

        public bool IsScrollingToEndEnabled
        {
            get => (bool)GetValue(ScrollProperty);
            set => SetValue(ScrollProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            SubscribeToCollectionChanged();
            var itemsSourceProperty = TypeDescriptor.GetProperties(AssociatedObject)["ItemsSource"];
            itemsSourceProperty.AddValueChanged(AssociatedObject, OnItemsSourceChanged);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            UnsubscribeFromCollectionChanged();
            var itemsSourceProperty = TypeDescriptor.GetProperties(AssociatedObject)["ItemsSource"];
            itemsSourceProperty.RemoveValueChanged(AssociatedObject, OnItemsSourceChanged);
        }

        private void OnItemsSourceChanged(object sender, EventArgs e)
        {
            SubscribeToCollectionChanged();
        }

        private void SubscribeToCollectionChanged()
        {
            UnsubscribeFromCollectionChanged();

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                _currentCollection = collection;
                _currentCollection.CollectionChanged += OnCollectionChanged;
            }
        }

        private void UnsubscribeFromCollectionChanged()
        {
            if (_currentCollection != null)
            {
                _currentCollection.CollectionChanged -= OnCollectionChanged;
                _currentCollection = null;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && AssociatedObject.Items.Count > 0 && IsScrollingToEndEnabled)
                AssociatedObject.ScrollIntoView(e.NewItems[0]);
        }
    }
}
