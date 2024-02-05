using BankAccount.Command;
using BankAccount.Service;
using BankAccount.View;
using BankAccount.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Popups;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;


namespace BankAccount.ViewModel
{
    public class MainBankAccViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private readonly IDataBaseService _dataBaseService;
        private readonly IDataCurrencyService _dataCurrencyService;
        private readonly IDataBalanceService _dataBalanceService;
        private readonly ICurrencyConverterService _currencyConverterService;

        public MainBankAccViewModel(IDataBaseService dataBaseService, IDataCurrencyService dataCurrencyService, 
                                        IDataBalanceService dataBalanceService, ICurrencyConverterService currencyConverterService) 
        {
            _dataBaseService = dataBaseService;

            _dataCurrencyService = dataCurrencyService;

            _dataBalanceService = dataBalanceService;

            _currencyConverterService = currencyConverterService;

            InitializeWindowAllData();

            NavigateToAddTrancCommand = new RelayCommand(OnNavigateToAddTrancCommandExecuted, CanNavigateToAddTrancCommandExecute);

            NavigateToHistoryCommand = new RelayCommand(OnNavigateToHistoryCommandExecuted, CanNavigateToHistoryCommandExecute);
        }

        public async void InitializeWindowAllData()
        {
            await CreateDatabaseAsync();
            await UpadateCurrencyAsync();
            await GetComboBoxCurrencyCharCode();
            await GetTextBoxBalance();
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
            set
            {
                if (_selectedCurrencyCharCode != value)
                {
                    _selectedCurrencyCharCode = value;
                    OnPropertyChanged(nameof(SelectedCurrencyCharCode));
                    _ = UpadateCurrencyAsync();
                    _ = UpdateDisplayedBalanceAsync();
                }
            }
        }


        private async Task GetComboBoxCurrencyCharCode()
        {
            List<string> currencyNamesFromDatabase = await _dataCurrencyService.GetCurrencyCharCodeAsync();

            if (!currencyNamesFromDatabase.Contains("RUB"))
            {
                currencyNamesFromDatabase.Insert(0, "RUB");
            }

            CurrencyCharCodes = new ObservableCollection<string>(currencyNamesFromDatabase);

            SelectedCurrencyCharCode = CurrencyCharCodes.FirstOrDefault();
        }


        private string _balanceText;
        public string BalanceText
        {
            get { return _balanceText; }
            set { Set(ref _balanceText, value); }
        }

               
        private async Task GetTextBoxBalance()
        {
            BalanceDTO currentBalance = await _dataBalanceService.GetCurrentBalanceDTOAsync();

            BalanceText = currentBalance.Amount.ToString("0.00");
        }


        private async Task UpdateDisplayedBalanceAsync()
        {
            BalanceDTO currentBalance = await _dataBalanceService.GetCurrentBalanceDTOAsync();

            if (SelectedCurrencyCharCode == "RUB")
            {
                BalanceText = currentBalance.Amount.ToString("0.00");
            }
            else 
            {
                decimal currentBalanceInRubles = await _currencyConverterService.ConvertToOtherAsync(currentBalance.Amount, SelectedCurrencyCharCode);

                BalanceText = currentBalanceInRubles.ToString("0.00");
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


        #region Validation TextBlock

        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();
        private bool _hasErrors;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingField, value))
                return false;

            backingField = value;
            OnPropertyChanged(propertyName);
            ValidateProperty(value, propertyName);
            return true;
        }

        private void ValidateProperty(object value, string propertyName)
        {
            if (propertyName == nameof(BalanceText))
            {
                if (double.TryParse(BalanceText, out double balance) && balance < 0)
                {
                    AddError(propertyName, "Внимание: отрицательный счет");
                }
                else
                {
                    RemoveError(propertyName);
                }
            }

            SetHasErrors();
        }

        private void SetHasErrors()
        {
            HasErrors = _errors.Values.Any(list => list != null && list.Count > 0);
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
            {
                _errors[propertyName] = new List<string>();
            }

            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        private void RemoveError(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                return _errors[propertyName];
            }

            return Enumerable.Empty<string>();
        }

        public bool HasErrors
        {
            get { return _hasErrors; }
            private set
            {
                if (_hasErrors != value)
                {
                    _hasErrors = value;
                    OnPropertyChanged(nameof(HasErrors));
                }
            }
        }

        #endregion
    }
    public class ErrorListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IEnumerable<string> errors)
            {
                return string.Join(Environment.NewLine, errors);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
