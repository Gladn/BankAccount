using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface ICurrencyApiService
    {
        Task<string> GetCurrencyRatesAsync();
    }

    public class CurrencyApiService : ICurrencyApiService
    {
        private readonly HttpClient _httpClient;

        public CurrencyApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetCurrencyRatesAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("https://www.cbr-xml-daily.ru/daily_json.js");

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при отправке запроса к API: " + ex.Message, ex);
            }
        }
    }
}
