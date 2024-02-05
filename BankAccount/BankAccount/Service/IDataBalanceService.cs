using BankAccount.DTOs;
using BankAccount.Model;
using Microsoft.Data.Sqlite;
using System;
using System.Threading.Tasks;

namespace BankAccount.Service
{
    public interface IDataBalanceService
    {
        Task UpdateBalanceAsync(decimal amountInRubles, string transactionType);
        Task<Balance> GetCurrentBalanceAsync();
       
        Task<BalanceDTO> GetCurrentBalanceDTOAsync(); //С этим еще подумать
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
                    Balance currentBalance = await GetCurrentBalanceAsync();

                    if (transactionType == "Зачисление")
                    {
                        currentBalance.Amount += amountInRubles;
                    }
                    else if (transactionType == "Снятие")
                    {
                        currentBalance.Amount -= amountInRubles;
                    }
                    else
                    {
                        throw new ArgumentException("Неподдерживаемый тип");
                    }

                    var updateBalanceCommand = connection.CreateCommand();
                    updateBalanceCommand.CommandText = @"
                    UPDATE Balance
                    SET Amount = $amount";
                    updateBalanceCommand.Parameters.AddWithValue("$amount", currentBalance.Amount);
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


        public async Task<Balance> GetCurrentBalanceAsync()
        {
            string dbPath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dataBaseService.GetDbFileName());

            using (var connection = new SqliteConnection($"Data Source={dbPath}"))
            {
                await connection.OpenAsync();

                Balance currentBalance = null;

                SqliteCommand selectBalanceCommand = connection.CreateCommand();
                selectBalanceCommand.CommandText = "SELECT BalanceId, Amount FROM Balance";

                using (var reader = await selectBalanceCommand.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        currentBalance = new Balance
                        {
                            BalanceId = reader.GetInt32(0),
                            Amount = reader.GetDecimal(1)
                        };
                    }
                }

                return currentBalance;
            }
        }

        public async Task<BalanceDTO> GetCurrentBalanceDTOAsync()
        {
            Balance currentBalance = await GetCurrentBalanceAsync();
            return BalanceMapper.MapToDTO(currentBalance);
        }
    }
}