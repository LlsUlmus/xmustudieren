using System;

namespace MealCardSystem
{
    /// <summary>
    /// 交易记录类
    /// </summary>
    public class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }

        public Transaction(decimal amount, DateTime date, string description)
        {
            Amount = amount;
            Date = date;
            Description = description;
        }

        public override string ToString()
        {
            return $"{Date:yyyy-MM-dd HH:mm:ss} | {Amount,10:F2} | {Description}";
        }
    }
}

