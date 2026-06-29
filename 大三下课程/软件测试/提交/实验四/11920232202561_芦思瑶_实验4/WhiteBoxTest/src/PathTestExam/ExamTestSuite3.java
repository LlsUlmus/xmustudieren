package PathTestExam;

import junit.framework.Test;
import junit.framework.TestSuite;

/** 测试套装：统一执行各类白盒覆盖测试 */
public class ExamTestSuite3 {

	public static Test suite() {
		TestSuite suite = new TestSuite(ExamTestSuite3.class.getName());
		//$JUnit-BEGIN$
		suite.addTestSuite(StateTest.class);
		suite.addTestSuite(BranchTest.class);
		suite.addTestSuite(ConditionTest.class);
		suite.addTestSuite(BranchConditionTest.class);
		suite.addTestSuite(PathTest.class);
		//$JUnit-END$
		return suite;
	}
}
