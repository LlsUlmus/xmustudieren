using System;

namespace MealCardSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== 食堂饭卡系统测试 ==========\n");

            // 测试1: 学生卡
            Console.WriteLine("【测试1: 学生饭卡】");
            StudentMealCard studentCard = new StudentMealCard("张三", 1000);
            studentCard.DisplayCardInfo();
            Console.WriteLine($"学生折扣: {(1 - studentCard.DiscountRate) * 100}% (享受{studentCard.DiscountRate * 10}折)\n");

            studentCard.Consume(100, DateTime.Now, "午餐");
            Console.WriteLine($"消费100元，实际扣除: {100 * studentCard.DiscountRate}元 (8折)");
            Console.WriteLine($"当前余额: {studentCard.Balance:F2}\n");

            studentCard.Consume(200, DateTime.Now.AddMonths(-1), "晚餐");
            Console.WriteLine($"消费200元，实际扣除: {200 * studentCard.DiscountRate}元 (8折)");
            Console.WriteLine($"当前余额: {studentCard.Balance:F2}\n");

            studentCard.Consume(150, DateTime.Now, "早餐");
            Console.WriteLine($"消费150元，实际扣除: {150 * studentCard.DiscountRate}元 (8折)");
            Console.WriteLine($"当前余额: {studentCard.Balance:F2}\n");

            studentCard.Recharge(500, DateTime.Now, "充值");
            Console.WriteLine($"充值500元，当前余额: {studentCard.Balance:F2}\n");

            studentCard.Consume(200, DateTime.Now, "午餐");
            Console.WriteLine($"消费200元，实际扣除: {200 * studentCard.DiscountRate}元 (8折)");
            Console.WriteLine($"当前余额: {studentCard.Balance:F2}\n");

            studentCard.PerformMonthEndTransactions();
            Console.WriteLine("执行月末处理（当月消费超过500元，返还5%）...");
            Console.WriteLine($"当前余额: {studentCard.Balance:F2}\n");
            Console.WriteLine(studentCard.GetAccountHistory());
            Console.WriteLine("\n");

            // 测试2: 教师卡
            Console.WriteLine("【测试2: 教师饭卡】");
            TeacherMealCard teacherCard = new TeacherMealCard("李老师", 1000);
            teacherCard.DisplayCardInfo();
            Console.WriteLine($"教师折扣: {(1 - teacherCard.DiscountRate) * 100}% (享受{teacherCard.DiscountRate * 10}折)\n");

            teacherCard.Consume(100, DateTime.Now, "午餐");
            Console.WriteLine($"消费100元，实际扣除: {100 * teacherCard.DiscountRate}元 (9折)");
            Console.WriteLine($"当前余额: {teacherCard.Balance:F2}\n");

            teacherCard.Consume(200, DateTime.Now, "晚餐");
            Console.WriteLine($"消费200元，实际扣除: {200 * teacherCard.DiscountRate}元 (9折)");
            Console.WriteLine($"当前余额: {teacherCard.Balance:F2}\n");

            teacherCard.Consume(300, DateTime.Now, "早餐");
            Console.WriteLine($"消费300元，实际扣除: {300 * teacherCard.DiscountRate}元 (9折)");
            Console.WriteLine($"当前余额: {teacherCard.Balance:F2}\n");

            teacherCard.Consume(250, DateTime.Now, "午餐");
            Console.WriteLine($"消费250元，实际扣除: {250 * teacherCard.DiscountRate}元 (9折)");
            Console.WriteLine($"当前余额: {teacherCard.Balance:F2}\n");

            teacherCard.Recharge(500, DateTime.Now, "充值");
            Console.WriteLine($"充值500元，当前余额: {teacherCard.Balance:F2}\n");

            teacherCard.PerformMonthEndTransactions();
            Console.WriteLine("执行月末处理（当月消费超过800元，返还10%）...");
            Console.WriteLine($"当前余额: {teacherCard.Balance:F2}\n");
            Console.WriteLine(teacherCard.GetAccountHistory());
            Console.WriteLine("\n");

            // 测试3: 临时卡
            Console.WriteLine("【测试3: 临时饭卡】");
            TemporaryMealCard tempCard = new TemporaryMealCard("访客", 500);
            tempCard.DisplayCardInfo();
            Console.WriteLine("临时卡无折扣\n");

            tempCard.Consume(50, DateTime.Now, "午餐");
            Console.WriteLine($"消费50元，实际扣除: 50元 (无折扣)");
            Console.WriteLine($"当前余额: {tempCard.Balance:F2}\n");

            tempCard.Consume(80, DateTime.Now, "晚餐");
            Console.WriteLine($"消费80元，实际扣除: 80元 (无折扣)");
            Console.WriteLine($"当前余额: {tempCard.Balance:F2}\n");

            tempCard.Recharge(200, DateTime.Now, "充值");
            Console.WriteLine($"充值200元，当前余额: {tempCard.Balance:F2}\n");

            tempCard.PerformMonthEndTransactions();
            Console.WriteLine("执行月末处理（临时卡无奖励）...");
            Console.WriteLine($"当前余额: {tempCard.Balance:F2}\n");
            Console.WriteLine(tempCard.GetAccountHistory());
            Console.WriteLine("\n");

            // 测试多态性
            Console.WriteLine("【测试4: 多态性演示】");
            Console.WriteLine("使用基类引用调用不同子类的月末处理方法:\n");

            MealCard[] cards = new MealCard[]
            {
                new StudentMealCard("学生1", 500),
                new TeacherMealCard("教师1", 500),
                new TemporaryMealCard("访客1", 500)
            };

            foreach (var card in cards)
            {
                card.Consume(100, DateTime.Now, "测试消费");
                Console.WriteLine($"{card.UserName} ({card.GetType().Name}) 消费后余额: {card.Balance:F2}");
                card.PerformMonthEndTransactions();
                Console.WriteLine($"{card.UserName} ({card.GetType().Name}) 月末处理后余额: {card.Balance:F2}");
                Console.WriteLine();
            }

            Console.WriteLine("========== 测试完成 ==========");
            Console.ReadKey();
        }
    }
}
