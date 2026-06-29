package bridge;

/**
 * 抽象化类：饮料订单（桥接模式 - 抽象化部分）
 */
public abstract class Beverage {
    protected DrinkAdditive additive;

    public Beverage(DrinkAdditive additive) {
        this.additive = additive;
    }

    /** 运行时更换加料方式 */
    public void setAdditive(DrinkAdditive additive) {
        this.additive = additive;
    }

    protected abstract String cupSize();

    public void serve() {
        System.out.println("【订单】" + cupSize() + "杯饮料，" + additive.describe());
    }
}
