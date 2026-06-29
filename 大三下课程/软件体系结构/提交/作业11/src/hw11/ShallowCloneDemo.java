package hw11;

/**
 * 运行浅拷贝示例：修改副本中的引用字段会影响原件。
 */
public class ShallowCloneDemo {

    public static void main(String[] args) {
        EquipmentTag tag = new EquipmentTag("EQ-1001");
        ExperimentSlot original = new ExperimentSlot("光谱标定", tag);

        ExperimentSlot copy = original.clone();

        copy.setTopic("荧光寿命"); // 字符串不可变，这里替换的是副本自己的字段引用
        copy.getTag().setAssetCode("EQ-9999");

        System.out.println("=== 浅拷贝演示 ===");
        System.out.println("原件 topic: " + original.getTopic());
        System.out.println("原件 tag : " + original.getTag().getAssetCode());
        System.out.println("副本 topic: " + copy.getTopic());
        System.out.println("副本 tag : " + copy.getTag().getAssetCode());
        System.out.println("引用同一标签? " + (original.getTag() == copy.getTag()));
    }
}
