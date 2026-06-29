package observer;

import java.util.Observable;
import java.util.Observer;

import model.RandomNumberModel;

/**
 * 以纯数字形式输出每次生成的随机数。
 */
public class DigitDisplayObserver implements Observer {

    private final String name;

    public DigitDisplayObserver(String name) {
        this.name = name;
    }

    @Override
    public void update(Observable observable, Object arg) {
        if (!(observable instanceof RandomNumberModel)) {
            return;
        }
        int value = resolveValue(arg, (RandomNumberModel) observable);
        System.out.printf("[%s] 数值: %d%n", name, value);
    }

    private static int resolveValue(Object arg, RandomNumberModel model) {
        if (arg instanceof Integer) {
            return (Integer) arg;
        }
        return model.getCurrentValue();
    }
}
