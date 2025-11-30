using Microsoft.Xaml.Behaviors;
using SafeBox.Extensions;
using SafeBox.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SafeBox.Behaviors;

public class SearchFocusBehavior : Behavior<UIElement>
{
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(UIElement), typeof(SearchFocusBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty RevertFocusTargetProperty =
        DependencyProperty.Register(nameof(RevertFocusTarget), typeof(UIElement), typeof(SearchFocusBehavior), new PropertyMetadata(null));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(SearchTextProperty), typeof(string), typeof(SearchFocusBehavior), new PropertyMetadata(null));

    public UIElement Target
    {
        get => (UIElement)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public UIElement RevertFocusTarget
    {
        get => (UIElement)GetValue(RevertFocusTargetProperty);
        set => SetValue(RevertFocusTargetProperty, value);
    }

    public string SearchTextProperty
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }


    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Target == null)
            return;

        if (e.Key == Key.Escape && Target.IsFocused)
        {
            if (!SearchTextProperty.IsNull())
                SearchTextProperty = string.Empty;

            if (RevertFocusTarget is ListBox { Items.Count: > 0, ItemContainerGenerator: { } container })
            {
                var firstItem = container.Items.FirstOrDefault();
                ((ListBoxItem)container.ContainerFromItem(firstItem)).Focus();
                return;
            }

            RevertFocusTarget.Focus();
            return;
        }

        var keyChar = Utilities.GetCharFromKey(e.Key);

        if (!Target.IsFocused && (char.IsLetterOrDigit(keyChar) || char.IsPunctuation(keyChar)))
            Target.Focus();
    }
}