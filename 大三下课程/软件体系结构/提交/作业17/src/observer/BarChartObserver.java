package observer;

import java.util.Observable;
import java.util.Observer;

import model.RandomNumberModel;

/**
 * 用星号条形图可视化随机数大小。
 */
public class BarChartObserver implements Observer {

    private final String name;
    private final char symbol;

    public BarChartObserver(String name, char symbol) {
        this.name = name;
        this.symbol = symbol;
    }

    @Override
    public void update(Observable observable, Object arg) {
        if (!(observable instanceof RandomNumberModel)) {
            return;
        }
        int value = arg instanceof Integer ? (Integer) arg : ((RandomNumberModel) observable).getCurrentValue();
        StringBuilder bar = new StringBuilder(value);
        for (int i = 0; i < value; i++) {
            bar.append(symbol);
        }
        System.out.printf("[%s] 条形图 (%d): %s%n", name, value, bar);
    }
}
