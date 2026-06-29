package PathTestExam;

import junit.framework.TestCase;

/** 分支-条件覆盖：同时满足分支覆盖与条件覆盖 */
public class BranchConditionTest extends TestCase {
	private int A1, B1, X1, R1;
	private int A2, B2, X2, R2;
	private int A3, B3, X3, R3;
	private int A4, B4, X4, R4;
	private BookExam be;

	protected void setUp() throws Exception {
		super.setUp();
		A1 = 2;
		B1 = 0;
		X1 = 3;
		R1 = 2;
		A2 = 1;
		B2 = 0;
		X2 = 1;
		R2 = 1;
		A3 = 2;
		B3 = 1;
		X3 = 1;
		R3 = 2;
		A4 = 3;
		B4 = 0;
		X4 = 1;
		R4 = 0;
		be = new BookExam();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testBranchConditionCoverage1() {
		assertEquals(R1, be.ComputeX(A1, B1, X1));
		assertEquals(R2, be.ComputeX(A2, B2, X2));
	}

	public void testBranchConditionCoverage2() {
		assertEquals(R3, be.ComputeX(A3, B3, X3));
		assertEquals(R4, be.ComputeX(A4, B4, X4));
	}
}
