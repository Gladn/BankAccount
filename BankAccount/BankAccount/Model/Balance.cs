using System.ComponentModel.DataAnnotations;

namespace BankAccount.Model
{
    public class Balance
    {
        [Key]
        public int BalanceId { get; set; }
        public decimal Amount { get; set; }
    }
}