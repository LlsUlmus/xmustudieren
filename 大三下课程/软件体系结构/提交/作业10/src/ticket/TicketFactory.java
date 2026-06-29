package ticket;

import framework.Factory;
import framework.Product;

import java.util.Vector;

public class TicketFactory extends Factory {
    private final Vector owners = new Vector();

    @Override
    protected Product createProduct(String owner) {
        return new Ticket(owner);
    }

    @Override
    protected void registerProduct(Product product) {
        Ticket ticket = (Ticket) product;
        owners.add(ticket.getHolder());
    }

    @Override
    public Vector getOwners() {
        return owners;
    }
}
