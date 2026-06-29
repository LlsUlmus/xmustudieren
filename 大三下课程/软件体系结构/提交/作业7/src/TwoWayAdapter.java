public class TwoWayAdapter implements EmailClient, SmsClient {

    private final EmailClient email;
    private final SmsClient sms;

    public TwoWayAdapter(EmailClient emailDelegate) {
        this.email = emailDelegate;
        this.sms = null;
    }

    public TwoWayAdapter(SmsClient smsDelegate) {
        this.sms = smsDelegate;
        this.email = null;
    }

    @Override
    public void sendMail(String text) {
        if (email != null) {
            email.sendMail(text);
        } else {
            System.out.println("[adapter] sendMail -> routed to SMS");
            sms.sendSms(text);
        }
    }

    @Override
    public void sendSms(String content) {
        if (sms != null) {
            sms.sendSms(content);
        } else {
            System.out.println("[adapter] sendSms -> routed to email");
            email.sendMail(content);
        }
    }
}
