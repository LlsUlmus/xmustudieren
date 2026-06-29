package whitebox;

/**
 * 题目一：function1 逻辑覆盖测试
 */
public class Function1Test {

    private static int passCount = 0;
    private static int failCount = 0;

    public static void main(String[] args) {
        System.out.println("========== 题目一：function1 逻辑覆盖测试 ==========\n");

        // --- 语句覆盖 ---
        System.out.println("【1. 语句覆盖】");
        run("SC-1", 90, 90, 'A', "x>=90且y>=90，输出A");
        run("SC-2", 80, 85, 'B', "x+y>=165，输出B");
        run("SC-3", 50, 50, 'C', "其余情况，输出C");
        System.out.println();

        // --- 判定覆盖 ---
        System.out.println("【2. 判定覆盖】");
        run("DC-1", 90, 90, 'A', "判定1为真");
        run("DC-2", 80, 85, 'B', "判定1为假，判定2为真");
        run("DC-3", 50, 50, 'C', "判定1为假，判定2为假");
        System.out.println();

        // --- 条件覆盖 ---
        System.out.println("【3. 条件覆盖】");
        run("CC-1", 90, 90, 'A', "C1=T, C2=T");
        run("CC-2", 90, 50, 'C', "C1=T, C2=F");
        run("CC-3", 50, 90, 'C', "C1=F, C2=T");
        run("CC-4", 80, 85, 'B', "C1=F, C2=F, C3=T");
        run("CC-5", 50, 50, 'C', "C1=F, C2=F, C3=F");
        System.out.println();

        // --- 判定-条件覆盖 ---
        System.out.println("【4. 判定-条件覆盖】");
        run("MCDC-1", 90, 90, 'A', "判定1真，C1=T,C2=T");
        run("MCDC-2", 90, 50, 'C', "判定1假，C1=T,C2=F");
        run("MCDC-3", 50, 90, 'C', "判定1假，C1=F,C2=T");
        run("MCDC-4", 80, 85, 'B', "判定1假，判定2真");
        run("MCDC-5", 50, 50, 'C', "判定1假，判定2假");
        System.out.println();

        // --- 条件组合覆盖 ---
        System.out.println("【5. 条件组合覆盖】");
        run("Comb-1", 90, 90, 'A', "C1=T,C2=T → 判定1真");
        run("Comb-2", 90, 50, 'C', "C1=T,C2=F → 判定1假,C3=F");
        run("Comb-3", 50, 90, 'C', "C1=F,C2=T → 判定1假,C3=F");
        run("Comb-4", 80, 85, 'B', "C1=F,C2=F,C3=T → 判定1假,判定2真");
        run("Comb-5", 50, 50, 'C', "C1=F,C2=F,C3=F → 判定1假,判定2假");
        System.out.println();

        // --- 路径覆盖 ---
        System.out.println("【6. 路径覆盖】");
        run("Path-1", 90, 90, 'A', "路径1: 判定1真 → A");
        run("Path-2", 80, 85, 'B', "路径2: 判定1假,判定2真 → B");
        run("Path-3", 50, 50, 'C', "路径3: 判定1假,判定2假 → C");

        System.out.println("\n========== 测试汇总 ==========");
        System.out.println("通过: " + passCount + "  失败: " + failCount);
    }

    private static void run(String id, int x, int y, char expected, String desc) {
        char actual = WhiteBox.function1(x, y);
        boolean ok = actual == expected;
        if (ok) passCount++;
        else failCount++;
        System.out.printf("  %s | x=%d, y=%d | 期望=%c, 实际=%c | %s %s%n",
                id, x, y, expected, actual, desc, ok ? "[PASS]" : "[FAIL]");
    }
}
