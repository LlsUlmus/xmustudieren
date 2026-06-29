import bridge.*;


public class Client {
    public static void main(String[] args) {
        System.out.println("========== 桥接模式：饮料订购系统 ==========\n");

        System.out.println("--- 1. 杯型 × 加料 自由组合（3×3） ---");
        Beverage[] orders = {
                new LargeCup(new MilkAdditive()),
                new LargeCup(new SugarAdditive()),
                new LargeCup(new PlainAdditive()),
                new MediumCup(new MilkAdditive()),
                new MediumCup(new SugarAdditive()),
                new MediumCup(new PlainAdditive()),
                new SmallCup(new MilkAdditive()),
                new SmallCup(new SugarAdditive()),
                new SmallCup(new PlainAdditive())
        };
        for (Beverage order : orders) {
            order.serve();
        }

        System.out.println("\n--- 2. 同一杯型运行时更换加料（桥接解耦） ---");
        Beverage cup = new MediumCup(new PlainAdditive());
        cup.serve();
        cup.setAdditive(new MilkAdditive());
        cup.serve();
        cup.setAdditive(new SugarAdditive());
        cup.serve();

        System.out.println("\n========== 演示结束 ==========");
    }
}
