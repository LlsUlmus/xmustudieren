package fs;

/**
 * 组合模式中的 Leaf：表示单个文件。
 */
public class FsFile extends Entry {

    private final String name;
    private final long size;

    public FsFile(String name, long size) {
        this.name = name;
        this.size = size;
    }

    @Override
    public String getName() {
        return name;
    }

    @Override
    public int getSize() {
        if (size > Integer.MAX_VALUE) {
            return Integer.MAX_VALUE;
        }
        return (int) size;
    }

    public long getSizeLong() {
        return size;
    }

    @Override
    protected void printList(String prefix) {
        System.out.println(prefix + "/" + this);
    }

    public boolean isDirectory() {
        return false;
    }
}
