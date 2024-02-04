using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using BankAccount.Model;
using System.Xml.Linq;
using Windows.UI.Xaml.Shapes;
using Windows.Data.Json;

namespace BankAccount.Service
{
    public interface IDataBaseService
    {
        Task InitializeDatabaseAsync();
        Task<List<Transaction>> GetTransactionsAsync();
        Task UpdateCurrencyTableAsync();
    }


    public class DataBaseService : IDataBaseService
    {
        private readonly ICurrencyApiService _currencyApiService;

        private string dbFileName = "BankAcc.db";

        public DataBaseService(ICurrencyApiService currencyApiService)
        {
            _currencyApiService = currencyApiService;
        }

        public async Task InitializeDatabaseAsync()
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, dbFileName);

            if (!File.Exists(dbPath))
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var createTransactionsTableCommand = connection.CreateCommand();
                    createTransactionsTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Transactions (
                            OperationID INTEGER PRIMARY KEY,
                            DateTime TEXT NOT NULL,
                            Amount NUMERIC NOT NULL,
                            Currency TEXT NOT NULL,
                            Type TEXT NOT NULL
                        )";
                    await createTransactionsTableCommand.ExecuteNonQueryAsync();


                    var insertTransactionCommand = connection.CreateCommand();
                    insertTransactionCommand.CommandText = @"
                        INSERT INTO Transactions (DateTime, Amount, Currency, Type)
                        VALUES ('2024-02-03', 100.00, 'USD', 'Deposit')";
                    await insertTransactionCommand.ExecuteNonQueryAsync();


                    var createCurrenciesTableCommand = connection.CreateCommand();
                    createCurrenciesTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Currency (
                            Id TEXT PRIMARY KEY,
                            CharCode TEXT NOT NULL,
                            Name TEXT NOT NULL,
                            Value NUMERIC NOT NULL
                        )";
                    await createCurrenciesTableCommand.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            List<Transaction> transactions = new List<Transaction>();

            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, dbFileName);

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = "SELECT * FROM Transactions";

                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Transaction transaction = new Transaction
                        {
                            OperationID = reader.GetInt32(0),
                            DateTime = DateTime.Parse(reader.GetString(1)),
                            Amount = reader.GetDecimal(2),
                            Currency = reader.GetString(3),
                            Type = reader.GetString(4)
                        };
                        transactions.Add(transaction);
                    }
                }
            }
            return transactions;
        }

        public async Task UpdateCurrencyTableAsync()
        {
            try
            {
                List<Currency> currencies = await _currencyApiService.GetCurrencyRatesAsync();

                using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
                {
                    await connection.OpenAsync();

                    var transaction = connection.BeginTransaction();

                    foreach (var currency in currencies)
                    {
                        var insertCommand = connection.CreateCommand();
                        insertCommand.CommandText = @"
                            INSERT INTO Currency (Id, CharCode, Name, Value)
                            VALUES ($id, $charCode, $name, $value)";

                        insertCommand.Parameters.AddWithValue("$id", currency.Id);
                        insertCommand.Parameters.AddWithValue("$charCode", currency.CharCode);
                        insertCommand.Parameters.AddWithValue("$name", currency.Name);
                        insertCommand.Parameters.AddWithValue("$value", currency.Value);

                        await insertCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при обновлении таблицы валют: " + ex.Message, ex);
            }
        }
    }
}
