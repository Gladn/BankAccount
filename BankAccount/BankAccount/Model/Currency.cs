using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BankAccount.Model
{
    public class CurrencyRate
    {
        [Key]
        public int DateId { get; set; }
        public DateTime Date { get; set; }
        public List<Currency> Currencies { get; set; }
    }

    public class Currency
    {
        [Key]
        public string Id { get; set; }
        public string NumCode { get; set; }
        public string CharCode { get; set; }
        public double Nominal { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
    }
}