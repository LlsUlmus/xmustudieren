package strategy;

/**
 * 上下文：持有策略并统计战绩
 */
public class Player {

    private final String name;
    private Strategy strategy;
    private int wins;
    private int losses;
    private int rounds;

    public Player(String name, Strategy strategy) {
        this.name = name;
        this.strategy = strategy;
    }

    public void setStrategy(Strategy strategy) {
        this.strategy = strategy;
    }

    public String getName() {
        return name;
    }

    public Hand play() {
        return strategy.nextHand();
    }

    public void recordWin() {
        strategy.study(true);
        wins++;
        rounds++;
    }

    public void recordLoss() {
        strategy.study(false);
        losses++;
        rounds++;
    }

    public void recordDraw() {
        rounds++;
    }

    public void resetStats() {
        wins = 0;
        losses = 0;
        rounds = 0;
    }

    @Override
    public String toString() {
        return "[" + name + ": " + rounds + " games, " + wins + " win, " + losses + " lose]";
    }
}
