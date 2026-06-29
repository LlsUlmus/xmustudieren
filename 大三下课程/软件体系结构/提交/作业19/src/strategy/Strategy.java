package strategy;

/**
 * 策略接口：决定下一出手势，并根据胜负学习
 */
public interface Strategy {

    Hand nextHand();

    void study(boolean wonLastRound);
}
