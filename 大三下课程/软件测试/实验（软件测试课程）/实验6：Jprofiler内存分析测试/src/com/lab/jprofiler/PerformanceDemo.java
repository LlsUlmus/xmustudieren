package com.lab.jprofiler;

/**
 * 模拟性能问题：低效的字符串拼接与重复计算。
 * 在 JProfiler 的 CPU Views / Hot Spots 中可定位到 slowConcat 与 heavyCalculation。
 */
public class PerformanceDemo {

    public static void run() {
        System.out.println("开始 CPU 密集型计算，观察 Hot Spots...");

        long sum = 0;
        for (int i = 0; i < 500; i++) {
            sum += heavyCalculation(i);
            slowConcat(i);
        }

        System.out.println("计算结果校验值: " + sum);
        System.out.println("性能演示完成。请在 CPU Views 中查看 heavyCalculation、slowConcat 耗时占比。");
    }

    /** 故意使用 O(n^2) 的字符串拼接 */
    private static void slowConcat(int seed) {
        String result = "";
        for (int i = 0; i < 8000; i++) {
            result += seed + "-" + i + ";";
        }
        if (result.length() == 0) {
            System.out.println("unexpected");
        }
    }

    /** 故意做大量无用循环 */
    private static long heavyCalculation(int n) {
        long total = 0;
        for (int i = 0; i < 500_000; i++) {
            total += (long) Math.sqrt(n * i + 1);
        }
        return total;
    }
}
