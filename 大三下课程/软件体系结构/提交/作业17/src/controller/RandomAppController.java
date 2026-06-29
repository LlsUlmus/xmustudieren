package controller;

import java.util.Observer;

import model.RandomNumberModel;
import observer.BarChartObserver;
import observer.DigitDisplayObserver;
import observer.StatisticsObserver;

/**
 * MVC 中的 Controller：组装 Model 与多个 Observer，并驱动随机数生成流程。
 */
public class RandomAppController {

    private final RandomNumberModel model;
    private final StatisticsObserver statsObserver;

    public RandomAppController(int upperBound) {
        this.model = new RandomNumberModel(upperBound);
        this.statsObserver = new StatisticsObserver();
        wireObservers();
    }

    private void wireObservers() {
        // Observable 按“后注册先通知”的逆序回调，故此处逆序注册以得到 数字→条形图→统计 的输出
        model.addObserver(statsObserver);
        model.addObserver(new BarChartObserver("条形图", '*'));
        model.addObserver(new DigitDisplayObserver("数字输出"));
    }

    public void addObserver(Observer observer) {
        model.addObserver(observer);
    }

    public void removeObserver(Observer observer) {
        model.deleteObserver(observer);
    }

    public RandomNumberModel getModel() {
        return model;
    }

    /**
     * 连续生成若干次随机数，每次通知所有观察者。
     */
    public void run(int rounds, long pauseMillis) throws InterruptedException {
        System.out.println("=== 观察者模式演示：随机数生成 ===");
        System.out.printf("范围 [0, %d)，共生成 %d 次%n%n",
                model.getUpperBound(), rounds);

        for (int i = 1; i <= rounds; i++) {
            System.out.println("--- 第 " + i + " 轮 ---");
            model.nextValue();
            if (pauseMillis > 0) {
                Thread.sleep(pauseMillis);
            }
            System.out.println();
        }

        System.out.println("=== 生成结束 ===");
        statsObserver.printSummary();
        System.out.printf("Model 记录的总生成次数: %d%n", model.getGenerationCount());
        System.out.printf("当前注册的观察者数量: %d%n", model.countObservers());
    }
}
