package model;

import java.util.Observable;
import java.util.Random;

/**
 * 被观察者（Model）：产生随机数并在状态变化时通知所有 Observer。
 * 使用 JDK 的 java.util.Observable。
 */
public class RandomNumberModel extends Observable {

    private final Random random = new Random();
    private final int upperBound;
    private int currentValue;
    private int generationCount;

    /** 防止 Observer 回调中再次触发通知造成循环（课件“标志位”思路） */
    private boolean inNotification;

    public RandomNumberModel(int upperBound) {
        if (upperBound <= 0) {
            throw new IllegalArgumentException("上界必须为正整数");
        }
        this.upperBound = upperBound;
    }

    public int getCurrentValue() {
        return currentValue;
    }

    public int getUpperBound() {
        return upperBound;
    }

    public int getGenerationCount() {
        return generationCount;
    }

    /**
     * 生成一个新随机数并通知观察者。
     *
     * @return 本次生成的数值
     */
    public int nextValue() {
        if (inNotification) {
            return currentValue;
        }
        currentValue = random.nextInt(upperBound);
        generationCount++;
        setChanged();
        notifyObservers(currentValue);
        return currentValue;
    }

    @Override
    public void notifyObservers(Object arg) {
        if (!hasChanged()) {
            return;
        }
        inNotification = true;
        try {
            super.notifyObservers(arg);
        } finally {
            inNotification = false;
            clearChanged();
        }
    }
}
