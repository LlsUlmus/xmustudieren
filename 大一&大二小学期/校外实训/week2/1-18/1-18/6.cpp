#include <iostream>
using namespace std;

void merge(int* a, int low, int mid, int high) {
    int n = high - low + 1;
    int* b = new int[n];
    int i = low, j = mid + 1, k = 0;
    while (i <= mid && j <= high) {
        if (a[i] <= a[j])
            b[k++] = a[i++];
        else
            b[k++] = a[j++];
    }
    while (i <= mid) b[k++] = a[i++];
    while (j <= high) b[k++] = a[j++];
    for (int i = low; i <= high; i++)
        a[i] = b[i - low];

    delete[] b; 
}
void mergesort(int* a, int low, int high) {
    if (low < high) {
        int mid = low + (high - low) / 2;
        mergesort(a, low, mid);
        mergesort(a, mid + 1, high);
        merge(a, low, mid, high);
    }
}

int main() {
    int n;
    cin >> n;
    int* a = new int[n];
    for (int i = 0; i < n; i++) {
        cin >> a[i];
    }
    mergesort(a, 0, n - 1);
    for (int i = 0; i < n; i++) {
        cout << a[i];
        if (i != n - 1) cout << " ";
    }
    cout << endl;
    delete[] a;
    return 0;
}


//#include <stdio.h>
//#include <iostream>
//#include <algorithm>
//#include <cstdlib>
//#include <cmath>
//using namespace std;
//void merge(int* a, int low, int mid, int hight) 
//{
//	int* b = new int[hight - low + 1]; 
//	int i = low, j = mid + 1, k = 0;   
//	while (i <= mid && j <= hight)
//	{
//		if (a[i] <= a[j])
//		{
//			b[k] = a[i++]; 
//		}
//		else
//		{
//			b[k] = a[j++];
//		}
//	}
//	while (i <= mid) 
//	{
//		b[k++] = a[i++];
//	}
//	while (j <= hight)
//	{
//		b[k++] = a[j++];
//	}
//	k = 0;  
//	for (int i = low; i <= hight; i++)  
//	{
//		a[i] = b[k++];
//	}
//	//delete[]b;   
//}
//void mergesort(int* a, int low, int hight) 
//{
//	if (low < hight)
//	{
//		int mid = (low + hight) / 2;
//		mergesort(a, low, mid);        
//		mergesort(a, mid + 1, hight);  
//		merge(a, low, mid, hight);      
//	}
//}
//int main()
//{
//	int n, a[100];
//	cin >> n;
//	for (int i = 0; i < n; i++)
//	{
//		cin >> a[i];
//	}
//	mergesort(a, 0, n - 1);
//	for (int i = 0; i < n; i++)
//	{
//		cout << a[i] << " ";
//	}
//	cout << endl;
//	return 0;
//}