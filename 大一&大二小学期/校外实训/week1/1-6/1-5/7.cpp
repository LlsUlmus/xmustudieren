//È«ÅÅÁĞ
//#include <iostream>
//#include <algorithm>
//#include <string>
//using namespace std;
//
//void permute(string& str, int start) {
//    if (start == str.length()) {
//        cout << str << endl;
//        return;
//    }
//    sort(str.begin() + start, str.end());
//
//    for (int i = start; i < str.length(); i++) {
//        if (i > start && str[i] == str[start]) continue;
//        swap(str[start], str[i]);
//        permute(str, start + 1);
//        swap(str[start], str[i]);
//    }
//    sort(str.begin() + start, str.end());
//}
//
//int main() {
//    string str;
//    cin >> str;
//    sort(str.begin(), str.end());
//    permute(str, 0);
//    return 0;
//}