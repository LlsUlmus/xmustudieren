public class GumballMachine {

	private final State soldOutState;
	private final State noQuarterState;
	private final State hasQuarterState;
	private final State soldState;

	private State state;
	private int count;

	public GumballMachine(int numberGumballs) {
		soldOutState = new SoldOutState(this);
		noQuarterState = new NoQuarterState(this);
		hasQuarterState = new HasQuarterState(this);
		soldState = new SoldState(this);

		count = numberGumballs;
		state = numberGumballs > 0 ? noQuarterState : soldOutState;
	}

	public String insertQuarter() {
		return state.insertQuarter();
	}

	public String ejectQuarter() {
		return state.ejectQuarter();
	}

	public String turnCrank() {
		String crankMsg = state.turnCrank();
		String dispenseMsg = state.dispense();
		if (dispenseMsg == null || dispenseMsg.isEmpty()) {
			return crankMsg;
		}
		if (crankMsg == null || crankMsg.isEmpty()) {
			return dispenseMsg;
		}
		return crankMsg + "\n" + dispenseMsg;
	}

	String releaseBall() {
		if (count > 0) {
			count--;
		}
		return "一颗口香糖从出口滚出！";
	}

	int getCount() {
		return count;
	}

	public String refill(int numGumballs) {
		count += numGumballs;
		String stateMsg = state.refill();
		String base = "已补充 " + numGumballs + " 颗，当前库存：" + count;
		if (stateMsg == null || stateMsg.isEmpty()) {
			return base;
		}
		return base + "\n" + stateMsg;
	}

	void setState(State state) {
		this.state = state;
	}

	State getState() {
		return state;
	}

	State getSoldOutState() {
		return soldOutState;
	}

	State getNoQuarterState() {
		return noQuarterState;
	}

	State getHasQuarterState() {
		return hasQuarterState;
	}

	State getSoldState() {
		return soldState;
	}

	@Override
	public String toString() {
		return "Mighty Gumball 口香糖机 #2004\n"
				+ "库存：" + count + " 颗\n"
				+ "状态：" + state;
	}
}
