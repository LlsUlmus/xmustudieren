package hw11;

/**
 * 设备标签：可变引用类型字段，用于演示浅/深拷贝对“内层对象”是否共享。
 */
public class EquipmentTag implements Cloneable {
    private String assetCode;

    public EquipmentTag(String assetCode) {
        this.assetCode = assetCode;
    }

    public String getAssetCode() {
        return assetCode;
    }

    public void setAssetCode(String assetCode) {
        this.assetCode = assetCode;
    }

    @Override
    public EquipmentTag clone() {
        try {
            return (EquipmentTag) super.clone();
        } catch (CloneNotSupportedException e) {
            throw new AssertionError(e);
        }
    }
}
