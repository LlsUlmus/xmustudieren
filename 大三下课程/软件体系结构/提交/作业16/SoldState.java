public class SoldState implements State {

	private final GumballMachine gumballMachine;

	public SoldState(GumballMachine gumballMachine) {
		this.gumballMachine = gumballMachine;
	}

	@Override
	public String insertQuarter() {
		return "正在出货，请稍候再投币";
	}

	@Override
	public String ejectQuarter() {
		return "已转柄，无法退币";
	}

	@Override
	public String turnCrank() {
		return "不能连续转两次曲柄";
	}

	@Override
	public String dispense() {
		String ballMsg = gumballMachine.releaseBall();
		if (gumballMachine.getCount() > 0) {
			gumballMachine.setState(gumballMachine.getNoQuarterState());
			return ballMsg;
		}
		gumballMachine.setState(gumballMachine.getSoldOutState());
		return ballMsg + "\n糟糕，口香糖已售罄！";
	}

	@Override
	public String refill() {
		return "";
	}

	@Override
	public String toString() {
		return "正在出货";
	}
}
