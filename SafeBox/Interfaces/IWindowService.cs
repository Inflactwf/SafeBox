using System.Windows;

namespace SafeBox.Interfaces;

public interface IWindowService
{
    void ShowWindow<T>(object dataContext) where T : Window, new();
}