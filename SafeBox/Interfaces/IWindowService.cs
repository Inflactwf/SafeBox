using System.Windows;

namespace SafeBox.Interfaces
{
    internal interface IWindowService
    {
        void ShowWindow<T>(object dataContext) where T : Window, new();
    }
}
