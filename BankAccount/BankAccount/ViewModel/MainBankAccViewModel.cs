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
using BankAccount.Model;

namespace BankAccount.ViewModel
{
    public class MainBankAccViewModel : ViewModelBase
    {
        private readonly IDataBaseService _dataBaseService;

        private List<Transaction> _transactions;

        public MainBankAccViewModel(IDataBaseService dataBaseService) 
        {
            _dataBaseService = dataBaseService;

            _ = DatabaseAsync();


            NavigateToAddTrancCommand = new RelayCommand(OnNavigateToAddTrancCommandExecuted, CanNavigateToAddTrancCommandExecute);

            NavigateToHistoryCommand = new RelayCommand(OnNavigateToHistoryCommandExecuted, CanNavigateToHistoryCommandExecute);

        }


        public async Task DatabaseAsync()
        {
            try
            {
                await _dataBaseService.InitializeDatabaseAsync();

                await _dataBaseService.UpdateCurrencyTableAsync();

            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка создания БД. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }


        


        public ICommand NavigateToAddTrancCommand { get; }
        private bool CanNavigateToAddTrancCommandExecute(object parameter) => true;
        private async Task OnNavigateToAddTrancCommandExecuted(object parameter)
        {
            await NavigateToAddTranc();
        }

        
        public async Task NavigateToAddTranc()
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(AddTransactionPage));
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка перехода. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }



        public ICommand NavigateToHistoryCommand { get; }
        private bool CanNavigateToHistoryCommandExecute(object parameter) => true;
        private async Task OnNavigateToHistoryCommandExecuted(object parameter)
        {
            await NavigateToHistory();
        }

        public async Task NavigateToHistory()
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                rootFrame.Navigate(typeof(HistoryPage));
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка перехода. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }
    }
}
