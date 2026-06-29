//¼Ù±Ò

//#include<iostream>
//using namespace std;
//#include<cstring>
//string l[3];
//string r[3];
//string result[3];
//
//bool fate_coin(char coin, bool is_light)
//{
//	string c;
//	c.push_back(coin);
//
//	for (int i = 0; i < 3; i++)
//	{
//		string left = l[i], right = r[i];
//
//		bool inLeft = (left.find(c) != string::npos);
//		bool inRight = (right.find(c) != string::npos);
//		if (inLeft && inRight)
//			continue;
//		if (!is_light)
//		{
//			swap(right, left);
//		}
//		switch (result[i][0])
//		{
//		case 'e':
//			if (left.find(c) != string::npos || right.find(c) != string::npos)
//				return false;
//			break;
//
//		case 'u':
//			if (right.find(c) == string::npos)
//				return false;
//			break;
//
//		case 'd':
//			if (left.find(c) == string::npos)
//				return false;
//			break;
//		}
//
//	}
//	return true;
//}
//
//
//int main()
//{
//
//	int n;
//	cin >> n;
//	int i = 0;
//	while (i++ < n)
//	{
//		for (int j = 0; j < 3; j++)
//			cin >> l[j] >> r[j] >> result[j];
//		for (char coin = 'A'; coin <= 'L'; coin++)
//		{
//			if (fate_coin(coin, true))
//			{
//				cout << coin << " is the counterfeit coin and it is light. " << endl;
//				break;
//			}
//			else if (fate_coin(coin, false))
//			{
//				cout << coin << " is the counterfeit coin and it is heavy. " << endl;
//				break;
//			}
//		}
//	}
//}







//#include <iostream>
//#include <string>
//using namespace std;
//
//struct Weighing {
//    string left, right, result;
//};
//
//int main() {
//    int n;
//    cin >> n;
//
//    for (int caseNum = 0; caseNum < n; caseNum++) {
//        Weighing weighings[3];
//        for (int i = 0; i < 3; i++) {
//            cin >> weighings[i].left >> weighings[i].right >> weighings[i].result;
//        }
//
//        bool found = false;
//        for (char coin = 'A'; coin <= 'L'; coin++) {
//            for (int status = 0; status < 2; status++) {
//                bool valid = true;
//                for (int i = 0; i < 3; i++) {
//                    const string& left = weighings[i].left;
//                    const string& right = weighings[i].right;
//                    const string& res = weighings[i].result;
//
//                    if (status == 0) { // coinÇá
//                        if (res == "up") {
//                            if (right.find(coin) == string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                        else if (res == "down") {
//                            if (left.find(coin) == string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                        else if (res == "even") {
//                            if (left.find(coin) != string::npos || right.find(coin) != string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                    }
//                    else { // coinÖØ
//                        if (res == "up") {
//                            if (left.find(coin) == string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                        else if (res == "down") {
//                            if (right.find(coin) == string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                        else if (res == "even") {
//                            if (left.find(coin) != string::npos || right.find(coin) != string::npos) {
//                                valid = false;
//                                break;
//                            }
//                        }
//                    }
//                }
//
//                if (valid) {
//                    cout << coin << " is the counterfeit coin and it is "
//                        << (status == 0 ? "light" : "heavy")  << endl;
//                    found = true;
//                    break;
//                }
//            }
//            if (found) break;
//        }
//    }
//
//    return 0;
//}