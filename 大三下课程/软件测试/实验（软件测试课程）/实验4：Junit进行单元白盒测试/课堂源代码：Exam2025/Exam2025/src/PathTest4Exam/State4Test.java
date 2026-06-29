package PathTest4Exam;
//记得在library里面删除junit3，否则会出现the test is not API错误。

import static org.junit.Assert.*;

import org.junit.After;
import org.junit.Before;
import org.junit.Test;

import PathTestExam.BookExam;

public class State4Test {
	private int A,B,X,R;
	private BookExam be;

	@Before
	public void setUp() throws Exception {
		A=2;B=0;X=3;R=2;
		be=new BookExam();
	}

	@After
	public void tearDown() throws Exception {
	}

	@Test
	public void testComputeX() {
		assertEquals(R,be.ComputeX(A, B, X));
	}

}
