using BankAccount.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface IDataCurrencyService
    {
        Task<List<string>> GetCurrencyCharCodeAsync();
        Task<DateTime> GetLastCurrencyDateAsync();
        Task<Currency> GetCurrencyDataAsync(string currency, DateTime date);
        Task<bool> IsCurrencyDataAvailableAsync();
    }


    public class DataCurrencyService : IDataCurrencyService
    {
        private readonly IDataBaseService _dataBaseService;

        public DataCurrencyService(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;
        }

        public async Task<List<string>> GetCurrencyCharCodeAsync()
        {
            List<string> currencyNames = new List<string>();

            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT DISTINCT CharCode FROM Currency";

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string currencyName = reader.GetString(0);
                        currencyNames.Add(currencyName);
                    }
                }
            }

            return currencyNames;
        }


        public async Task<DateTime> GetLastCurrencyDateAsync()
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT Date FROM CurrencyDate ORDER BY Date DESC LIMIT 1";

                var result = await command.ExecuteScalarAsync();
                if (result == null || result == DBNull.Value)
                {
                    throw new Exception("Нет инфы в бд");
                }

                return DateTime.Parse(result.ToString());
            }
        }


        public async Task<Currency> GetCurrencyDataAsync(string currency, DateTime date)
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());
            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT NumCode, CharCode, Nominal, Name, Value
                    FROM Currency
                    INNER JOIN CurrencyDate ON Currency.DateId = CurrencyDate.DateId
                    WHERE Currency.CharCode = $charCode AND CurrencyDate.Date = $date";
                command.Parameters.AddWithValue("$charCode", currency);
                command.Parameters.AddWithValue("$date", date.ToString("yyyy-MM-dd"));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new Currency
                        {
                            NumCode = reader.GetString(0),
                            CharCode = reader.GetString(1),
                            Nominal = reader.GetDouble(2),
                            Name = reader.GetString(3),
                            Value = reader.GetDouble(4)
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public async Task<bool> IsCurrencyDataAvailableAsync()
        {
            try
            {
                string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Currency";
                        var result = await command.ExecuteScalarAsync();
                        int count = Convert.ToInt32(result);

                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при проверке наличия данных о валюте в базе данных: " + ex.Message, ex);
            }
        }
    }
}
