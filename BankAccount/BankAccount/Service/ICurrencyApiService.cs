using BankAccount.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BankAccount.Service
{
    public interface ICurrencyApiService
    {
        Task<List<CurrencyRate>> GetCurrencyRatesAsync();
    }

    public class CurrencyApiService : ICurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public CurrencyApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<CurrencyRate>> GetCurrencyRatesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://www.cbr-xml-daily.ru/daily_json.js");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonObject rootObject = JsonObject.Parse(responseBody);
                DateTime date = DateTime.Parse(rootObject.GetNamedString("Date"));
                JsonObject valuteObject = rootObject["Valute"].GetObject();

                List<CurrencyRate> currencyRates = new List<CurrencyRate>();

                // Создаем или получаем существующий CurrencyRate для текущей даты
                CurrencyRate currentCurrencyRate = currencyRates.FirstOrDefault(rate => rate.Date == date);
                
                if (currentCurrencyRate == null)
                {
                    currentCurrencyRate = new CurrencyRate
                    {
                        Date = date,
                        Currencies = new List<Currency>()
                    };
                    currencyRates.Add(currentCurrencyRate);
                }


                foreach (var valute in valuteObject)
                {
                    JsonObject currencyObject = valute.Value.GetObject();

                    Currency currency = new Currency
                    {
                        Id = currencyObject.GetNamedString("ID"),
                        NumCode = currencyObject.GetNamedString("NumCode"),
                        CharCode = currencyObject.GetNamedString("CharCode"),
                        Nominal = currencyObject.GetNamedNumber("Nominal"),
                        Name = currencyObject.GetNamedString("Name"),
                        Value = currencyObject.GetNamedNumber("Value")
                    };

                    currentCurrencyRate.Currencies.Add(currency);
                }

                return currencyRates;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при отправке запроса к API: " + ex.Message, ex);
            }
        }
    }
}
