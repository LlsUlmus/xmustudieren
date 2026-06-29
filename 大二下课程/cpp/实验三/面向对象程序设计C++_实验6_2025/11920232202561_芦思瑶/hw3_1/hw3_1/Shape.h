
#ifndef SHAPE_H
#define SHAPE_H

class Shape {
public:
    virtual double getArea() = 0;
    virtual ~Shape() {}
};

class Rectangle : public Shape {
private:
    double width;
    double height;
public:
    Rectangle(double w = 0, double h = 0);
    double getArea() override;
    ~Rectangle();
};

class Circle : public Shape {
private:
    double radius;
public:
    Circle(double r = 0);
    double getArea() override;
    ~Circle();
};

#endif