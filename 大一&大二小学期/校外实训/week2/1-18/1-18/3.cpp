#include<iostream>
#include<string>
using namespace std;

string cur;        
string goal;       

void switchs(string& cur, int i) {			
	if (cur[i] == '0') cur[i] = '1';
	else cur[i] = '0';
}

int Calculate(string cur) {
	int count = 0;                   
	for (int i = 1; i < cur.size(); i++) {  
		if (cur[i - 1] != goal[i - 1] && i != cur.size() - 1) {    
			switchs(cur, i);
			switchs(cur, i - 1);				
			switchs(cur, i + 1);
			count++;
		}
		else if (cur[i - 1] != goal[i - 1] && i == cur.size() - 1) {
			switchs(cur, i);					
			switchs(cur, i - 1);
			count++;
		}
	}
	if (cur[cur.size() - 1] == goal[goal.size() - 1])	
		return count;
	else return 31;   
}

int main() {
	cin >> cur;
	cin >> goal;
	int count0 = Calculate(cur);      
	switchs(cur, 0);					
	switchs(cur, 1);
	int count1 = Calculate(cur) + 1;
	if (count1 != 32 || count0 != 31) {    
		int min = count1 < count0 ? count1 : count0;
		cout << min;
	}
	else cout << "impossible";  
}


