//八皇后的第n个解
//#include <iostream>
//using namespace std;
//
//int n;
//int board[14];
//bool isValid(int row, int col) {
//    for (int i = 1; i < row; i++) {
//        if (board[i] == col || abs(row - i) == abs(col - board[i])) {
//            return false;
//        }
//    }
//    return true;
//}
//
//void backtrack(int row) {
//    if (row > n) {
//        for (int i = 1; i <= n; i++) {
//            cout << board[i];
//        }
//        cout << endl;
//        return;
//    }
//
//    for (int col = 1; col <= n; col++) {
//        if (isValid(row, col)) {
//            board[row] = col;
//            backtrack(row + 1);
//            board[row] = 0;
//        }
//    }
//}
//
//int main() {
//    cin >> n;
//    for (int i = 1; i <= n; i++) {
//        board[i] = 0;
//    }
//    backtrack(1); 
//    return 0;
//}

//#include <stdio.h>
//#include <math.h>
//#define N 8
//
//int s[93][N];        
//int buf[N];          
//int q = 0;           
//
//void equeen(int cur)
//{
//    int i, j;
//    if (cur == N)                               
//    {
//        for (int i = 0; i < N; i++) s[q][i] = buf[i];    
//        q++;
//        return;
//    }
//    else
//    {
//        for (i = 1; i <= N; i++)
//        {
//            for (j = 0; j < cur; j++)
//                if (buf[j] == i || abs(buf[j] - i) == abs(j - cur)) break; 
//            if (j == cur)
//            {
//                buf[cur] = i;     
//                equeen(cur + 1);    
//            }
//        }
//    }
//
//}
//
//int main()
//{
//
//    equeen(0);
//    int t, n;
//    scanf("%d", &t);
//    while (t--)
//    {
//        scanf("%d", &n);
//        for (int i = 0; i < 8; i++) printf("%d", s[n - 1][i]);
//        printf("\n");
//
//    }
//    return 0;
//}