public class Main {

    public static void main(String[] args) {
        EmailClient realMail = new OutlookMail();
        SmsClient realSms = new TwilioSms();

        System.out.println("--- Delegate: email; expose both Email + SMS APIs ---");
        TwoWayAdapter fromMail = new TwoWayAdapter(realMail);
        fromMail.sendMail("Meeting rescheduled");
        fromMail.sendSms("Same as email, check inbox");

        System.out.println();
        System.out.println("--- Delegate: SMS; expose both Email + SMS APIs ---");
        TwoWayAdapter fromSms = new TwoWayAdapter(realSms);
        fromSms.sendSms("OTP 9527");
        fromSms.sendMail("OTP delivered via SMS channel");
    }
}
