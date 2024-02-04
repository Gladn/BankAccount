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
        Task InitializeDatabaseAsync();
        Task<List<Transaction>> GetTransactionsAsync();
    }


    public class DataBaseService : IDataBaseService
    {

        private string dbFileName = "BankAcc.db";


        public async Task InitializeDatabaseAsync()
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, dbFileName);

            if (!File.Exists(dbPath))
            {
                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    connection.Open();
                    var createTableCommand = connection.CreateCommand();
                    createTableCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        OperationID INTEGER PRIMARY KEY,
                        DateTime TEXT NOT NULL,
                        Amount NUMERIC NOT NULL,
                        Currency TEXT NOT NULL,
                        Type TEXT NOT NULL
                    )";
                    await createTableCommand.ExecuteNonQueryAsync();

                    // Добавление одной строки в таблицу Transactions
                    var insertCommand = connection.CreateCommand();
                    insertCommand.CommandText = @"
                        INSERT INTO Transactions (DateTime, Amount, Currency, Type)
                        VALUES ('2024-02-03', 100.00, 'USD', 'Deposit')";
                    await insertCommand.ExecuteNonQueryAsync();
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
    }
}
