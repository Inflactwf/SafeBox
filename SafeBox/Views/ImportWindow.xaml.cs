using SafeBox.ViewModels;
using System.Windows;

namespace SafeBox.Views;

/// <summary>
/// Interaction logic for ImportWindow.xaml
/// </summary>
public partial class ImportWindow : Window
{
    public ImportWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ImportViewModel vm)
            return;

        vm.RequestClose += ViewModel_RequestClose;
        Closed += (s, args) => vm.RequestClose -= ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose() => Close();
}