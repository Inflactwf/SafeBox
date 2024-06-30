using SafeBox.Interfaces;
using System.Windows;

namespace SafeBox.Services
{
    internal class WindowService : IWindowService
    {
        public void ShowWindow<T>(object dataContext) where T : Window, new()
        {
            var window = new T
            {
                DataContext = dataContext
            };

            window.ShowDialog();
        }
    }
}
