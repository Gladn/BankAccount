using BankAccount.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace BankAccount.Service
{
    public interface ICurrencyApiService
    {
        Task<List<Currency>> GetCurrencyRatesAsync();
    }

    public class CurrencyApiService : ICurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public CurrencyApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<List<Currency>> GetCurrencyRatesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://www.cbr-xml-daily.ru/daily_json.js");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                JsonObject rootObject = JsonObject.Parse(responseBody);
                JsonObject valuteObject = rootObject["Valute"].GetObject();
                List<Currency> currencies = new List<Currency>();

                foreach (var valute in valuteObject)
                {
                    JsonObject currencyObject = valute.Value.GetObject();

                    Currency currency = new Currency
                    {
                        Id = currencyObject.GetNamedString("ID"),
                        CharCode = currencyObject.GetNamedString("CharCode"),
                        Name = currencyObject.GetNamedString("Name"),
                        Value = (decimal)currencyObject.GetNamedNumber("Value")
                    };
                    currencies.Add(currency);
                }

                return currencies;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при отправке запроса к API: " + ex.Message, ex);
            }
        }       
    }
}
