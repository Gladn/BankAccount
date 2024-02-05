using BankAccount.Model;
using System;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface ICurrencyConverterService
    {
        Task<decimal> ConvertToRublesAsync(decimal amount, string currency);
    }


    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly IDataCurrencyService _dataCurrencyService;

        public CurrencyConverterService(IDataCurrencyService dataCurrencyService)
        {
            _dataCurrencyService = dataCurrencyService;
        }

        public async Task<decimal> ConvertToRublesAsync(decimal amount, string currency)
        {
            DateTime lastDate = await _dataCurrencyService.GetLastCurrencyDateAsync();

            Currency currencyData = await _dataCurrencyService.GetCurrencyDataAsync(currency, lastDate);

            if (currencyData == null)
            {
                throw new Exception($"Данные о валюте '{currency}' на дату '{lastDate}' не найдены.");
            }


            decimal equivalentAmount = amount *  (decimal)(currencyData.Value / currencyData.Nominal);

            return equivalentAmount;
        }
    }
}
