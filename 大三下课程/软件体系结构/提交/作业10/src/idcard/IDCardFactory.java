package idcard;

import framework.Factory;
import framework.Product;

import java.util.Vector;

public class IDCardFactory extends Factory {
    private final Vector owners = new Vector();

    @Override
    protected Product createProduct(String owner) {
        return new IDCard(owner);
    }

    @Override
    protected void registerProduct(Product product) {
        IDCard card = (IDCard) product;
        owners.add(card.getOwner());
    }

    @Override
    public Vector getOwners() {
        return owners;
    }
}
