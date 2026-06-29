//两数之和
//#include <iostream>
//using namespace std;
//
//int main() {
//    int target, n;
//    cin >> target >> n;
//
//    int a[9999]; 
//    for (int i = 0; i < n; i++) {
//        cin >> a[i];
//    }
//
//    int left = 0, right = n - 1;
//    while (left < right) {
//        int sum = a[left] + a[right];
//        if (sum == target) {
//            cout << left << " " << right << endl;
//            return 0;
//        }
//        else if (sum < target) {
//            left--;
//        }
//        else {
//            right++;
//        }
//    }
//
//    return 0;
//}