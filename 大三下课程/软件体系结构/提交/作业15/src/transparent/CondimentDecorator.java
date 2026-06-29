package transparent;

public abstract class CondimentDecorator extends Beverage {
    protected Beverage beverage;

    protected CondimentDecorator(Beverage beverage) {
        this.beverage = beverage;
    }
}
