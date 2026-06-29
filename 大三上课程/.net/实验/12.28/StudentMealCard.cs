using System;

namespace MealCardSystem
{
    /// <summary>
    /// 学生饭卡类 - 享受学生折扣
    /// </summary>
    public class StudentMealCard : MealCard
    {
        private decimal _discountRate; // 折扣率（0.8表示8折）

        public decimal DiscountRate
        {
            get { return _discountRate; }
        }

        public StudentMealCard(string name, decimal initialBalance) 
            : base(name, initialBalance)
        {
            _discountRate = 0.8m; // 学生卡享受8折优惠
        }

        /// <summary>
        /// 重写计算实际消费金额方法 - 学生卡享受8折优惠
        /// </summary>
        protected override decimal CalculateActualAmount(decimal originalAmount)
        {
            return originalAmount * _discountRate;
        }

        /// <summary>
        /// 重写月末处理 - 学生卡如果当月消费超过500元，返还当月消费额的5%
        /// </summary>
        public override void PerformMonthEndTransactions()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            decimal amount = this.GetMonthExpenditure(currentYear, currentMonth);
            
            // 如果当月消费超过500元，返还当月消费额的5%
            if (amount > 500)
            {
                this.Recharge(amount * 0.05m, DateTime.Now, "学生卡月末返还");
            }
        }
    }
}

