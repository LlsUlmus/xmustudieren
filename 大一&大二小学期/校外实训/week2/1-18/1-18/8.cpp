#include <iostream>
#include <vector>
using namespace std;
const char initialMatrix[9][9] = {
    {'5', '3', '0', '0', '7', '0', '0', '0', '0'},
    {'6', '0', '0', '1', '9', '5', '0', '0', '0'},
    {'0', '9', '8', '0', '0', '0', '0', '6', '0'},
    {'8', '0', '0', '0', '6', '0', '0', '0', '3'},
    {'4', '0', '0', '8', '0', '3', '0', '0', '1'},
    {'7', '0', '0', '0', '2', '0', '0', '0', '6'},
    {'0', '6', '0', '0', '0', '0', '2', '8', '0'},
    {'0', '0', '0', '4', '1', '9', '0', '0', '5'},
    {'0', '0', '0', '0', '8', '0', '0', '7', '9'}
};
bool isValidSudoku(const vector<vector<char>>& board) {
    for (int i = 0; i < 9; i++) {
        vector<bool> used(10, false);
        for (int j = 0; j < 9; j++) {
            if (board[i][j] == '0') continue;
            int num = board[i][j] - '0';
            if (used[num]) return false;
            used[num] = true;
        }
    }
    for (int j = 0; j < 9; j++) {
        vector<bool> used(10, false);
        for (int i = 0; i < 9; i++) {
            if (board[i][j] == '0') continue;
            int num = board[i][j] - '0';
            if (used[num]) return false;
            used[num] = true;
        }
    }
    for (int i = 0; i < 9; i += 3) {
        for (int j = 0; j < 9; j += 3) {
            vector<bool> used(10, false);
            for (int x = 0; x < 3; x++) {
                for (int y = 0; y < 3; y++) {
                    int row = i + x;
                    int col = j + y;
                    if (board[row][col] == '0') continue;
                    int num = board[row][col] - '0';
                    if (used[num]) return false;
                    used[num] = true;
                }
            }
        }
    }

    return true;
}

bool isSolution(const vector<vector<char>>& board) {
    if (!isValidSudoku(board)) return false;
    for (int i = 0; i < 9; i++) {
        for (int j = 0; j < 9; j++) {
            if (initialMatrix[i][j] != '0' && initialMatrix[i][j] != board[i][j]) {
                return false;
            }
        }
    }
    return true;
}

int main() {
    vector<vector<char>> board(9, vector<char>(9));
    for (int i = 0; i < 9; i++) {
        string row;
        cin >> row;
        for (int j = 0; j < 9; j++) {
            board[i][j] = row[j];
        }
    }
    if (isSolution(board)) {
        cout << "Yes" << endl;
    }
    else {
        cout << "No" << endl;
    }
    return 0;
}