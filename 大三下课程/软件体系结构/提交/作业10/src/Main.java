import framework.Product;
import idcard.IDCardFactory;
import ticket.TicketFactory;

public class Main {
    public static void main(String[] args) {
        IDCardFactory cardFactory = new IDCardFactory();
        Product card = cardFactory.create("Zhang");
        card.use();

        TicketFactory ticketFactory = new TicketFactory();
        Product ticket = ticketFactory.create("Li");
        ticket.use();

        System.out.println("Card owners: " + cardFactory.getOwners());
        System.out.println("Ticket holders: " + ticketFactory.getOwners());
    }
}
