using Microsoft.Xaml.Behaviors;
using SafeBox.Extensions;
using System.Windows;
using System.Windows.Input;

namespace SafeBox.TriggerActions
{
    public class SearchBoxFocusAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(UIElement), typeof(SearchBoxFocusAction), new UIPropertyMetadata(null));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.RegisterAttached("SearchTextProperty", typeof(string), typeof(SearchBoxFocusAction), new UIPropertyMetadata(null));

        public UIElement Target
        {
            get => (UIElement)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public string SearchTextProperty
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        protected override void Invoke(object parameter)
        {
            if (Target == null)
                return;

            if (Keyboard.IsKeyDown(Key.Escape) && Target.IsFocused)
            {
                if (!SearchTextProperty.IsNullOrWhiteSpace())
                    SearchTextProperty = string.Empty;

                Target.MoveFocus(new(FocusNavigationDirection.Previous));

                return;
            }

            if (!Target.IsFocused)
                Target.Focus();
        }
    }
}
