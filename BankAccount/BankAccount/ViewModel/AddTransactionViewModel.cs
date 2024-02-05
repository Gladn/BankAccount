using BankAccount.Command;
using BankAccount.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml.Data;

namespace BankAccount.ViewModel
{
    public class AddTransactionViewModel : ViewModelBase, INotifyDataErrorInfo
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
            get => _transactionAmount;
            set
            {
                if (value == _transactionAmount) return;
                _transactionAmount = value;
                ValidateProperty(value, "TransactionAmount");
                OnPropertyChanged(nameof(TransactionAmount));
            }
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

            if (!currencyNamesFromDatabase.Contains("RUB"))
            {
                currencyNamesFromDatabase.Insert(0, "RUB");
            }

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

                if (SelectedCurrencyCharCode != "RUB")
                {
                    decimal amountInRubles = await _currencyConverterService.ConvertToRublesAsync(amount, SelectedCurrencyCharCode);
                    await _dataBalanceService.UpdateBalanceAsync(amountInRubles, SelectedTransactionType);
                }
                else await _dataBalanceService.UpdateBalanceAsync(amount, SelectedTransactionType);
                

                await NavigateBackAsync();
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
            await NavigateBackAsync();
        }




        #region Validation TextBox

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
            if (propertyName == nameof(TransactionAmount))
            {
                if (string.IsNullOrWhiteSpace(value as string))
                {
                    AddError(propertyName, "Поле не может быть пустым");
                }
                else if (!Regex.IsMatch(value as string, @"^[0-9,]+$"))
                {
                    AddError(propertyName, "Поле может содержать только цифры и запятые");
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
}
