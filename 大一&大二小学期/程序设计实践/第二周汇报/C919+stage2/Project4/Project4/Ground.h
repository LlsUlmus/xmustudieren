#pragma once
#include <SFML/Graphics.hpp>
#include <vector>
#include "Config.h"
#include <SFML/Graphics.hpp>
#include <vector>
#include <stdexcept>


class Ground {
public:
    explicit Ground(float groundY);
    void update(float dt);
    void render(sf::RenderWindow& window) const;
    // 获取指定x位置的地形高度
    float getHeightAt(float x) const {
        return y - 50.0f * std::sin(x * 0.01f + totalTime * 2.0f);
    }
    // 获取总时间（用于外部计算）
    float getTotalTime() const { return totalTime; }
    // 获取基准Y坐标
    float getY() const { return y; }
    float getX() const { return xPos; }
private:
    std::vector<sf::RectangleShape> segments;
    float speed;
    float segmentHeight;
    float totalTime;
    float y;
    float xPos;
};