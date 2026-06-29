#include <iostream>
#include <algorithm>
using namespace std;

int main() {
	int n, num[100005];
	cin >> n;
	for (int i = 0; i < n; i++) {
		cin >> num[i];
	}
	sort(num, num + n);
	int k = 0;
	cin >> k;
	for (int i = n - 1; i > n - 1 - k; i--) {
		cout << num[i] << endl;
	}
	return 0;
}