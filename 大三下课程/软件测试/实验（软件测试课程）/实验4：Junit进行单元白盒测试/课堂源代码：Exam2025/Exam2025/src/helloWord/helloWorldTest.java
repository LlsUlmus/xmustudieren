package helloWord;

import junit.framework.TestCase;

public class helloWorldTest extends TestCase {
	private int X1,R1,X2,R2,X3,R3;
	private helloWorld hw;

	protected void setUp() throws Exception {
		super.setUp();
		X1=-2; R1=4;
		X2=0; R2=1;
		X3=2; R3=3;
		hw=new helloWorld();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}

	public void testSayHello() {
		assertEquals(R1,hw.SayHello(X1) );
		assertEquals(R2,hw.SayHello(X2) );
		assertEquals(R3,hw.SayHello(X3) );
	}

}
