using System;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Model
{
    public class Currency
    {
        [Key]
        public string Id { get; set; }
        public string CharCode { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }
    }
}