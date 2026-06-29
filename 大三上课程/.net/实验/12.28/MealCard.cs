using System;
using System.Collections.Generic;
using System.Text;

namespace MealCardSystem
{
    /// <summary>
    /// 饭卡基类
    /// </summary>
    public class MealCard
    {
        protected decimal _balance;
        protected string _cardNo;
        protected string _userName;
        protected List<Transaction> _allTransactions;
        protected static int s_CardNoSeed = 1000;

        public decimal Balance
        {
            get { return _balance; }
            protected set { _balance = value; }
        }

        public string CardNo
        {
            get { return _cardNo; }
        }

        public string UserName
        {
            get { return _userName; }
        }

        public MealCard(string name, decimal initialBalance)
        {
            _userName = name;
            _balance = initialBalance;
            _cardNo = s_CardNoSeed.ToString();
            s_CardNoSeed++;
            _allTransactions = new List<Transaction>();
            if (initialBalance > 0)
            {
                _allTransactions.Add(new Transaction(initialBalance, DateTime.Now, "初始充值"));
            }
        }

        /// <summary>
        /// 消费
        /// </summary>
        /// <param name="amount">消费金额</param>
        /// <param name="date">交易日期</param>
        /// <param name="description">交易描述</param>
        public virtual void Consume(decimal amount, DateTime date, string description)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("消费金额必须大于0");
            }

            decimal actualAmount = CalculateActualAmount(amount);
            if (_balance < actualAmount)
            {
                throw new InvalidOperationException("余额不足");
            }

            _balance -= actualAmount;
            _allTransactions.Add(new Transaction(-actualAmount, date, description));
        }

        /// <summary>
        /// 计算实际消费金额（子类可以重写此方法实现不同的折扣逻辑）
        /// </summary>
        /// <param name="originalAmount">原始金额</param>
        /// <returns>实际消费金额</returns>
        protected virtual decimal CalculateActualAmount(decimal originalAmount)
        {
            return originalAmount;
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="amount">充值金额</param>
        /// <param name="date">交易日期</param>
        /// <param name="description">交易描述</param>
        public void Recharge(decimal amount, DateTime date, string description)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("充值金额必须大于0");
            }

            _balance += amount;
            _allTransactions.Add(new Transaction(amount, date, description));
        }

        /// <summary>
        /// 获取某年某月的消费额
        /// </summary>
        /// <param name="year">年份</param>
        /// <param name="month">月份</param>
        /// <returns>消费总额</returns>
        public decimal GetMonthExpenditure(int year, int month)
        {
            decimal amount = 0;
            foreach (var item in _allTransactions)
            {
                if (item.Date.Year == year && item.Date.Month == month && item.Amount < 0)
                {
                    amount += Math.Abs(item.Amount);
                }
            }
            return amount;
        }

        /// <summary>
        /// 月末处理（虚方法，子类可以重写）
        /// </summary>
        public virtual void PerformMonthEndTransactions()
        {
            // 基类默认不做任何处理
        }

        /// <summary>
        /// 显示饭卡信息
        /// </summary>
        public void DisplayCardInfo()
        {
            Console.WriteLine($"饭卡编号: {_cardNo}");
            Console.WriteLine($"持卡人: {_userName}");
            Console.WriteLine($"当前余额: {_balance:F2}");
        }

        /// <summary>
        /// 获取账户历史记录
        /// </summary>
        /// <returns>历史记录字符串</returns>
        public string GetAccountHistory()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("交易历史记录:");
            sb.AppendLine("日期时间          | 金额      | 描述");
            sb.AppendLine("----------------------------------------");
            foreach (var transaction in _allTransactions)
            {
                sb.AppendLine(transaction.ToString());
            }
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"当前余额: {_balance:F2}");
            return sb.ToString();
        }
    }
}

