//·ÖÆ»¹û


//#include <iostream>
//using namespace std;
//
//int set(int m, int n) {
//    if (m == 0) {
//        return 1;  
//    }
//    if (n == 0) {
//        return 0;  
//    }
//    if (n > m) {
//        return set(m, m);  
//    }
//    else {
//        return set(m, n - 1) + set(m - n, n); 
//    }
//}
//
//int main() {
//    int t, m, n;
//    cin >> t;
//    while (t--) {
//        cin >> m >> n;
//        cout << set(m, n) << endl;
//    }
//    return 0;
//}

//#include <iostream>
//using namespace std;
//int set(int m, int n) {
//    if (m == 0 || n == 1) {
//        return 1;
//    }
//    if (n > m) {
//        return set(m, m);
//    }
//    else {
//        return set(m, n - 1) + set(m - n, 1);
//    }
//}
//
//int main() {
//    int t, m, n;
//    cin >> t;
//    while (t--) {
//        cin >> m >> n;
//        cout << set(m, n) << endl;
//    }
//    return 0;
//}