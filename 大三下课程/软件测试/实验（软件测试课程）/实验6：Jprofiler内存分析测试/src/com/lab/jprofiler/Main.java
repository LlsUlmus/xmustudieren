package com.lab.jprofiler;

/**
 * JProfiler 实验演示程序入口。
 * 依次触发：内存泄漏、线程阻塞、性能热点三类典型问题，便于在 JProfiler 中观察与分析。
 */
public class Main {

    public static void main(String[] args) throws Exception {
        System.out.println("=== JProfiler 实验演示程序 ===");
        System.out.println("1. 内存泄漏演示");
        System.out.println("2. 线程阻塞演示");
        System.out.println("3. 性能分析演示");
        System.out.println("4. 全部运行（推荐用于 JProfiler 监测）");
        System.out.println("默认 10 秒后自动开始全部演示，也可传入参数 1/2/3/4");
        System.out.println();

        int choice = parseChoice(args);
        System.out.println("即将执行场景: " + choice);
        Thread.sleep(3000);

        switch (choice) {
            case 1 -> MemoryLeakDemo.run();
            case 2 -> ThreadBlockDemo.run();
            case 3 -> PerformanceDemo.run();
            default -> runAll();
        }

        System.out.println("演示结束。请在 JProfiler 中查看内存、线程与 CPU 热点视图。");
        Thread.sleep(5000);
    }

    private static void runAll() throws Exception {
        System.out.println("\n--- 阶段1: 内存泄漏 ---");
        MemoryLeakDemo.run();
        Thread.sleep(2000);

        System.out.println("\n--- 阶段2: 线程阻塞 ---");
        ThreadBlockDemo.run();
        Thread.sleep(2000);

        System.out.println("\n--- 阶段3: 性能热点 ---");
        PerformanceDemo.run();
    }

    private static int parseChoice(String[] args) {
        if (args.length == 0) {
            return 4;
        }
        try {
            int value = Integer.parseInt(args[0]);
            if (value >= 1 && value <= 4) {
                return value;
            }
        } catch (NumberFormatException ignored) {
            // 使用默认场景
        }
        return 4;
    }
}
