using BankAccount.Command;
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
        private readonly IDataCurrencyService _dataCurrencyService;
        private readonly IDataTransactionService _dataTransactionService;
        private readonly IDataBalanceService _dataBalanceService;
        private readonly ICurrencyConverterService _currencyConverterService;

        public AddTransactionViewModel(IDataCurrencyService dataCurrencyService, IDataTransactionService dataTransactionService,
                                            IDataBalanceService dataBalanceService, ICurrencyConverterService currencyConverterService)
        {
            _dataCurrencyService = dataCurrencyService;

            _dataTransactionService = dataTransactionService;

            _dataBalanceService = dataBalanceService;

            _currencyConverterService = currencyConverterService;


            _ = GetAllBoxesData();

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


        
        private ObservableCollection<string> _currencycharCodes;
        public ObservableCollection<string> CurrencyCharCodes
        {
            get { return _currencycharCodes; }
            set { Set(ref _currencycharCodes, value); }
        }


        private string _selectedCurrencyCharCode;
        public string SelectedCurrencyCharCode
        {
            get { return _selectedCurrencyCharCode; }
            set { Set(ref _selectedCurrencyCharCode, value); }
        }


        
        private string _transactionAmount;
        public string TransactionAmount
        {
            get { return _transactionAmount; }
            set { Set(ref _transactionAmount, value); }
        }


        private async Task GetAllBoxesData()
        {
            TransactionAmount = "";

            TransactionTypes = new ObservableCollection<string>
            {
                "Зачисление",
                "Снятие"
            };

            SelectedTransactionType = TransactionTypes.FirstOrDefault();

            List<string> currencyNamesFromDatabase = await _dataCurrencyService.GetCurrencyCharCodeAsync();

            CurrencyCharCodes = new ObservableCollection<string>(currencyNamesFromDatabase);

            SelectedCurrencyCharCode = CurrencyCharCodes.FirstOrDefault();
        }



        public ICommand AddTransactionCommand { get; }
        private bool CanAddTransactionCommandExecute(object parameter) => true;

        private async Task OnAddTransactionCommandExecuted(object parameter)
        {
            try
            {
                decimal amount = decimal.Parse(TransactionAmount);
                await _dataTransactionService.AddTransactionAsync(amount, SelectedCurrencyCharCode, SelectedTransactionType);
                

                decimal amountInRubles = await _currencyConverterService.ConvertToRublesAsync(amount, SelectedCurrencyCharCode);               
                await _dataBalanceService.UpdateBalanceAsync(amountInRubles, SelectedTransactionType);


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
