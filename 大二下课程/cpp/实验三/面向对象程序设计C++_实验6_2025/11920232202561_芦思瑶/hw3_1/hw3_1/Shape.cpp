#include "Shape.h"

Rectangle::Rectangle(double w, double h): width(w), height(h) {}

Rectangle::~Rectangle() {}

double Rectangle::getArea() {
    return width * height;
}

Circle::Circle(double r) : radius(r) {}

Circle::~Circle() {}

double Circle::getArea() {
    return 3.14 * radius * radius;
}