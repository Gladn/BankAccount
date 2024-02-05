using BankAccount.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface IDataBalanceService
    {
        Task UpdateBalanceAsync(decimal amountInRubles, string transactionType);
        Task<decimal> GetCurrentBalanceAsync();
    }


    public class DataBalanceService : IDataBalanceService
    {
        private readonly IDataBaseService _dataBaseService;

        public DataBalanceService(IDataBaseService dataBaseService)
        {
            _dataBaseService = dataBaseService;
        }


        public async Task UpdateBalanceAsync(decimal amountInRubles, string transactionType)
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();
                var transaction = connection.BeginTransaction();

                try
                {
                    decimal currentBalance = await GetCurrentBalanceAsync();

                    if (transactionType == "Зачисление")
                    {
                        currentBalance += amountInRubles;
                    }
                    else if (transactionType == "Снятие")
                    {
                        currentBalance -= amountInRubles;
                    }
                    else
                    {
                        throw new ArgumentException("Неподдерживаемый тип");
                    }

                    var updateBalanceCommand = connection.CreateCommand();
                    updateBalanceCommand.CommandText = @"
                        UPDATE Balance
                        SET Amount = $amount";
                    updateBalanceCommand.Parameters.AddWithValue("$amount", currentBalance);
                    await updateBalanceCommand.ExecuteNonQueryAsync();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Ошибка при обновлении баланса: " + ex.Message, ex);
                }
            }
        }


        public async Task<decimal> GetCurrentBalanceAsync()
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                decimal currentBalance = 0;

                SqliteCommand selectBalanceCommand = connection.CreateCommand();
                selectBalanceCommand.CommandText = "SELECT Amount FROM Balance";

                using (var reader = await selectBalanceCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        currentBalance = reader.GetDecimal(0);
                    }
                }

                return currentBalance;
            }
        }
    }
}