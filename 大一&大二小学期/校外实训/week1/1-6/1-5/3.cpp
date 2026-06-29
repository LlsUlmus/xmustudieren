//人的周期
//#include <iostream>
//using namespace std;
//
//
//int main() {
//    int p, e, i, d;
//    int caseNum = 1;
//
//    while (cin >> p >> e >> i >> d) {
//        if (p == -1 && e == -1 && i == -1 && d == -1) {
//            break;
//        }
//
//        int x = d + 1;
//        while (true) {
//            if ((x - p) % 23 == 0 &&
//                (x - e) % 28 == 0 &&
//                (x - i) % 33 == 0) {
//                break;
//            }
//            x++;
//        }
//
//        cout << "Case " << caseNum << ": the next triple peak occurs in "
//            << x - d << " days." << endl;
//        caseNum++;
//    }
//
//    return 0;
//}

