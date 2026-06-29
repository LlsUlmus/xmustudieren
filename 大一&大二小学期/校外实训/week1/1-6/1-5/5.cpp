//三数之和

//#include<iostream>
//#include<vector>
//using namespace std;
//
//int main()
//{
//    int target, n;
//    cin >> target >> n;
//    vector<int> a(n);
//    for (int i = 0; i < n; i++)
//        cin >> a[i];
//
//    for (int i = 0; i < n - 2; i++) {
//        if (i > 0 && a[i] == a[i - 1])
//            continue;
//
//        int left = i + 1;
//        int right = n - 1;
//
//        while (left < right) {
//            int sum = a[i] + a[left] + a[right];
//
//            if (sum == target) {
//                if (a[i] != a[left] && a[left] != a[right])
//                    cout << a[i] << " " << a[left] << " " << a[right] << "\n";
//
//                while (left < right && a[left] == a[left + 1])
//                    left++;
//
//                while (left < right && a[right] == a[right - 1])
//                    right--;
//
//                left++;
//                right--;
//            }
//            else if (sum < target)
//                left++;
//
//            else
//                right--;
//
//        }
//    }
//
//    return 0;
//}


//#include <iostream>
//using namespace std;
//
//void swap(int& a, int& b) {
//    int tmp = a;
//    a = b;
//    b = tmp;
//}
//
//void bubbleSort(int arr[], int n) {
//    for (int i = 0; i < n - 1; i++) {
//        for (int j = 0; j < n - i - 1; j++) {
//            if (arr[j] > arr[j + 1]) {
//                swap(arr[j], arr[j + 1]);
//            }
//        }
//    }
//}
//
//int main() {
//    int target, n;
//    cin >> target >> n;
//    int nums[1000];
//    for (int i = 0; i < n; i++) {
//        cin >> nums[i];
//    }
//    bubbleSort(nums, n);
//    int results[1000][3];
//    int resultCount = 0;
//    for (int i = 0; i < n - 2; i++) { 
//        if (i > 0 && nums[i] == nums[i - 1]) continue;
//
//        int x = nums[i];
//        int left = i + 1;
//        int right = n - 1;
//
//        while (left < right) {
//            int sum = x + nums[left] + nums[right];
//
//            if (sum == target) {
//                results[resultCount][0] = x;
//                results[resultCount][1] = nums[left];
//                results[resultCount][2] = nums[right];
//                resultCount++;
//                while (left < right && nums[left] == nums[left + 1]) left++;
//                while (left < right && nums[right] == nums[right - 1]) right--;
//                left++;
//                right--;
//            }
//            else if (sum < target) {
//                left++;
//            }
//            else {
//                right--;
//            }
//        }
//    }
//
//    for (int i = 0; i < resultCount; i++) {
//        cout << results[i][0] << " " << results[i][1] << " " << results[i][2] << endl;
//    }
//
//    return 0;
//}