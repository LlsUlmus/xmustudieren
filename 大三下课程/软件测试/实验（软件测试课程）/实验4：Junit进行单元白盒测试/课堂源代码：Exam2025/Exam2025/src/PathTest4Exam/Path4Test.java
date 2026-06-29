package PathTest4Exam;

import static org.junit.Assert.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import PathTestExam.BookExam;

public class Path4Test {
	private int A1,B1,X1,R1;
	private int A2,B2,X2,R2;
	private int A3,B3,X3,R3;
	private int A4,B4,X4,R4;
	private BookExam be;

	@Before
	public void setUp() throws Exception {
		A1=2;B1=0;X1=3;R1=2;
		A2=1;B2=0;X2=1;R2=1;
		A3=2;B3=1;X3=1;R3=2;
		A4=3;B4=0;X4=1;R4=0;
		be=new BookExam();
	}

	@After
	public void tearDown() throws Exception {
	}

	@Test
	public void testComputeX() {
		assertEquals(R1,be.ComputeX(A1, B1, X1));
		assertEquals(R2,be.ComputeX(A2, B2, X2));
		assertEquals(R3,be.ComputeX(A3, B3, X3));
		assertEquals(R4,be.ComputeX(A4, B4, X4));
	}

}
