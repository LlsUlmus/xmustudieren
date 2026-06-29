package com.lab.jprofiler;

import java.util.ArrayList;
import java.util.List;

/**
 * 模拟内存泄漏：静态集合持续持有大对象，且不会被释放。
 * 在 JProfiler 的 Live Memory / Memory Views 中可看到 byte[]、LeakItem 实例数持续上升。
 */
public class MemoryLeakDemo {

    /** 泄漏容器：对象只增不减，模拟“忘记清理”的缓存 */
    private static final List<LeakItem> LEAK_CACHE = new ArrayList<>();

    /** 每轮添加的对象数（总量 ≈ 轮数 × 本值 × PAYLOAD_SIZE_KB） */
    private static final int ITEMS_PER_ROUND = 200;
    private static final int ROUNDS = 35;
    /** 每个对象 payload 大小（KB），过大易 OOM，过小不易在 Profiler 中观察 */
    private static final int PAYLOAD_SIZE_KB = 32;

    public static void run() throws InterruptedException {
        System.out.println("开始制造内存泄漏，观察堆内存与 LeakItem 数量...");
        System.out.printf("预计泄漏约 %d MB（%d 轮 × %d 个 × %d KB）%n",
                ROUNDS * ITEMS_PER_ROUND * PAYLOAD_SIZE_KB / 1024,
                ROUNDS, ITEMS_PER_ROUND, PAYLOAD_SIZE_KB);
        for (int round = 0; round < ROUNDS; round++) {
            for (int i = 0; i < ITEMS_PER_ROUND; i++) {
                LEAK_CACHE.add(new LeakItem("item-" + round + "-" + i));
            }
            System.out.printf("第 %d 轮：LeakItem 总数 = %d%n", round + 1, LEAK_CACHE.size());
            Thread.sleep(200);
        }
        System.out.println("内存泄漏演示完成。LeakItem 仍被 static 集合引用，GC 无法回收。");
    }

    static class LeakItem {
        private final String name;
        /** 每个对象 payload，默认 32KB，累计后堆仍明显上升且不易 OOM */
        private final byte[] payload = new byte[PAYLOAD_SIZE_KB * 1024];

        LeakItem(String name) {
            this.name = name;
        }

        public String getName() {
            return name;
        }
    }
}
