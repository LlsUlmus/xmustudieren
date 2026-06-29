package blackbox;

/**
 * 黑盒测试自动化执行器 — 根据测试方案批量运行用例并输出结果
 */
public class BlackBoxTestRunner {

    static class TestCase {
        int id;
        String category;
        String description;
        int year, month, day;
        String expected;

        TestCase(int id, String category, String description,
                 int year, int month, int day, String expected) {
            this.id = id;
            this.category = category;
            this.description = description;
            this.year = year;
            this.month = month;
            this.day = day;
            this.expected = expected;
        }
    }

    static String run(int year, int month, int day) {
        inquireDate d = new inquireDate();
        d.year = year;
        d.month = month;
        d.day = day;

        if (year < 1900 || year > 2050) {
            return "年份不符合要求！";
        }
        if (!d.isLegal()) {
            return "该日期不存在！";
        }
        d.result();
        return d.year + "年" + d.month + "月" + d.day + "日";
    }

    public static void main(String[] args) {
        TestCase[] cases = {
            // ===== 等价类划分 =====
            new TestCase(1, "等价类-年份有效", "有效年份中间值", 2021, 7, 22, "2021年7月24日"),
            new TestCase(2, "等价类-年份无效(小)", "年份小于1900", 1899, 6, 15, "年份不符合要求！"),
            new TestCase(3, "等价类-年份无效(大)", "年份大于2050", 2051, 6, 15, "年份不符合要求！"),
            new TestCase(4, "等价类-大月有效日", "31天月份有效日期", 2020, 1, 15, "2020年1月17日"),
            new TestCase(5, "等价类-小月有效日", "30天月份有效日期", 2020, 4, 15, "2020年4月17日"),
            new TestCase(6, "等价类-闰年2月有效", "闰年2月29日", 2020, 2, 29, "2020年3月2日"),
            new TestCase(7, "等价类-平年2月有效", "平年2月28日", 2021, 2, 28, "2021年3月2日"),
            new TestCase(8, "等价类-非法日期", "4月31日不存在", 2021, 4, 31, "该日期不存在！"),
            new TestCase(9, "等价类-非法月份", "月份为0", 2021, 0, 15, "该日期不存在！"),
            new TestCase(10, "等价类-非法月份", "月份为13", 2021, 13, 15, "该日期不存在！"),
            new TestCase(11, "等价类-平年2月非法", "平年2月29日", 2021, 2, 29, "该日期不存在！"),
            new TestCase(12, "等价类-闰年判定", "能被400整除的闰年", 2000, 2, 29, "2000年3月2日"),
            new TestCase(13, "等价类-非闰年", "能被100整除但不能被400整除", 1900, 2, 28, "1900年3月2日"),

            // ===== 边界值分析 =====
            new TestCase(14, "边界值-年份下界", "year=1900", 1900, 1, 1, "1900年1月3日"),
            new TestCase(15, "边界值-年份下界-1", "year=1899", 1899, 1, 1, "年份不符合要求！"),
            new TestCase(16, "边界值-年份上界", "year=2050", 2050, 12, 31, "2051年1月2日"),
            new TestCase(17, "边界值-年份上界+1", "year=2051", 2051, 1, 1, "年份不符合要求！"),
            new TestCase(18, "边界值-月份下界", "month=1", 2021, 1, 1, "2021年1月3日"),
            new TestCase(19, "边界值-月份上界", "month=12", 2021, 12, 1, "2021年12月3日"),
            new TestCase(20, "边界值-大月day=31", "1月31日+2", 2021, 1, 31, "2021年2月2日"),
            new TestCase(21, "边界值-大月day=30", "1月30日+2跨月", 2021, 1, 30, "2021年2月1日"),
            new TestCase(22, "边界值-小月day=30", "4月30日+2跨月", 2021, 4, 30, "2021年5月2日"),
            new TestCase(23, "边界值-小月day=29", "4月29日+2跨月", 2021, 4, 29, "2021年5月1日"),
            new TestCase(24, "边界值-小月非法day=31", "4月31日", 2021, 4, 31, "该日期不存在！"),
            new TestCase(25, "边界值-闰年2月day=29", "闰年2月29+2", 2020, 2, 29, "2020年3月2日"),
            new TestCase(26, "边界值-闰年2月day=28", "闰年2月28+2跨月", 2020, 2, 28, "2020年3月1日"),
            new TestCase(27, "边界值-平年2月day=28", "平年2月28+2跨月", 2021, 2, 28, "2021年3月2日"),
            new TestCase(28, "边界值-平年2月day=27", "平年2月27+2跨月", 2021, 2, 27, "2021年3月1日"),
            new TestCase(29, "边界值-day=0", "日期为0", 2021, 5, 0, "该日期不存在！"),
            new TestCase(30, "边界值-跨年12月31", "12月31日+2跨年", 2021, 12, 31, "2022年1月2日"),
            new TestCase(31, "边界值-跨年12月30", "12月30日+2跨年", 2021, 12, 30, "2022年1月1日"),

            // ===== 决策表覆盖 =====
            new TestCase(32, "决策表-R1", "年份无效", 1800, 5, 10, "年份不符合要求！"),
            new TestCase(33, "决策表-R2", "年份有效+日期非法", 2021, 2, 30, "该日期不存在！"),
            new TestCase(34, "决策表-R3", "同月普通+2", 2021, 7, 22, "2021年7月24日"),
            new TestCase(35, "决策表-R4", "小月跨月(day=29)", 2021, 6, 29, "2021年7月1日"),
            new TestCase(36, "决策表-R5", "小月跨月(day=30)", 2021, 6, 30, "2021年7月2日"),
            new TestCase(37, "决策表-R6", "大月跨月(day=30)", 2021, 3, 30, "2021年4月1日"),
            new TestCase(38, "决策表-R7", "大月跨月(day=31)", 2021, 3, 31, "2021年4月2日"),
            new TestCase(39, "决策表-R8", "12月跨年(day=30)", 2020, 12, 30, "2021年1月1日"),
            new TestCase(40, "决策表-R9", "12月跨年(day=31)", 2020, 12, 31, "2021年1月2日"),
            new TestCase(41, "决策表-R10", "闰年2月跨月(day=28)", 2020, 2, 28, "2020年3月1日"),
            new TestCase(42, "决策表-R11", "闰年2月跨月(day=29)", 2020, 2, 29, "2020年3月2日"),
            new TestCase(43, "决策表-R12", "平年2月跨月(day=27)", 2021, 2, 27, "2021年3月1日"),
            new TestCase(44, "决策表-R13", "平年2月跨月(day=28)", 2021, 2, 28, "2021年3月2日"),
        };

        System.out.println("=".repeat(90));
        System.out.println("BlackBox 黑盒测试执行报告");
        System.out.println("=".repeat(90));
        System.out.printf("%-4s %-18s %-22s %-18s %-18s %-6s%n",
                "编号", "类别", "描述", "输入(年-月-日)", "期望输出", "结果");
        System.out.println("-".repeat(90));

        int pass = 0, fail = 0;
        for (TestCase tc : cases) {
            String actual = run(tc.year, tc.month, tc.day);
            boolean ok = actual.equals(tc.expected);
            if (ok) pass++;
            else fail++;

            System.out.printf("%-4d %-18s %-22s %4d-%02d-%02d       %-18s %-6s%n",
                    tc.id, tc.category, tc.description,
                    tc.year, tc.month, tc.day,
                    tc.expected, ok ? "PASS" : "FAIL");
            if (!ok) {
                System.out.printf("     >>> 实际输出: %s%n", actual);
            }
        }

        System.out.println("=".repeat(90));
        System.out.printf("总计: %d  通过: %d  失败: %d  通过率: %.1f%%%n",
                cases.length, pass, fail, 100.0 * pass / cases.length);
        System.out.println("=".repeat(90));
    }
}
