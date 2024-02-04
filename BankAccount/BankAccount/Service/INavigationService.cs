using System;
using Windows.UI.Xaml.Controls;

namespace BankAccount.Service
{
    public interface INavigationService
    {
        void NavigateToPage(Type pageType);
    }

    public class NavigationService : INavigationService
    {
        private readonly Frame _frame;

        public NavigationService(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateToPage(Type pageType)
        {
            _frame.Navigate(pageType);
        }
    }
}
