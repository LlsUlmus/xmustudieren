package PathTestExam;

import junit.framework.Test;
import junit.framework.TestSuite;

public class ExamTestSuite3 {

	public static Test suite() {
		TestSuite suite = new TestSuite(ExamTestSuite3.class.getName());
		//$JUnit-BEGIN$
		suite.addTestSuite(BranchTest.class);
		suite.addTestSuite(ConditionTest.class);
		suite.addTestSuite(PathTest.class);
		suite.addTestSuite(StateTest.class);
		//$JUnit-END$
		return suite;
	}

}
