using Microsoft.Xaml.Behaviors;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace SafeBox.Behaviors;

public class PasswordBoxSecureBehavior : Behavior<PasswordBox>
{
    public static readonly DependencyProperty SecurePasswordProperty =
        DependencyProperty.Register(nameof(SecurePassword), typeof(SecureString), typeof(PasswordBoxSecureBehavior),
            new FrameworkPropertyMetadata(null));

    public SecureString SecurePassword
    {
        get => (SecureString)GetValue(SecurePasswordProperty);
        set => SetValue(SecurePasswordProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.PasswordChanged += OnPasswordChanged;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.PasswordChanged -= OnPasswordChanged;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e) =>
        SecurePassword = AssociatedObject.SecurePassword;
}