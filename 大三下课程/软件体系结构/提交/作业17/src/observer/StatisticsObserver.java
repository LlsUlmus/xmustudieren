package observer;

import java.util.Observable;
import java.util.Observer;

import model.RandomNumberModel;

/**
 * 改进：累计统计已观察到的随机数（最小、最大、平均值）。
 */
public class StatisticsObserver implements Observer {

    private int count;
    private int sum;
    private int min = Integer.MAX_VALUE;
    private int max = Integer.MIN_VALUE;

    @Override
    public void update(Observable observable, Object arg) {
        if (!(observable instanceof RandomNumberModel)) {
            return;
        }
        int value = arg instanceof Integer ? (Integer) arg : ((RandomNumberModel) observable).getCurrentValue();
        count++;
        sum += value;
        min = Math.min(min, value);
        max = Math.max(max, value);
        double avg = (double) sum / count;
        System.out.printf("[统计观察者] 第 %d 次 | 当前=%d | 最小=%d | 最大=%d | 平均=%.2f%n",
                count, value, min, max, avg);
    }

    public void printSummary() {
        if (count == 0) {
            System.out.println("[统计观察者] 暂无数据");
            return;
        }
        System.out.printf("[统计观察者] 汇总: 共 %d 次, 总和=%d, 最小=%d, 最大=%d, 平均=%.2f%n",
                count, sum, min, max, (double) sum / count);
    }
}
