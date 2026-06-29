#include <iostream>
#include <vector>
using namespace std;

typedef long long LL;

LL merge_sort(vector<int>& q, int l, int r) {
    if (l >= r) return 0;

    int mid = l + r >> 1;
    LL res = merge_sort(q, l, mid) + merge_sort(q, mid + 1, r);

    int i = l, j = mid + 1, k = 0;
    vector<int> temp(r - l + 1);

    while (i <= mid && j <= r) {
        if (q[i] <= q[j]) {
            temp[k++] = q[i++];
        }
        else {
            temp[k++] = q[j++];
            res += mid - i + 1;
        }
    }

    while (i <= mid) temp[k++] = q[i++];
    while (j <= r) temp[k++] = q[j++];

    for (i = l, k = 0; i <= r; i++, k++) q[i] = temp[k];

    return res;
}

int main() {
    int n;
    cin >> n;
    vector<int> q(n);

    for (int i = 0; i < n; i++) {
        cin >> q[i];
    }

    cout << merge_sort(q, 0, n - 1) << endl;

    return 0;
}