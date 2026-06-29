using System.Text;

namespace LINQDrill;

internal class Program
{
    
    static void Main(string[] args)
    {
        Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance); // 这是为了使用GBK来统计字符串长度
        // 
        // Create a data source by using a collection initializer.
        List<Student> students = new List<Student>
        {
            new Student {First="Svetlana", Last="Omelchenko", ID=111, Scores= new List<int> {97, 92, 81, 60}},
            new Student {First="Claire", Last="O'Donnell", ID=112, Scores= new List<int> {75, 84, 91, 39}},
            new Student {First="Sven", Last="Mortensen", ID=113, Scores= new List<int> {88, 94, 65, 91}},
            new Student {First="Cesar", Last="Garcia", ID=114, Scores= new List<int> {97, 89, 85, 82}},
            new Student {First="Debra", Last="Garcia", ID=115, Scores= new List<int> {35, 72, 91, 70}},
            new Student {First="Fadi", Last="Fakhouri", ID=116, Scores= new List<int> {99, 86, 90, 94}},
            new Student {First="Hanying", Last="Feng", ID=117, Scores= new List<int> {93, 92, 80, 87}},
            new Student {First="Hugo", Last="Garcia", ID=118, Scores= new List<int> {92, 90, 83, 78}},
            new Student {First="Lance", Last="Tucker", ID=119, Scores= new List<int> {68, 79, 88, 92}},
            new Student {First="Terry", Last="Adams", ID=120, Scores= new List<int> {99, 82, 81, 79}},
            new Student {First="Eugene", Last="Zabokritski", ID=121, Scores= new List<int> {96, 85, 91, 60}},
            new Student {First="Michael", Last="Tucker", ID=122, Scores= new List<int> {94, 92, 91, 91}}
        };

        //查询第1次测试分数不小于90的所有学生，并格式输出ID，Last，First，第1次测试分数等信息
        
        
/* 
 * Output:
ID        | 姓氏               |名字                |   第1次测试分数
========================================================================
111       |Omelchenko          |Svetlana            |        97
114       |Garcia              |Cesar               |        97
116       |Fakhouri            |Fadi                |        99
117       |Feng                |Hanying             |        93
118       |Garcia              |Hugo                |        92
120       |Adams               |Terry               |        99
121       |Zabokritski         |Eugene              |        96
122       |Tucker              |Michael             |        94
========================================================================
*/


        //查询第1次测试分数大于90 并且 所有测试都高于80 的所有学生，并格式输出ID，Last，First，第1次测试分数等信息
        

        /*Output
         * 
ID        |姓氏                |名字                |   第1次测试分数
========================================================================
114       |Garcia              |Cesar               |        97
116       |Fakhouri            |Fadi                |        99
122       |Tucker              |Michael             |        94
========================================================================
         * 
         */



        //根据姓氏首字母来分组所有学生，并格式输出ID，Last，First
        

        /*Output:
         * 
         * 
         */

        //根据姓氏首字母来分组所有学生，需要按首字母升序排列，并格式输出ID，Last，First
        

        //let 的使用
        //取得每位学生的平均成绩，进而获取所有学生的平均成绩
        //输出所有学生的平均成绩，小数点后保留一位

        
        /*输出样例：所有学生的平均成绩：88.8
         * 
         * 
         */


        //计算班级学生四次测试成绩和的平均值
        
        /*输出样例：班级学生四次测试成绩和的平均值：222.2
        * 
        * 
        */


        // 查询那些第4次测试成绩不低于自己的平均成绩且不低于所有人的平均成绩的学生，返回由ID、Last、First、自己的平均成绩、第4次测试成绩 组成的字符串的列表
        // 注意：studentQuery5 is an IEnumerable<string>

        /*输出：
         * 
         */

        //查询所有的学生，其总分大于平均总分，按总分的降序输出学生学号和总分的列表
        
        // Output:
        // Student ID: 113, Score: 338
        // Student ID: 114, Score: 353
        // Student ID: 116, Score: 369
        // Student ID: 117, Score: 352
        // Student ID: 118, Score: 343
        // Student ID: 120, Score: 341
        // Student ID: 122, Score: 368
    }
}