public class OutlookMail implements EmailClient {
    @Override
    public void sendMail(String text) {
        System.out.println("[Outlook] mail: " + text);
    }
}
