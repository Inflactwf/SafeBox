using Microsoft.Xaml.Behaviors;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace SafeBox.Behaviors
{
    public class WindowClosingBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty StoryboardProperty =
            DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(WindowClosingBehavior), new PropertyMetadata(default(Storyboard)));

        public Storyboard Storyboard
        {
            get => (Storyboard)GetValue(StoryboardProperty);
            set => SetValue(StoryboardProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            if (Storyboard == null)
                return;

            e.Cancel = true;
            AssociatedObject.Closing -= OnWindowClosing;

            Storyboard.Completed += (o, a) => AssociatedObject.Close();
            Storyboard.Begin(AssociatedObject);
        }
    }
}
