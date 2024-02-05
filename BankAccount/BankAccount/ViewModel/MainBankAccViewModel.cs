using BankAccount.Command;
using BankAccount.Service;
using BankAccount.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;


namespace BankAccount.ViewModel
{
    public class MainBankAccViewModel : ViewModelBase
    {
        private readonly IDataBaseService _dataBaseService;
        private readonly IDataCurrencyService _dataCurrencyService;

        public MainBankAccViewModel(IDataBaseService dataBaseService, IDataCurrencyService dataCurrencyService) 
        {
            _dataBaseService = dataBaseService;

            _dataCurrencyService = dataCurrencyService;

            InitializeAsync();


            NavigateToAddTrancCommand = new RelayCommand(OnNavigateToAddTrancCommandExecuted, CanNavigateToAddTrancCommandExecute);

            NavigateToHistoryCommand = new RelayCommand(OnNavigateToHistoryCommandExecuted, CanNavigateToHistoryCommandExecute);
        }

        public async void InitializeAsync()
        {
            await CreateDatabaseAsync();
            await UpadateCurrencyAsync();
            await GetComboBoxCurrencyCharCode();

        }

        public async Task CreateDatabaseAsync()
        {
            try
            {
                await _dataBaseService.InitializeDatabaseAsync();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка создания БД. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }

        public async Task UpadateCurrencyAsync()
        {
            try
            {
                await _dataBaseService.UpdateCurrencyTableFromApiAsync();
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"Ошибка в Api. Код ошибки: {ex.Message}", "Уведомление");
                await dialog.ShowAsync();
            }
        }


        private ObservableCollection<string> _currencyCharCodes;
        public ObservableCollection<string> CurrencyCharCodes
        {
            get { return _currencyCharCodes; }
            set { Set(ref _currencyCharCodes, value); }
        }

        private string _selectedCurrencyCharCode;
        public string SelectedCurrencyCharCode
        {
            get { return _selectedCurrencyCharCode; }
            set { Set(ref _selectedCurrencyCharCode, value); }
        }



        private async Task GetComboBoxCurrencyCharCode()
        {
            List<string> currencyNamesFromDatabase = await _dataCurrencyService.GetCurrencyCharCodeAsync();

            CurrencyCharCodes = new ObservableCollection<string>(currencyNamesFromDatabase);

            SelectedCurrencyCharCode = CurrencyCharCodes.FirstOrDefault();
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
