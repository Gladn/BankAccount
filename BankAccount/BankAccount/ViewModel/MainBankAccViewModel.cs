using BankAccount.Command;
using BankAccount.Service;
using BankAccount.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;

namespace BankAccount.ViewModel
{
    public class MainBankAccViewModel : ViewModelBase
    {

        public MainBankAccViewModel() 
        {
            NavigateToAddTrancCommand = new RelayCommand(OnNavigateToAddTrancCommandExecuted, CanNavigateToAddTrancCommandExecute);

            NavigateToHistoryCommand = new RelayCommand(OnNavigateToHistoryCommandExecuted, CanNavigateToHistoryCommandExecute);
        }


        public ICommand NavigateToAddTrancCommand { get; }
        private bool CanNavigateToAddTrancCommandExecute(object parameter) => true;
        private void OnNavigateToAddTrancCommandExecuted(object parameter)
        {
            NavigateToAddTranc();
        }

        public void NavigateToAddTranc()
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(AddTransactionPage));
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка перехода. Код ошибки: {ex.Message}", "Уведомление");
                dialog.ShowAsync();
            }
        }

        public ICommand NavigateToHistoryCommand { get; }
        private bool CanNavigateToHistoryCommandExecute(object parameter) => true;
        private void OnNavigateToHistoryCommandExecuted(object parameter)
        {
            NavigateToHistory();
        }

        public void NavigateToHistory()
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(HistoryPage));
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка перехода. Код ошибки: {ex.Message}", "Уведомление");
                dialog.ShowAsync();
            }
        }





    }
}
