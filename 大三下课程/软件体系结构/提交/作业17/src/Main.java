import controller.RandomAppController;

public class Main {

    public static void main(String[] args) throws InterruptedException {
        int rounds = parseIntArg(args, 0, 10);
        int upperBound = parseIntArg(args, 1, 50);
        long pauseMs = parseIntArg(args, 2, 50);

        RandomAppController controller = new RandomAppController(upperBound);
        controller.run(rounds, pauseMs);
    }

    private static int parseIntArg(String[] args, int index, int defaultValue) {
        if (args.length <= index) {
            return defaultValue;
        }
        try {
            return Integer.parseInt(args[index]);
        } catch (NumberFormatException e) {
            System.err.printf("参数 %d 无效，使用默认值 %d%n", index, defaultValue);
            return defaultValue;
        }
    }
}
