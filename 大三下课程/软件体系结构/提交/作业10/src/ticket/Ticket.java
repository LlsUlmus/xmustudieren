package ticket;

import framework.Product;

public class Ticket extends Product {
    private final String holder;

    public Ticket(String holder) {
        this.holder = holder;
        System.out.println("Print ticket for " + holder + ".");
    }

    @Override
    public void use() {
        System.out.println("Check in: " + holder + ".");
    }

    public String getHolder() {
        return holder;
    }
}
