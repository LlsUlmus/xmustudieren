package bridge;

/** 中杯 */
public class MediumCup extends Beverage {
    public MediumCup(DrinkAdditive additive) {
        super(additive);
    }

    @Override
    protected String cupSize() {
        return "中";
    }
}
