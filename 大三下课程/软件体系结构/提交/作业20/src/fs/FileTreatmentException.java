package fs;

/**
 * 对文件（叶子节点）执行“添加子项”等目录专属操作时抛出。
 */
public class FileTreatmentException extends RuntimeException {

    public FileTreatmentException() {
        super("不能在文件上执行该操作。");
    }

    public FileTreatmentException(String message) {
        super(message);
    }
}
