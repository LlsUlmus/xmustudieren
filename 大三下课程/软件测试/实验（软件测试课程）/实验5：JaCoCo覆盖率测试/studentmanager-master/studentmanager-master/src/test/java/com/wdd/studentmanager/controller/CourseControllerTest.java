package com.wdd.studentmanager.controller;

import com.wdd.studentmanager.domain.Course;
import com.wdd.studentmanager.service.CourseService;
import com.wdd.studentmanager.util.AjaxResult;
import com.wdd.studentmanager.util.Data;
import com.wdd.studentmanager.util.PageBean;
import org.junit.Before;
import org.junit.Test;

import java.lang.reflect.Field;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.Map;

import static org.junit.Assert.*;

/**
 * CourseController 单元测试类
 * 覆盖课程管理模块的增删改查功能
 */
public class CourseControllerTest {

    private CourseController courseController;
    private StubCourseService courseService;

    private Course sampleCourse;
    private PageBean<Course> pageBean;

    @Before
    public void setUp() throws Exception {
        courseController = new CourseController();
        courseService = new StubCourseService();
        injectService(courseController, courseService);

        sampleCourse = new Course();
        sampleCourse.setId(1);
        sampleCourse.setName("软件测试");
        sampleCourse.setTeacherId(1001);
        sampleCourse.setCourseDate("周一 1-2节");
        sampleCourse.setInfo("JaCoCo覆盖率实验课程");

        pageBean = new PageBean<>(1, 10);
        pageBean.setTotalsize(1);
        pageBean.setDatas(Collections.singletonList(sampleCourse));
    }

    private void injectService(CourseController controller, CourseService service) throws Exception {
        Field field = CourseController.class.getDeclaredField("courseService");
        field.setAccessible(true);
        field.set(controller, service);
    }

    /**
     * 测试用例1：访问课程列表页面
     */
    @Test
    public void testCourseList() {
        String viewName = courseController.courseList();
        assertEquals("course/courseList", viewName);
    }

    /**
     * 测试用例2：分页查询课程列表（默认参数）
     */
    @Test
    public void testGetCourseListDefault() {
        courseService.setQueryPageResult(pageBean);

        @SuppressWarnings("unchecked")
        Map<String, Object> result = (Map<String, Object>) courseController.getClazzList(1, 100, null, "0", null);

        assertEquals(1, result.get("total"));
        assertEquals(pageBean.getDatas(), result.get("rows"));
        assertTrue(courseService.isQueryPageCalled());
    }

    /**
     * 测试用例3：按课程名称和教师ID查询
     */
    @Test
    public void testGetCourseListWithFilters() {
        courseService.setQueryPageResult(pageBean);

        @SuppressWarnings("unchecked")
        Map<String, Object> result = (Map<String, Object>) courseController.getClazzList(1, 10, "软件测试", "1001", null);

        assertNotNull(result.get("rows"));
        assertTrue(courseService.isQueryPageCalled());
    }

    /**
     * 测试用例4：下拉框模式返回课程数据
     */
    @Test
    public void testGetCourseListForCombox() {
        courseService.setQueryPageResult(pageBean);

        Object result = courseController.getClazzList(1, 100, null, "0", "combox");

        assertTrue(result instanceof List);
        assertEquals(1, ((List<?>) result).size());
    }

    /**
     * 测试用例5：添加课程成功
     */
    @Test
    public void testAddCourseSuccess() {
        courseService.setAddCourseResult(1);

        AjaxResult result = courseController.addCourse(sampleCourse);

        assertTrue(result.isSuccess());
        assertEquals("添加成功", result.getMessage());
    }

    /**
     * 测试用例6：添加课程失败
     */
    @Test
    public void testAddCourseFailure() {
        courseService.setAddCourseResult(0);

        AjaxResult result = courseController.addCourse(sampleCourse);

        assertFalse(result.isSuccess());
        assertEquals("添加失败", result.getMessage());
    }

    /**
     * 测试用例7：添加课程异常
     */
    @Test
    public void testAddCourseException() {
        courseService.setAddCourseException(new RuntimeException("数据库异常"));

        AjaxResult result = courseController.addCourse(sampleCourse);

        assertFalse(result.isSuccess());
        assertEquals("添加失败", result.getMessage());
    }

