package fs;

/**
 * 组合模式中的 Component：对文件与目录一视同仁。
 */
public abstract class Entry {

    public abstract String getName();

    public abstract int getSize();

    /**
     * 默认实现：叶子（文件）不支持添加子项。
     */
    public Entry add(Entry child) {
        throw new FileTreatmentException(
                "无法在文件 \"" + getName() + "\" 中添加: " + child.getName());
    }

    public void printList() {
        printList("");
    }

    protected abstract void printList(String prefix);

    @Override
    public String toString() {
        return getName() + " (" + formatSize(getSize()) + ")";
    }

    public static String formatSize(long bytes) {
        if (bytes < 1024) {
            return bytes + " B";
        }
        if (bytes < 1024 * 1024) {
            return String.format("%.1f KB", bytes / 1024.0);
        }
        if (bytes < 1024L * 1024 * 1024) {
            return String.format("%.1f MB", bytes / (1024.0 * 1024));
        }
        return String.format("%.2f GB", bytes / (1024.0 * 1024 * 1024));
    }
}
