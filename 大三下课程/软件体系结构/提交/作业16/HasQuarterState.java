public class HasQuarterState implements State {

	private final GumballMachine gumballMachine;

	public HasQuarterState(GumballMachine gumballMachine) {
		this.gumballMachine = gumballMachine;
	}

	@Override
	public String insertQuarter() {
		return "不能再投第二枚硬币";
	}

	@Override
	public String ejectQuarter() {
		gumballMachine.setState(gumballMachine.getNoQuarterState());
		return "硬币已退回";
	}

	@Override
	public String turnCrank() {
		gumballMachine.setState(gumballMachine.getSoldState());
		return "正在转动曲柄…";
	}

	@Override
	public String dispense() {
		return "尚未完成出货";
	}

	@Override
	public String refill() {
		return "";
	}

	@Override
	public String toString() {
		return "已投币，等待转柄";
	}
}
