package bridge;

/** 大杯 */
public class LargeCup extends Beverage {
    public LargeCup(DrinkAdditive additive) {
        super(additive);
    }

    @Override
    protected String cupSize() {
        return "大";
    }
}
