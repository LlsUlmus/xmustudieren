package bridge;

/** 小杯 */
public class SmallCup extends Beverage {
    public SmallCup(DrinkAdditive additive) {
        super(additive);
    }

    @Override
    protected String cupSize() {
        return "小";
    }
}
