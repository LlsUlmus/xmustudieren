//四数之和


#include<iostream>
#include<vector>
using namespace std;

int main()
{
    int target, n;
    cin >> target >> n;
    vector<int> a(n);
    for (int i = 0; i < n; i++)
        cin >> a[i];

    for (int i = 0; i < n - 3; i++) {
        if (i > 0 && a[i] == a[i - 1])
            continue;
        for (int j = i + 1; j < n - 2; j++)
        {
            if (j > i && a[j] == a[j - 1])
                continue;

            int left = j + 1;
            int right = n - 1;

            while (left < right) {
                int sum = a[i] + a[j] + a[left] + a[right];

                if (sum == target) {
                    if (a[i] != a[j] && a[j] != a[left] && a[left] != a[right])
                        cout << a[i] << " " << a[j] << " " << a[left] << " " << a[right] << "\n";

                    while (left < right && a[left] == a[left + 1])
                        left++;
                    while (left < right && a[right] == a[right - 1])
                        right--;

                    left++;
                    right--;
                }
                else if (sum < target)
                    left++;

                else
                    right--;

            }
        }

    }
    return 0;
}