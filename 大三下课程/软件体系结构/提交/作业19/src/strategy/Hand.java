package strategy;

/**
 * 手势：石头、剪刀、布（策略模式示例中的领域对象）
 */
public class Hand {

    public static final int ROCK = 0;
    public static final int SCISSORS = 1;
    public static final int PAPER = 2;

    private static final Hand[] INSTANCES = {
            new Hand(ROCK),
            new Hand(SCISSORS),
            new Hand(PAPER)
    };

    private static final String[] LABELS = {"石头", "剪刀", "布"};

    private final int value;

    private Hand(int value) {
        this.value = value;
    }

    public static Hand of(int value) {
        if (value < 0 || value > 2) {
            throw new IllegalArgumentException("无效手势: " + value);
        }
        return INSTANCES[value];
    }

    public boolean beats(Hand other) {
        return compare(other) > 0;
    }

    public boolean losesTo(Hand other) {
        return compare(other) < 0;
    }

    /** @return 1 胜，0 平，-1 负 */
    private int compare(Hand other) {
        if (this == other) {
            return 0;
        }
        if ((this.value + 1) % 3 == other.value) {
            return 1;
        }
        return -1;
    }

    @Override
    public String toString() {
        return LABELS[value];
    }
}
