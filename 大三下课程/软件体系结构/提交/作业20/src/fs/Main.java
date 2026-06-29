package fs;


public class Main {

    public static void main(String[] args) {
        Directory root = new Directory("root");

        Directory bin = new Directory("bin");
        Directory tmp = new Directory("tmp");
        Directory usr = new Directory("usr");
        root.add(bin).add(tmp).add(usr);

        bin.add(new FsFile("vi", 10_000));
        bin.add(new FsFile("latex", 20_000));

        System.out.println("=== 第一次 printList ===");
        root.printList();

        Directory lee = new Directory("Lee");
        Directory wang = new Directory("Wang");
        Directory liu = new Directory("Liu");
        usr.add(lee).add(wang).add(liu);

        lee.add(new FsFile("diary.html", 100));
        wang.add(new FsFile("Composite.java", 200));
        liu.add(new FsFile("memo.tex", 300));

        System.out.println();
        System.out.println("=== 第二次 printList ===");
        root.printList();

        System.out.println();
        System.out.println("root 递归总大小: " + root.getSizeLong() + " 字节");

        try {
            new FsFile("test.txt", 1).add(new FsFile("x", 1));
        } catch (FileTreatmentException ex) {
            System.out.println("预期异常: " + ex.getMessage());
        }
    }
}
