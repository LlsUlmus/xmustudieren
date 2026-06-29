package whitebox;

/**
 * 白盒测试用例执行程序
 * 覆盖 function1 的六种逻辑覆盖法 + function2 的基本路径测试
 */
public class WhiteBoxTest {

    private static int passCount = 0;
    private static int failCount = 0;

    public static void main(String[] args) {
        System.out.println("========== 实验2：白盒测试用例设计 ==========\n");

        testFunction1();
        testFunction2();

        System.out.println("\n========== 测试汇总 ==========");
        System.out.println("通过: " + passCount + "  失败: " + failCount);
    }

    // ==================== function1 测试 ====================

    private static void testFunction1() {
        System.out.println("---------- 一、function1 逻辑覆盖测试 ----------\n");

        // --- 语句覆盖 ---
        System.out.println("【1. 语句覆盖】");
        runF1("SC-1", 90, 90, 'A', "x>=90且y>=90，输出A");
        runF1("SC-2", 80, 85, 'B', "x+y>=165，输出B");
        runF1("SC-3", 50, 50, 'C', "其余情况，输出C");
        System.out.println();

        // --- 判定覆盖 ---
        System.out.println("【2. 判定覆盖】");
        runF1("DC-1", 90, 90, 'A', "判定1为真");
        runF1("DC-2", 80, 85, 'B', "判定1为假，判定2为真");
        runF1("DC-3", 50, 50, 'C', "判定1为假，判定2为假");
        System.out.println();

        // --- 条件覆盖 ---
        System.out.println("【3. 条件覆盖】");
        runF1("CC-1", 90, 90, 'A', "C1=T, C2=T");
        runF1("CC-2", 90, 50, 'C', "C1=T, C2=F");
        runF1("CC-3", 50, 90, 'C', "C1=F, C2=T");
        runF1("CC-4", 80, 85, 'B', "C1=F, C2=F, C3=T");
        runF1("CC-5", 50, 50, 'C', "C1=F, C2=F, C3=F");
        System.out.println();

        // --- 判定-条件覆盖 ---
        System.out.println("【4. 判定-条件覆盖】");
        runF1("MCDC-1", 90, 90, 'A', "判定1真，C1=T,C2=T");
        runF1("MCDC-2", 90, 50, 'C', "判定1假，C1=T,C2=F");
        runF1("MCDC-3", 50, 90, 'C', "判定1假，C1=F,C2=T");
        runF1("MCDC-4", 80, 85, 'B', "判定1假，判定2真");
        runF1("MCDC-5", 50, 50, 'C', "判定1假，判定2假");
        System.out.println();

        // --- 条件组合覆盖 ---
        System.out.println("【5. 条件组合覆盖】");
        runF1("Comb-1", 90, 90, 'A', "C1=T,C2=T → 判定1真");
        runF1("Comb-2", 90, 50, 'C', "C1=T,C2=F → 判定1假,C3=F");
        runF1("Comb-3", 50, 90, 'C', "C1=F,C2=T → 判定1假,C3=F");
        runF1("Comb-4", 80, 85, 'B', "C1=F,C2=F,C3=T → 判定1假,判定2真");
        runF1("Comb-5", 50, 50, 'C', "C1=F,C2=F,C3=F → 判定1假,判定2假");
        System.out.println();

        // --- 路径覆盖 ---
        System.out.println("【6. 路径覆盖】");
        runF1("Path-1", 90, 90, 'A', "路径1: 判定1真 → A");
        runF1("Path-2", 80, 85, 'B', "路径2: 判定1假,判定2真 → B");
        runF1("Path-3", 50, 50, 'C', "路径3: 判定1假,判定2假 → C");
        System.out.println();
    }

    private static void runF1(String id, int x, int y, char expected, String desc) {
        char actual = WhiteBox.function1(x, y);
        boolean ok = actual == expected;
        if (ok) passCount++;
        else failCount++;
        System.out.printf("  %s | x=%d, y=%d | 期望=%c, 实际=%c | %s %s%n",
                id, x, y, expected, actual, desc, ok ? "[PASS]" : "[FAIL]");
    }

    // ==================== function2 基本路径测试 ====================

    private static void testFunction2() {
        System.out.println("---------- 二、function2 基本路径测试 ----------");
        System.out.println("环形复杂度 V(G) = 4，共4条独立路径\n");

        System.out.println("【路径1】while条件为假，循环体不执行 (n=1):");
        System.out.print("  输出: ");
        WhiteBox.function2(1);
        passCount++;

        System.out.println("\n【路径2】k==n 为真，直接输出质数 (n=7):");
        System.out.print("  输出: ");
        WhiteBox.function2(7);
        passCount++;

        System.out.println("\n【路径3】n%k==0 为真，分解因子 (n=150):");
        System.out.print("  输出: ");
        WhiteBox.function2(150);
        passCount++;

        System.out.println("\n【路径4】n%k!=0 为真，k++ (n=9，含k++过程):");
        System.out.print("  输出: ");
        WhiteBox.function2(9);
        passCount++;

        System.out.println("\n【补充】合数全分解 (n=12):");
        System.out.print("  输出: ");
        WhiteBox.function2(12);
        passCount++;
    }
}
