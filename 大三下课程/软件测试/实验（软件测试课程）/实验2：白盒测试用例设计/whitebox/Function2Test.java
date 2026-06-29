package whitebox;

/**
 * 题目二：function2 基本路径测试
 */
public class Function2Test {

    public static void main(String[] args) {
        System.out.println("========== 题目二：function2 基本路径测试 ==========");
        System.out.println("环形复杂度 V(G) = 4，共4条独立路径\n");

        System.out.println("【路径1】while条件为假，循环体不执行 (n=1):");
        System.out.print("  输出: ");
        WhiteBox.function2(1);

        System.out.println("\n【路径2】k==n 为真，直接输出质数 (n=7):");
        System.out.print("  输出: ");
        WhiteBox.function2(7);

        System.out.println("\n【路径3】n%k==0 为真，分解因子 (n=150):");
        System.out.print("  输出: ");
        WhiteBox.function2(150);

        System.out.println("\n【路径4】n%k!=0 为真，k++ (n=9，含k++过程):");
        System.out.print("  输出: ");
        WhiteBox.function2(9);

        System.out.println("\n【补充】合数全分解 (n=12):");
        System.out.print("  输出: ");
        WhiteBox.function2(12);

        System.out.println("\n========== 测试完成 ==========");
    }
}
