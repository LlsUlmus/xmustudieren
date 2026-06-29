package hw11;

/**
 * 运行深拷贝示例：副本与原件在引用类型字段上彼此独立。
 */
public class DeepCloneDemo {

    public static void main(String[] args) {
        EquipmentTag tag = new EquipmentTag("EQ-1001");
        ExperimentSlot original = new ExperimentSlot("光谱标定", tag);

        ExperimentSlot copy = original.deepCopy();

        copy.setTopic("荧光寿命");
        copy.getTag().setAssetCode("EQ-9999");

        System.out.println("=== 深拷贝演示（手写复制链）===");
        System.out.println("原件 topic: " + original.getTopic());
        System.out.println("原件 tag : " + original.getTag().getAssetCode());
        System.out.println("副本 topic: " + copy.getTopic());
        System.out.println("副本 tag : " + copy.getTag().getAssetCode());
        System.out.println("引用同一标签? " + (original.getTag() == copy.getTag()));
    }
}
