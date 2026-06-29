package idcard;

import framework.Product;

public class IDCard extends Product {
    private final String owner;

    public IDCard(String owner) {
        this.owner = owner;
        System.out.println("Create " + owner + "'s card.");
    }

    @Override
    public void use() {
        System.out.println("Use " + owner + "'s card.");
    }

    public String getOwner() {
        return owner;
    }
}
