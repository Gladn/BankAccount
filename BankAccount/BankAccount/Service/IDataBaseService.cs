using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading.Tasks;
using BankAccount.Model;


namespace BankAccount.Service
{
    public interface IDataBaseService
    {
        string GetDbFileName();
        Task InitializeDatabaseAsync();
        Task UpdateCurrencyTableFromApiAsync();       
    }


    public class DataBaseService : IDataBaseService
    {
        private readonly ICurrencyApiService _currencyApiService;

        private string dbFileName = "BankAcc.db";

        public DataBaseService(ICurrencyApiService currencyApiService)
        {
            _currencyApiService = currencyApiService;
        }

        public string GetDbFileName()
        {
            return dbFileName;
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
                            Id TEXT NOT NULL,
                            DateId INTEGER NOT NULL,
                            NumCode TEXT NOT NULL,
                            CharCode TEXT NOT NULL,
                            Nominal NUMERIC NOT NULL,
                            Name TEXT NOT NULL,
                            Value NUMERIC NOT NULL,
                            PRIMARY KEY (DateId, Id),
                            FOREIGN KEY (DateId) REFERENCES CurrencyDate (DateId)
                        )";
                    await createCurrencyTableCommand.ExecuteNonQueryAsync();


                    SqliteCommand createBalanceTableCommand = connection.CreateCommand();
                    createBalanceTableCommand.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Balance (
                            BalanceId INTEGER PRIMARY KEY,
                            Amount NUMERIC NOT NULL
                        )";
                    await createBalanceTableCommand.ExecuteNonQueryAsync();


                    SqliteCommand insertBalanceCommand = connection.CreateCommand();
                    insertBalanceCommand.CommandText = @"
                        INSERT INTO Balance (Amount)
                        VALUES (0.00)";
                    await insertBalanceCommand.ExecuteNonQueryAsync();
                }
            }
        }
        

        public async Task UpdateCurrencyTableFromApiAsync()
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
                        throw new Exception("Ошибка апи при обновлении последней валюты: " + ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка апи при обновлении валют: " + ex.Message, ex);
            }
        }
    }
}
