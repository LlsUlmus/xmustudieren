#include <iostream>
using namespace std;
#define MAX_W 20
#define MAX_H 20
int santified[MAX_W][MAX_H];
char fogForests[MAX_W][MAX_H];
int W, H;
int c = 0; 
void dfs(int x, int y) {
    if (x < 0 || y < 0 || x >= H || y >= W)
        return;
    if (fogForests[x][y] == '#' || santified[x][y])
        return;
    c++;
    santified[x][y] = 1;
    dfs(x - 1, y); dfs(x + 1, y); dfs(x, y - 1); dfs(x, y + 1);
}

int main() {
    while (1) {
        cin >> W >> H;
        if (W == 0 && H == 0) break;
        memset(santified, 0, sizeof(santified));
        int beginx, beginy; 
        for (int i = 0; i < H; i++)
            for (int j = 0; j < W; j++) {
                cin >> fogForests[i][j];
                if (fogForests[i][j] == '@') {
                    beginx = i; beginy = j;
                }
            }
        c = 0;
        dfs(beginx, beginy);
        cout << c << endl;
    }
    return 0;
}

