using BankAccount.Command;
using BankAccount.Model;
using BankAccount.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BankAccount.ViewModel
{
    public class AddTransactionViewModel : ViewModelBase
    {
        private readonly IDataBaseService _dataBaseService;
        public AddTransactionViewModel(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;

            _ = GetAllComboBoxNames();

            AddTransactionCommand = new RelayCommand(OnAddTransactionCommandExecuted, CanAddTransactionCommandExecute);

            NavigateBackToMainCommand = new RelayCommand(OnNavigateBackToMainCommandExecuted, CanNavigateBackToMainCommandExecute);
        }


        
        public ObservableCollection<string> TransactionTypes { get; private set; }
        
        private string _selectedTransactionType;
        public string SelectedTransactionType
        {
            get { return _selectedTransactionType; }
            set { Set(ref _selectedTransactionType, value); }
        }


        
        private ObservableCollection<string> _currencyNames;
        public ObservableCollection<string> CurrencyNames
        {
            get { return _currencyNames; }
            set { Set(ref _currencyNames, value); }
        }

        private string _selectedCurrencyName;
        public string SelectedCurrencyName
        {
            get { return _selectedCurrencyName; }
            set { Set(ref _selectedCurrencyName, value); }
        }


        
        private string _transactionAmount;
        public string TransactionAmount
        {
            get { return _transactionAmount; }
            set { Set(ref _transactionAmount, value); }
        }


        private async Task GetAllComboBoxNames()
        {
            TransactionAmount = "";

            TransactionTypes = new ObservableCollection<string>
            {
                "Зачисление",
                "Снятие"
            };

            SelectedTransactionType = TransactionTypes.FirstOrDefault();

            List<string> currencyNamesFromDatabase = await _dataBaseService.GetCurrencyNamesAsync();

            CurrencyNames = new ObservableCollection<string>(currencyNamesFromDatabase);

            SelectedCurrencyName = CurrencyNames.FirstOrDefault();
        }



        public ICommand AddTransactionCommand { get; }
        private bool CanAddTransactionCommandExecute(object parameter) => true;

        private async Task OnAddTransactionCommandExecuted(object parameter)
        {
            try
            {
                decimal amount = decimal.Parse(TransactionAmount);
                await _dataBaseService.AddTransactionAsync(amount, SelectedCurrencyName, SelectedTransactionType);
                await NavigateBackToMain();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка ввода суммы. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
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
