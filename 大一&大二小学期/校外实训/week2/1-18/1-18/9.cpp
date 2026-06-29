#include<iostream>
#include<algorithm>
#include<cmath>
#include<cstring>
using namespace std;
int fate[50][50] = { 0 };
int num = 0;
bool isLeft(int i, int j)
{
    return fate[i][j - 1] == 0;
}
bool isRight(int i, int j)
{
    return fate[i][j + 1] == 0;
}
int choose(int n, int i, int j)
{
    if (n == 0)
    {
        num++;
        return 1;
    }
    fate[i][j] = 1;
    if (isLeft(i, j))
    {
        choose(n - 1, i, j - 1);
    }
    if (isRight(i, j))
    {
        choose(n - 1, i, j + 1);
    }
    choose(n - 1, i + 1, j);
    fate[i][j] = 0;
    return num;
}
int main()
{
    int n;
    cin >> n;
    cout << choose(n, 0, 25);
    return 0;
}
