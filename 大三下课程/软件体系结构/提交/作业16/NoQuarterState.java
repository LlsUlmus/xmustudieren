public class NoQuarterState implements State {

	private final GumballMachine gumballMachine;

	public NoQuarterState(GumballMachine gumballMachine) {
		this.gumballMachine = gumballMachine;
	}

	@Override
	public String insertQuarter() {
		gumballMachine.setState(gumballMachine.getHasQuarterState());
		return "已投入一枚硬币";
	}

	@Override
	public String ejectQuarter() {
		return "尚未投入硬币，无法退币";
	}

	@Override
	public String turnCrank() {
		return "请先投入硬币再转曲柄";
	}

	@Override
	public String dispense() {
		return "请先付款";
	}

	@Override
	public String refill() {
		return "";
	}

	@Override
	public String toString() {
		return "等待投币";
	}
}