    /**
     * 测试用例8：修改课程成功
     */
    @Test
    public void testEditCourseSuccess() {
        courseService.setEditCourseResult(1);

        AjaxResult result = courseController.editCourse(sampleCourse);

        assertTrue(result.isSuccess());
        assertEquals("修改成功", result.getMessage());
    }

    /**
     * 测试用例9：修改课程失败
     */
    @Test
    public void testEditCourseFailure() {
        courseService.setEditCourseResult(0);

        AjaxResult result = courseController.editCourse(sampleCourse);

        assertFalse(result.isSuccess());
        assertEquals("修改失败", result.getMessage());
    }

    /**
     * 测试用例10：修改课程异常
     */
    @Test
    public void testEditCourseException() {
        courseService.setEditCourseException(new RuntimeException("数据库异常"));

        AjaxResult result = courseController.editCourse(sampleCourse);

        assertFalse(result.isSuccess());
        assertEquals("修改失败", result.getMessage());
    }

    /**
     * 测试用例11：删除课程成功
     */
    @Test
    public void testDeleteCourseSuccess() {
        Data data = new Data();
        data.setIds(Arrays.asList(1, 2));
        courseService.setDeleteCourseResult(2);

        AjaxResult result = courseController.deleteCourse(data);

        assertTrue(result.isSuccess());
        assertEquals("删除成功", result.getMessage());
    }

    /**
     * 测试用例12：删除课程失败
     */
    @Test
    public void testDeleteCourseFailure() {
        Data data = new Data();
        data.setIds(Collections.singletonList(1));
        courseService.setDeleteCourseResult(0);

        AjaxResult result = courseController.deleteCourse(data);

        assertFalse(result.isSuccess());
        assertEquals("删除失败", result.getMessage());
    }

    /**
     * 测试用例13：删除课程异常（存在关联数据）
     */
    @Test
    public void testDeleteCourseException() {
        Data data = new Data();
        data.setIds(Collections.singletonList(1));
        courseService.setDeleteCourseException(new RuntimeException("外键约束"));

        AjaxResult result = courseController.deleteCourse(data);

        assertFalse(result.isSuccess());
        assertEquals("删除失败,该班级存在老师或学生", result.getMessage());
    }

    /**
     * CourseService 桩实现，用于隔离 Controller 层测试
     */
    private static class StubCourseService implements CourseService {
        private PageBean<Course> queryPageResult;
        private int addCourseResult;
        private int editCourseResult;
        private int deleteCourseResult;
        private RuntimeException addCourseException;
        private RuntimeException editCourseException;
        private RuntimeException deleteCourseException;
        private boolean queryPageCalled;

        void setQueryPageResult(PageBean<Course> queryPageResult) {
            this.queryPageResult = queryPageResult;
        }

        void setAddCourseResult(int addCourseResult) {
            this.addCourseResult = addCourseResult;
        }

        void setEditCourseResult(int editCourseResult) {
            this.editCourseResult = editCourseResult;
        }

        void setDeleteCourseResult(int deleteCourseResult) {
            this.deleteCourseResult = deleteCourseResult;
        }

        void setAddCourseException(RuntimeException addCourseException) {
            this.addCourseException = addCourseException;
        }

        void setEditCourseException(RuntimeException editCourseException) {
            this.editCourseException = editCourseException;
        }

        void setDeleteCourseException(RuntimeException deleteCourseException) {
            this.deleteCourseException = deleteCourseException;
        }

        boolean isQueryPageCalled() {
            return queryPageCalled;
        }

        @Override
        public PageBean<Course> queryPage(Map<String, Object> paramMap) {
            queryPageCalled = true;
            return queryPageResult;
        }

        @Override
        public int addCourse(Course course) {
            if (addCourseException != null) {
                throw addCourseException;
            }
            return addCourseResult;
        }

        @Override
        public int editCourse(Course course) {
            if (editCourseException != null) {
                throw editCourseException;
            }
            return editCourseResult;
        }

        @Override
        public int deleteCourse(List<Integer> ids) {
            if (deleteCourseException != null) {
                throw deleteCourseException;
            }
            return deleteCourseResult;
        }

        @Override
        public List<Course> getCourseById(List<Integer> ids) {
            return Collections.emptyList();
        }

        @Override
        public int findByName(String name) {
            return 0;
        }
    }
}
