#pragma once
#ifndef COMPLEX_H_
#define COMPLEX_H_
#include <iostream>
#include <cmath>
#include <iomanip>
using std::ostream;
using std::sqrt;
class Complex {
public:
	double Re, Im;
	// 构造函数，初始化实部和虚部
	Complex(double Re, double Im) : Re(Re), Im(Im) {}
	// 构造函数，将实数转换为复数
	Complex(double d) : Re(d), Im(0) {}
	// 取共轭操作
	Complex operator~() const {
		return Complex(Re, -Im);
	}
	// 取负数操作
	Complex operator-() const {
		return Complex(-Re, -Im);
	}
	// 友元函数，重载流输出运算符
	friend ostream& operator<<(ostream& os, const Complex& c);
	// 重载加法运算符
	friend Complex operator+(Complex c1, Complex c2);
	// 重载减法运算符
	friend Complex operator-(Complex c1, Complex c2);
	// 重载乘法运算符
	friend Complex operator*(Complex c1, Complex c2);
		// 重载除法运算符
		friend Complex operator/(Complex c1, Complex c2);
};
// 重载加法运算符
Complex operator+(Complex c1, Complex c2) {
	return Complex(c1.Re + c2.Re, c1.Im + c2.Im);
}
// 重载减法运算符
Complex operator-(Complex c1, Complex c2) {
	return Complex(c1.Re - c2.Re, c1.Im - c2.Im);
}
// 重载乘法运算符
Complex operator*(Complex c1, Complex c2) {
	return Complex(c1.Re * c2.Re - c1.Im * c2.Im, c1.Re * c2.Im + c1.Im * c2.Re);
}
// 重载除法运算符
Complex operator/(Complex c1, Complex c2) {
	double denominator = c2.Re * c2.Re + c2.Im * c2.Im;
	return Complex((c1.Re * c2.Re + c1.Im * c2.Im) / denominator, (c1.Im * c2.Re - c1.Re * c2.Im) / denominator);
}
// 计算复数的模⻓
double abs(Complex& c) {
	return sqrt(c.Re * c.Re + c.Im * c.Im);
}
// 重载流输出运算符
ostream& operator<<(ostream& os, const Complex& c) {
	os << std::fixed << std::setprecision(2);
	if (std::abs(c.Re) < 1e-5 && std::abs(c.Im) < 1e-5) {
		os << "0.00";
	}
	else if (std::abs(c.Re) < 1e-5) {
		os << c.Im << "i";
	}
	else if (std::abs(c.Im) < 1e-5) {
		os << c.Re;
	}
	else if (c.Im > 0) {
		os << c.Re << " + " << c.Im << "i";
	}
	else {
		os << c.Re << " - " << std::abs(c.Im) << "i";
	}
	return os;
}
// 为虚数单位i定义字⾯量
Complex operator""_i(unsigned long long Im) {
	return Complex(0, static_cast<double>(Im));
}
// 为虚数单位i定义字⾯量
Complex operator""_i(long double Im) {
	return Complex(0, static_cast<double>(Im));
}
#endif