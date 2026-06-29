public class TwilioSms implements SmsClient {
    @Override
    public void sendSms(String content) {
        System.out.println("[Twilio] sms: " + content);
    }
}
