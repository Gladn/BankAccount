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
                    await connection.OpenAsync();

                    SqliteCommand createTransactionsTableCommand = connection.CreateCommand();
                    createTransactionsTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Transactions (
                            OperationID INTEGER PRIMARY KEY,
                            DateTime TEXT NOT NULL,
                            Amount NUMERIC NOT NULL,
                            Currency TEXT NOT NULL,
                            Type TEXT NOT NULL
                        )";
                    await createTransactionsTableCommand.ExecuteNonQueryAsync();


                    SqliteCommand insertTransactionCommand = connection.CreateCommand();
                    insertTransactionCommand.CommandText = @"
                        INSERT INTO Transactions (DateTime, Amount, Currency, Type)
                        VALUES ('2024-02-03', 100.00, 'USD', 'Deposit')";
                    await insertTransactionCommand.ExecuteNonQueryAsync();


                    SqliteCommand createCurrencyDateTableCommand = connection.CreateCommand();
                    createCurrencyDateTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS CurrencyDate (
                            DateId INTEGER PRIMARY KEY,
                            Date TEXT NOT NULL
                        )";
                    await createCurrencyDateTableCommand.ExecuteNonQueryAsync();


                    SqliteCommand createCurrencyTableCommand = connection.CreateCommand();
                    createCurrencyTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Currency (
                            Id TEXT PRIMARY KEY,
                            DateId INTEGER NOT NULL,
                            NumCode TEXT NOT NULL,
                            CharCode TEXT NOT NULL,
                            Nominal NUMERIC NOT NULL,
                            Name TEXT NOT NULL,
                            Value NUMERIC NOT NULL,
                            FOREIGN KEY (DateId) REFERENCES CurrencyDate (DateId)
                        )";
                    await createCurrencyTableCommand.ExecuteNonQueryAsync();
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
                List<CurrencyRate> currencyRates = await _currencyApiService.GetCurrencyRatesAsync();

                using (var connection = new SqliteConnection($"Data Source={dbFileName}"))
                {
                    await connection.OpenAsync();
                    var transaction = connection.BeginTransaction();

                    try
                    {
                        foreach (var currencyRate in currencyRates)
                        {
                            bool dateExists;

                            using (var checkDateCommand = connection.CreateCommand())
                            {
                                checkDateCommand.CommandText = "SELECT EXISTS (SELECT 1 FROM CurrencyDate WHERE Date = $date)";
                                checkDateCommand.Parameters.AddWithValue("$date", currencyRate.Date.ToString("yyyy-MM-dd"));
                                dateExists = Convert.ToBoolean(await checkDateCommand.ExecuteScalarAsync());
                            }


                            if (dateExists)
                            {
                                transaction.Rollback();
                                return;
                            }

                            var insertDateCommand = connection.CreateCommand();
                            insertDateCommand.CommandText = @"
                                INSERT INTO CurrencyDate (Date)
                                VALUES ($date)";
                            insertDateCommand.Parameters.AddWithValue("$date", currencyRate.Date.ToString("yyyy-MM-dd"));
                            await insertDateCommand.ExecuteNonQueryAsync();


                            int dateId;
                            using (var getDateIdCommand = connection.CreateCommand())
                            {
                                getDateIdCommand.CommandText = "SELECT last_insert_rowid()";
                                dateId = Convert.ToInt32(await getDateIdCommand.ExecuteScalarAsync());
                            }


                            foreach (var currency in currencyRate.Currencies)
                            {
                                var insertValueCommand = connection.CreateCommand();
                                insertValueCommand.CommandText = @"
                                    INSERT INTO Currency (DateId, Id, NumCode, CharCode, Nominal, Name, Value)
                                    VALUES ($dateId, $id, $numCode, $charCode, $nominal, $name, $value)";
                                insertValueCommand.Parameters.AddWithValue("$dateId", dateId);
                                insertValueCommand.Parameters.AddWithValue("$id", currency.Id);
                                insertValueCommand.Parameters.AddWithValue("$numCode", currency.NumCode);
                                insertValueCommand.Parameters.AddWithValue("$charCode", currency.CharCode);
                                insertValueCommand.Parameters.AddWithValue("$nominal", currency.Nominal);
                                insertValueCommand.Parameters.AddWithValue("$name", currency.Name);
                                insertValueCommand.Parameters.AddWithValue("$value", currency.Value);
                                await insertValueCommand.ExecuteNonQueryAsync();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Ошибка при обновлении этой валюты: " + ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при обновлении валюты: " + ex.Message, ex);
            }
        }
    }
}
