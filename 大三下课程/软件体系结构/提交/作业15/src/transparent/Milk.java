package transparent;

public class Milk extends CondimentDecorator {

    public Milk(Beverage beverage) {
        super(beverage);
    }

    @Override
    public String getDescription() {
        return beverage.getDescription() + " + 牛奶";
    }

    @Override
    public double cost() {
        return beverage.cost() + 2.0;
    }
}
