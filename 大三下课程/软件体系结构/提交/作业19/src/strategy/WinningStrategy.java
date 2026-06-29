package strategy;

import java.util.Random;

/**
 * 胜则沿用、负则随机换招
 */
public class WinningStrategy implements Strategy {

    private final Random rng;
    private boolean lastWon;
    private Hand lastHand;

    public WinningStrategy(int seed) {
        this.rng = new Random(seed);
        this.lastWon = false;
    }

    @Override
    public Hand nextHand() {
        if (!lastWon) {
            lastHand = Hand.of(rng.nextInt(3));
        }
        return lastHand;
    }

    @Override
    public void study(boolean wonLastRound) {
        this.lastWon = wonLastRound;
    }
}
