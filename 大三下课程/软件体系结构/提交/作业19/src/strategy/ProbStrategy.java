package strategy;

import java.util.Random;

/**
 * 基于历史统计的概率策略
 */
public class ProbStrategy implements Strategy {

    private final Random rng;
    private int prevValue;
    private int currValue;
    private final int[][] weights;

    public ProbStrategy(int seed) {
        this.rng = new Random(seed);
        this.weights = new int[][]{
                {1, 1, 1},
                {1, 1, 1},
                {1, 1, 1}
        };
    }

    @Override
    public Hand nextHand() {
        int rowSum = rowTotal(currValue);
        int pick = rng.nextInt(rowSum);
        int choice = 0;
        while (pick >= weights[currValue][choice]) {
            pick -= weights[currValue][choice];
            choice++;
        }
        prevValue = currValue;
        currValue = choice;
        return Hand.of(choice);
    }

    private int rowTotal(int row) {
        int sum = 0;
        for (int c = 0; c < 3; c++) {
            sum += weights[row][c];
        }
        return sum;
    }

    @Override
    public void study(boolean wonLastRound) {
        if (wonLastRound) {
            weights[prevValue][currValue]++;
        } else {
            weights[prevValue][(currValue + 1) % 3]++;
            weights[prevValue][(currValue + 2) % 3]++;
        }
    }
}
