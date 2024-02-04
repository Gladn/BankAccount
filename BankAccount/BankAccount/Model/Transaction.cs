using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Model
{
    public class Transaction
    {
        [Key]
        public int OperationID { get; set; }

        public DateTime DateTime { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; }

        public string Type { get; set; }
    }
}