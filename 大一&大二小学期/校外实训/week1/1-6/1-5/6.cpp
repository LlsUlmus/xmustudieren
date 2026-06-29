//汉诺塔 题解备选
//#include <iostream>
//using namespace std;
//
//void move(int n, char A, char B, char C);
//int step;
//int main()
//{
//    int n;
//    cin >> n;
//    move(n, 'A', 'B', 'C');
//    return 0;
//}
//void move(int n, char A, char B, char C)
//{
//    if (n == 1)
//    {
//        step++;
//        cout << A << "->" << C << endl;
//    }
//    else
//    {
//        move(n - 1, A, C, B);
//        cout << A << "->" << C << endl;
//        step++;
//        move(n - 1, B, A, C);
//    }
//
//}