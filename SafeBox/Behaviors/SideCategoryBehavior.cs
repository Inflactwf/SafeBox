using Microsoft.Xaml.Behaviors;
using SafeBox.Enums;
using SafeBox.Models;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace SafeBox.Behaviors;

public class SideCategoryBehavior : Behavior<RadioButton>
{
    private readonly DoubleAnimation _fadeInAnimation = new(1, TimeSpan.FromMilliseconds(150));
    private readonly DoubleAnimation _fadeOutAnimation = new(0, TimeSpan.FromMilliseconds(150));
    private TextBlock _displayTextBox;

    public static readonly DependencyProperty IsSideShownProperty =
        DependencyProperty.Register(
            nameof(IsSideShown),
            typeof(bool),
            typeof(SideCategoryBehavior),
            new PropertyMetadata(false, OnIsSideShownChanged));

    public static readonly DependencyProperty CategoryDisplayNameProperty =
        DependencyProperty.Register(
            nameof(CategoryDisplayName),
            typeof(string),
            typeof(SideCategoryBehavior));

    public static readonly DependencyProperty CountProperty =
        DependencyProperty.Register(
            nameof(Count),
            typeof(int),
            typeof(SideCategoryBehavior));

    public static readonly DependencyProperty CategoryProperty =
        DependencyProperty.Register(
            nameof(Category),
            typeof(Category),
            typeof(SideCategoryBehavior));

    public static readonly DependencyProperty SourceCollectionViewProperty =
        DependencyProperty.Register(
            nameof(SourceCollectionView),
            typeof(ICollectionView),
            typeof(SideCategoryBehavior),
            new PropertyMetadata(null, OnSourceCollectionChanged));

    public bool IsSideShown
    {
        get => (bool)GetValue(IsSideShownProperty);
        set => SetValue(IsSideShownProperty, value);
    }

    public string CategoryDisplayName
    {
        get => (string)GetValue(CategoryDisplayNameProperty);
        set => SetValue(CategoryDisplayNameProperty, value);
    }

    public int Count
    {
        get => (int)GetValue(CountProperty);
        private set => SetValue(CountProperty, value);
    }

    public Category Category
    {
        get => (Category)GetValue(CategoryProperty);
        set => SetValue(CategoryProperty, value);
    }

    public ICollectionView SourceCollectionView
    {
        get => (ICollectionView)GetValue(SourceCollectionViewProperty);
        set => SetValue(SourceCollectionViewProperty, value);
    }

    protected override void OnAttached()
    {
        if (AssociatedObject is { Content: TextBlock tb })
            _displayTextBox = tb;

        UpdateText();
        base.OnAttached();
    }

    private static void OnSourceCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SideCategoryBehavior instance)
            return;

        if (e.OldValue is ICollectionView oldCollectionView)
            oldCollectionView.CollectionChanged -= instance.OnSourceCollectionElementsChanged;

        if (e.NewValue is not ICollectionView newCollectionView)
        {
            instance.Count = 0;
            instance.UpdateText();
            return;
        }

        instance.Count = instance.GetStorageMembersCount(newCollectionView);
        instance.UpdateText();
        newCollectionView.CollectionChanged += instance.OnSourceCollectionElementsChanged;
    }

    private void OnSourceCollectionElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        Count = GetStorageMembersCount(SourceCollectionView);
        UpdateText();
    }

    private static void OnIsSideShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideCategoryBehavior { AssociatedObject.Content: TextBlock tb } behavior)
            behavior.RunTextFadeAnimation(tb);
    }

    private int GetStorageMembersCount(ICollectionView collectionView)
    {
        var collection = collectionView
            .SourceCollection
            .Cast<StorageMember>();

        return Category == Category.All
            ? collection.Count()
            : collection.Count(storageMember => storageMember.Category == Category);
    }

    private void UpdateText()
    {
        if (_displayTextBox == null)
            return;

        _displayTextBox.Text = IsSideShown
            ? $"{CategoryDisplayName} ({Count})"
            : Count.ToString();
    }

    private void RunTextFadeAnimation(TextBlock textBlock)
    {
        EventHandler handler = null;
        handler = (s, e) =>
        {
            _fadeOutAnimation.Completed -= handler;
            UpdateText();
            textBlock.BeginAnimation(UIElement.OpacityProperty, _fadeInAnimation);
        };

        _fadeOutAnimation.Completed += handler;
        textBlock.BeginAnimation(UIElement.OpacityProperty, _fadeOutAnimation);
    }
}