using BankAccount.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface IDataTransactionService
    {
        Task AddTransactionAsync(decimal amount, string currency, string transactionType);
        Task<List<Transaction>> GetTransactionsAsync();
    }


    public class DataTransactionService : IDataTransactionService
    {
        private readonly IDataBaseService _dataBaseService;

        public DataTransactionService(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;
        }


        public async Task AddTransactionAsync(decimal amount, string currency, string transactionType)
        {
            try
            {
                DateTime currentDate = DateTime.Now;

                string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

                using (var connection = new SqliteConnection($"Data Source={dbPath}"))
                {
                    await connection.OpenAsync();
                    var transaction = connection.BeginTransaction();

                    try
                    {
                        var insertTransactionCommand = connection.CreateCommand();
                        insertTransactionCommand.CommandText = @"
                            INSERT INTO Transactions (DateTime, Amount, Currency, Type)
                            VALUES ($dateTime, $amount, $currency, $type)";

                        insertTransactionCommand.Parameters.AddWithValue("$dateTime", currentDate.ToString("yyyy-MM-dd HH:mm:ss"));
                        insertTransactionCommand.Parameters.AddWithValue("$amount", amount);
                        insertTransactionCommand.Parameters.AddWithValue("$currency", currency);
                        insertTransactionCommand.Parameters.AddWithValue("$type", transactionType);

                        await insertTransactionCommand.ExecuteNonQueryAsync();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw new Exception("Ошибка при добавлении: " + ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при обращении к бд: " + ex.Message, ex);
            }
        }


        public async Task<List<Transaction>> GetTransactionsAsync()
        {
            List<Transaction> transactions = new List<Transaction>();

            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

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
                            OperationId = reader.GetInt32(0),
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
