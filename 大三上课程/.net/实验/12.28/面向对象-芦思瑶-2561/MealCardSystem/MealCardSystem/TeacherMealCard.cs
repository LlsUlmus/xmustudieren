using System;

namespace MealCardSystem
{
    /// <summary>
    /// 教师饭卡类 - 享受教师折扣
    /// </summary>
    public class TeacherMealCard : MealCard
    {
        private decimal _discountRate; // 折扣率（0.9表示9折）

        public decimal DiscountRate
        {
            get { return _discountRate; }
        }

        public TeacherMealCard(string name, decimal initialBalance) 
            : base(name, initialBalance)
        {
            _discountRate = 0.9m; // 教师卡享受9折优惠
        }

        /// <summary>
        /// 重写计算实际消费金额方法 - 教师卡享受9折优惠
        /// </summary>
        protected override decimal CalculateActualAmount(decimal originalAmount)
        {
            return originalAmount * _discountRate;
        }

        /// <summary>
        /// 重写月末处理 - 教师卡如果当月消费超过800元，返还当月消费额的10%
        /// </summary>
        public override void PerformMonthEndTransactions()
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            decimal amount = this.GetMonthExpenditure(currentYear, currentMonth);
            
            // 如果当月消费超过800元，返还当月消费额的10%
            if (amount > 800)
            {
                this.Recharge(amount * 0.10m, DateTime.Now, "教师卡月末返还");
            }
        }
    }
}

