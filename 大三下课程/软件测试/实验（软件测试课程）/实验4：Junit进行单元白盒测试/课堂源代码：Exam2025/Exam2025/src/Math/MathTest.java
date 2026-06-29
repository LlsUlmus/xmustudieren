package Math;

import static org.junit.Assert.*;
import org.junit.Ignore;

import org.junit.After;
import org.junit.AfterClass;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.Test;

/*
 @Before：初始化方法
@After：释放资源
@Test：测试方法，在这里可以测试期望异常和超时时间
@Ignore：忽略的测试方法
@BeforeClass：针对所有测试，只执行一次，且必须为static void
@AfterClass：针对所有测试，只执行一次，且必须为static void
一个JUnit 4 的单元测试用例执行顺序为：
@BeforeClass –> @Before –> @Test –> @After –> @AfterClass
每一个测试方法的调用顺序为：
@Before –> @Test –> @After
 */

//@Ignore //使用这个，就会导致整个测试类都不会被运行
public class MathTest {
	private static int x;
	private static int y;
	private static int z;
	//private  int x,y,z;
    @BeforeClass
    public static void beforeClass(){
        System.out.println("beforeClass");
        x=9;y=3;z=10;
    }

	@Before
	public void setUp() throws Exception {
		System.out.println("before");
		// x=9;y=3;z=10;
	}

	@After
	public void tearDown() throws Exception {
		System.out.println("after");
	}
	
    @AfterClass
    public static void afterClass(){
        System.out.println("afterClass");
    }
	
	@Test(expected=ArithmeticException.class)
	public void testDivide()  {
		System.out.println("测试方法：testDivide");
		assertEquals(y,Math.divide(x,y));   
		 assertEquals(y,Math.divide(z,y));   
		 Math.divide(z,0); //除数不能为0，会抛出异常  
		//throw new IllegalAccessException ();  
		 
	}

	@Test
	public void testMultiple() {
		assertEquals(6,Math.multiple(3, 2));
		System.out.println("测试方法：testMultiple");
	}
	
	//超时停止
	@Test(timeout=1000) 
	public void doLongTimeTask() {   
		System.out.println("测试方法：doLongTimeTask");
	   double d = 0;   
	   for(int i=1; i<10000000; i++)   
	      d+=i;   
	}  
	
	@Ignore
	@Test 
	public void testIgnore(){
		System.out.println("这个测试方法被忽略了");
	}
}

/*
@Test(timeout =1000)注解：设置当前测试方法在一定的时间内运行完，否则返回错误；
@Test（expected = Exception.class）注解：设置被测的方法是否有异常抛出，抛出的异常类型为：Exception.class；
eg: @Test（expected = NullPointException.class）
@ignore注解：注释掉一个测试方法或一个类，被注释的方法或类，不会被执行；
@RunWith(Suite.class):执行suite
@RunWith(Suite.class)
@Suite.SuiteClasses({JunitTest.class,TestClassDemo.class})
public class AllTestClass {
//此类的作用是整合测试也称 打包测试;可以把之前所有的写好的test class类进行集成;
//如需测试多个类时，只需要把相关的测试类加入到”{}”即可;如果不是同一个包类的class记得加上package名称。
//@Suite.SuiteClasses({JunitTest.class,TestClassDemo.class})
} 

*/
//详细注解见：https://blog.csdn.net/lp_cq242/article/details/80557731