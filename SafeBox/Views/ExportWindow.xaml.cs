using SafeBox.ViewModels;
using System.Windows;

namespace SafeBox.Views;

/// <summary>
/// Interaction logic for ExportWindow.xaml
/// </summary>
public partial class ExportWindow : Window
{
    public ExportWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not ExportViewModel vm)
            return;

        vm.RequestClose += ViewModel_RequestClose;
        Closed += (s, args) => vm.RequestClose -= ViewModel_RequestClose;
    }

    private void ViewModel_RequestClose() => Close();
}