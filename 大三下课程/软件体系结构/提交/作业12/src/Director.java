/**
 * 指挥者：固定构建流程，与具体格式无关
 */
public class Director {
    private final Builder builder;

    public Director(Builder builder) {
        this.builder = builder;
    }

    public Object construct() {
        builder.makeTitle("校园一日");
        builder.makeString("清晨阳光正好，开启新的学习日程。");
        builder.makeItems(new String[]{
                "整理书包与笔记。",
                "到教室前回顾上节课要点。"
        });
        builder.makeString("午间适当放松，为下午储备精力。");
        builder.makeItems(new String[]{
                "补充水分。",
                "起身活动五分钟。"
        });
        builder.makeString("夜幕降临，感谢今天的每一份努力。");
        builder.makeItems(new String[]{
                "复盘今日收获。",
                "早点休息，明天继续加油。"
        });
        return builder.getResult();
    }
}
