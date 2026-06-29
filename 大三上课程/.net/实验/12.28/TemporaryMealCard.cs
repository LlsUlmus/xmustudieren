using System;

namespace MealCardSystem
{
    /// <summary>
    /// 临时饭卡类 - 无折扣，无月末奖励
    /// </summary>
    public class TemporaryMealCard : MealCard
    {
        public TemporaryMealCard(string name, decimal initialBalance) 
            : base(name, initialBalance)
        {
        }

        /// <summary>
        /// 临时卡不重写CalculateActualAmount，使用基类的实现（无折扣）
        /// </summary>

        /// <summary>
        /// 重写月末处理 - 临时卡不做任何处理
        /// </summary>
        public override void PerformMonthEndTransactions()
        {
            // 临时卡月末不做任何处理
        }
    }
}

