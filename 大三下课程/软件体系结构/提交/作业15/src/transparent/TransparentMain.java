package transparent;

public class TransparentMain {

    public static void main(String[] args) {
        // 客户端始终用 Beverage 引用，无需知道具体装饰器类型
        Beverage drink = new Espresso();
        drink = new Milk(drink);
        drink = new Sugar(drink);

        System.out.println("饮品: " + drink.getDescription());
        System.out.println("价格: " + drink.cost() + " 元");
    }
}
