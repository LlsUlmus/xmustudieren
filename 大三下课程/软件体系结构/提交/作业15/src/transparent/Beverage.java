package transparent;

/**
 * 透明装饰模式：组件与装饰器继承同一抽象类，对外只通过 Beverage 类型交互。
 */
public abstract class Beverage {
    protected String description = "未知饮品";

    public String getDescription() {
        return description;
    }

    public abstract double cost();
}
