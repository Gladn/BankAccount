using BankAccount.Command;
using BankAccount.Model;
using BankAccount.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BankAccount.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IDataBaseService _dataBaseService;

        private List<Transaction> _transactions;

        public HistoryViewModel(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            LoadTransactions();

            NavigateBackToMainCommand = new RelayCommand(OnNavigateBackToMainCommandExecuted, CanNavigateBackToMainCommandExecute);
        }

        public List<Transaction> Transactions
        {
            get { return _transactions; }
            set
            {
                _transactions = value;
                OnPropertyChanged(nameof(Transactions));
            }
        }

        private async void LoadTransactions()
        {
            Transactions = await _dataBaseService.GetTransactionsAsync();
        }



        public ICommand NavigateBackToMainCommand { get; }
        private bool CanNavigateBackToMainCommandExecute(object parameter) => true;
        private async Task OnNavigateBackToMainCommandExecuted(object parameter)
        {
            await NavigateBackToMain();
        }

        public async Task NavigateBackToMain()
        {
            try
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка перехода назад. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }
    }
}
