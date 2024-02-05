using BankAccount.Command;
using BankAccount.DTOs;
using BankAccount.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BankAccount.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IDataTransactionService _dataTransactionService;

        private List<TransactionDTO> _transactions;

        public HistoryViewModel(IDataTransactionService dataTransactionService)
        {
            _dataTransactionService = dataTransactionService;

            _ = LoadTransactions();

            NavigateBackToMainCommand = new RelayCommand(OnNavigateBackToMainCommandExecuted, CanNavigateBackToMainCommandExecute);
        }

        public List<TransactionDTO> Transactions
        {
            get { return _transactions; }
            set { Set(ref _transactions, value); }
        }

        private async Task LoadTransactions()
        {
            var transactions = await _dataTransactionService.GetTransactionsAsync();

            Transactions = transactions.Select(t => new TransactionDTO
            {
                OperationId = t.OperationId,
                DateTime = t.DateTime,
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type
            }).ToList();
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
