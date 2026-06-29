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
    double Re;
    double Im;

    Complex(double real, double imag) : Re(real), Im(imag) {}

    Complex(double real) : Re(real), Im(0) {}

    Complex operator~() const {
        return Complex(Re, -Im);
    }

    Complex operator-() const {
        return Complex(-Re, -Im);
    }

    friend ostream& operator<<(ostream& os, const Complex& c);

    friend Complex operator+(Complex c1, Complex c2);

    friend Complex operator-(Complex c1, Complex c2);

    friend Complex operator*(Complex c1, Complex c2);

    friend Complex operator/(Complex c1, Complex c2);
};

Complex operator+(Complex c1, Complex c2) {
    return Complex(c1.Re + c2.Re, c1.Im + c2.Im);
}

Complex operator-(Complex c1, Complex c2) {
    return Complex(c1.Re - c2.Re, c1.Im - c2.Im);
}

Complex operator*(Complex c1, Complex c2) {
    return Complex(c1.Re * c2.Re - c1.Im * c2.Im, c1.Re * c2.Im + c1.Im * c2.Re);
}

Complex operator/(Complex c1, Complex c2) {
    double denominator = c2.Re * c2.Re + c2.Im * c2.Im;
    return Complex((c1.Re * c2.Re + c1.Im * c2.Im) / denominator, (c1.Im * c2.Re - c1.Re * c2.Im) / denominator);
}

double abs(Complex& c) {
    return sqrt(c.Re * c.Re + c.Im * c.Im);
}

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

Complex operator""i(unsigned long long Im) {
    return Complex(0, static_cast<double>(Im));
}

Complex operator""i(long double Im) {
    return Complex(0, static_cast<double>(Im));
}

#endif