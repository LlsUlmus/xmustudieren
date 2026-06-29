package PathTestExam;

import junit.framework.TestCase;

public class StateTest extends TestCase {
	private int A,B,X,R;
	private BookExam be;
	

	protected void setUp() throws Exception {
		super.setUp();
		A=2;B=0;X=3;R=2;
		be=new BookExam();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testComputeX() {
		assertEquals(R,be.ComputeX(A, B, X));
	}

}
