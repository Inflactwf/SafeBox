using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace SafeBox.Behaviors
{
    public class BorderlessWindowDragBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseDown -= AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                AssociatedObject.DragMove();
        }
    }
}
