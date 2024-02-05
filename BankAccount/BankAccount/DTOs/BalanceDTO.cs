using BankAccount.Model;
using System;

namespace BankAccount.DTOs
{
    public class BalanceDTO
    {
        public int BalanceId { get; set; }
        public decimal Amount { get; set; }
    }

    public class BalanceMapper
    {
        public static BalanceDTO MapToDTO(Balance balance)
        {
            if (balance == null)
            {
                throw new ArgumentNullException(nameof(balance));
            }

            return new BalanceDTO
            {
                BalanceId = balance.BalanceId,
                Amount = balance.Amount
            };
        }
    }
}
