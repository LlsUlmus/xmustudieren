package hw11;

/**
 * 实验时段：实现 Cloneable，默认 clone() 为浅拷贝（与 Object.clone 语义一致）。
 */
public class ExperimentSlot implements Cloneable {
    private String topic;
    private EquipmentTag tag;

    public ExperimentSlot(String topic, EquipmentTag tag) {
        this.topic = topic;
        this.tag = tag;
    }

    public String getTopic() {
        return topic;
    }

    public void setTopic(String topic) {
        this.topic = topic;
    }

    public EquipmentTag getTag() {
        return tag;
    }

    public void setTag(EquipmentTag tag) {
        this.tag = tag;
    }

    /**
     * 浅拷贝：新外壳对象，但 tag 引用仍指向同一块内存。
     */
    @Override
    public ExperimentSlot clone() {
        try {
            return (ExperimentSlot) super.clone();
        } catch (CloneNotSupportedException e) {
            throw new AssertionError(e);
        }
    }

    /**
     * 深拷贝：手写逐层复制，不依赖序列化。
     */
    public ExperimentSlot deepCopy() {
        EquipmentTag tagCopy = tag == null ? null : tag.clone();
        return new ExperimentSlot(topic, tagCopy);
    }
}
