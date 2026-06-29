#include <iostream>
#include <string>
#include <cmath>
using namespace std;

const double EPS = 1e-6; 

double cal(double a, int m, double b) {
    switch (m) {
    case 1: return a + b;
    case 2: return a - b;
    case 3: return a * b;
    case 4:
        if (abs(b) < EPS) return 1e9; 
        return a / b;
    }
    return 0;
}

bool cal1(double a, double b, double c, double d, int m1, int m2, int m3) {
    double r1 = cal(a, m1, b);
    double r2 = cal(r1, m2, c);
    double r3 = cal(r2, m3, d);
    return abs(r3 - 24) < EPS;
}

bool cal2(double a, double b, double c, double d, int m1, int m2, int m3) {
    double r1 = cal(b, m1, c);
    double r2 = cal(a, m2, r1);
    double r3 = cal(r2, m3, d);
    return abs(r3 - 24) < EPS;
}

bool cal3(double a, double b, double c, double d, int m1, int m2, int m3) {
    double r1 = cal(b, m1, c);
    double r2 = cal(r1, m2, d);
    double r3 = cal(a, m3, r2);
    return abs(r3 - 24) < EPS;
}

bool cal4(double a, double b, double c, double d, int m1, int m2, int m3) {
    double r1 = cal(c, m1, d);
    double r2 = cal(b, m2, r1);
    double r3 = cal(a, m3, r2);
    return abs(r3 - 24) < EPS;
}

bool cal5(double a, double b, double c, double d, int m1, int m2, int m3) {
    double r1 = cal(a, m1, b);
    double r2 = cal(c, m3, d);
    double r3 = cal(r1, m2, r2);
    return abs(r3 - 24) < EPS;
}

bool all_cal(double a, double b, double c, double d) {
    for (int i = 1; i <= 4; i++)
        for (int j = 1; j <= 4; j++)
            for (int k = 1; k <= 4; k++)
                if (cal1(a, b, c, d, i, j, k) || cal2(a, b, c, d, i, j, k) ||
                    cal3(a, b, c, d, i, j, k) || cal4(a, b, c, d, i, j, k) ||
                    cal5(a, b, c, d, i, j, k))
                    return true;
    return false;
}

bool judge(int a, int b, int c, int d) {
    int all[24][4] = {
        {a,b,c,d},{a,b,d,c},{a,c,b,d},{a,c,d,b},{a,d,b,c},{a,d,c,b},
        {b,a,c,d},{b,a,d,c},{b,c,a,d},{b,c,d,a},{b,d,a,c},{b,d,c,a},
        {c,a,b,d},{c,a,d,b},{c,b,a,d},{c,b,d,a},{c,d,a,b},{c,d,b,a},
        {d,a,b,c},{d,a,c,b},{d,b,a,c},{d,b,c,a},{d,c,a,b},{d,c,b,a}
    };
    for (int i = 0; i < 24; i++)
        if (all_cal(all[i][0], all[i][1], all[i][2], all[i][3]))
            return true;
    return false;
}

int main() {
    int a, b, c, d;
    while (cin >> a >> b >> c >> d) {
        if (a == 0 && b == 0 && c == 0 && d == 0) break;
        if (judge(a, b, c, d))
            cout << "YES" << endl;
        else
            cout << "NO" << endl;
    }
    return 0;
}


//#include <iostream>
//#include <string>
//using namespace std;
//int mark_int[4] = { 1,2,3,4 };
//string mark_char = "+-*/";
//double cal(double a, int m, double b)
//{
//    switch (m)
//    {
//    case 1:    return a + b;
//    case 2:    return a - b;
//    case 3:    return a * b;
//    case 4:    return a / b;
//    }
//}
//
//bool cal1(double a, double b, double c, double d, int m1, int m2, int m3)
//{
//    double r1;
//    double r2;
//    double r3;
//    r1 = cal(a, m1, b);
//    r2 = cal(r1, m2, c);
//    r3 = cal(r2, m3, d);
//    if (r3 == 24)
//    {
//        cout << "Yes" << endl;
//        return 1;
//    }
//    return 0;
//}
//
//bool cal2(int a, int b, int c, int d, int m1, int m2, int m3)
//{
//    double r1;
//    double r2;
//    double r3;
//    r1 = cal(b, m1, c);
//    r2 = cal(a, m2, r1);
//    r3 = cal(r2, m3, d);
//    if (r3 == 24)
//    {
//        cout << "Yes" << endl;
//        return 1;
//    }
//    return 0;
//}
//
//bool cal3(int a, int b, int c, int d, int m1, int m2, int m3)
//{
//    double r1;
//    double r2;
//    double r3;
//    r1 = cal(b, m1, c);
//    r2 = cal(r1, m2, d);
//    r3 = cal(a, m3, r2);
//    if (r3 == 24)
//    {
//        cout << "Yes" << endl;
//        return 1;
//    }
//    return 0;
//}
//
//bool cal4(int a, int b, int c, int d, int m1, int m2, int m3)
//{
//    double r1;
//    double r2;
//    double r3;
//    r1 = cal(c, m1, d);
//    r2 = cal(b, m2, r1);
//    r3 = cal(a, m3, r2);
//    if (r3 == 24)
//    {
//        cout << "Yes" << endl;
//        return 1;
//    }
//    return 0;
//}
//
//bool cal5(int a, int b, int c, int d, int m1, int m2, int m3)
//{
//    double r1;
//    double r2;
//    double r3;
//    r1 = cal(a, m1, b);
//    r2 = cal(c, m3, d);
//    r3 = cal(r1, m2, r2);
//    if (r3 == 24)
//    {
//        cout << "Yes" << endl;
//        return 1;
//    }
//    return 0;
//}
//
//
//bool all_cal(int a, int b, int c, int d)
//{
//    for (int i = 1; i <= 4; i++)
//        for (int j = 1; j <= 4; j++)
//            for (int k = 1; k <= 4; k++)
//            {
//                if (cal1(a, b, c, d, i, j, k) == true || cal2(a, b, c, d, i, j, k) == true || cal3(a, b, c, d, i, j, k) == true || cal4(a, b, c, d, i, j, k) == true || cal5(a, b, c, d, i, j, k) == true)
//                    return 1;
//            }
//    return 0;
//}
//
//
//bool judge(int a, int b, int c, int d)
//{
//    int all[24][4] = {
//        {a,b,c,d},{a,b,d,c},{a,c,b,d},{a,c,d,b},{a,d,b,c},{a,d,c,b},
//        {b,a,c,d},{b,a,d,c},{b,c,a,d},{b,c,d,a},{b,d,a,c},{b,d,c,a},
//        {c,a,b,d},{c,a,d,b},{c,b,a,d},{c,b,d,a},{c,d,a,b},{c,d,b,a},
//        {d,a,b,d},{d,a,d,b},{d,b,a,c},{d,b,c,a},{d,c,a,b},{d,c,b,a},
//    };
//    for (int i = 0; i < 24; i++)
//    {
//        if (all_cal(all[i][0], all[i][1], all[i][2], all[i][3]))
//            return 1;
//    }
//    return 0;
//}
//
//int main()
//{
//    int a, b, c, d;
//    cin >> a >> b >> c >> d;
//    if (!judge(a, b, c, d))
//        cout << "No" << endl;
//
//}