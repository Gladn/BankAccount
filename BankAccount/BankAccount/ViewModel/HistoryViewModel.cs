using BankAccount.Command;
using BankAccount.DTOs;
using BankAccount.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BankAccount.ViewModel
{
    public class HistoryViewModel : ViewModelBase
    {
        private readonly IDataTransactionService _dataTransactionService;

        public HistoryViewModel(IDataTransactionService dataTransactionService)
        {
            _dataTransactionService = dataTransactionService;

            _ = LoadTransactions();

            NavigateBackToMainCommand = new RelayCommand(OnNavigateBackToMainCommandExecuted, CanNavigateBackToMainCommandExecute);
        }

        private List<TransactionDTO> _transactions;
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
                DateTime = t.DateTime.ToString("dd-MM-yy"),
                Amount = t.Amount,
                Currency = t.Currency,
                Type = t.Type
            }).ToList();

        }


        public ICommand NavigateBackToMainCommand { get; }
        private bool CanNavigateBackToMainCommandExecute(object parameter) => true;
        private async Task OnNavigateBackToMainCommandExecuted(object parameter)
        {
            await NavigateBackAsync();
        }
    }
}
