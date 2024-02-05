using System;

namespace BankAccount.DTOs
{
    public class TransactionDTO
    {
        public int OperationId { get; set; }
        public string DateTime { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Type { get; set; }
    }
}
