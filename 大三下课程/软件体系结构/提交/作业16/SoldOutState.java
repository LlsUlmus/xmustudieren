public class SoldOutState implements State {

	private final GumballMachine gumballMachine;

	public SoldOutState(GumballMachine gumballMachine) {
		this.gumballMachine = gumballMachine;
	}

	@Override
	public String insertQuarter() {
		return "机器已售罄，无法投币";
	}

	@Override
	public String ejectQuarter() {
		return "尚未投币，无法退币";
	}

	@Override
	public String turnCrank() {
		return "没有口香糖，转柄无效";
	}

	@Override
	public String dispense() {
		return "没有口香糖可出";
	}

	@Override
	public String refill() {
		gumballMachine.setState(gumballMachine.getNoQuarterState());
		return "补货完成，机器恢复待机";
	}

	@Override
	public String toString() {
		return "售罄";
	}
}
