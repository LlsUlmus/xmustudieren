#include "Obstacle.h"
#include <stdexcept>

Obstacle::Obstacle(float groundY) : groundY(groundY) {  // 默认向左移动
    shape.setSize(sf::Vector2f(40.f, 40.f));
    shape.setFillColor(sf::Color::Green);  // 绿色障碍物
    shape.setPosition(sf::Vector2f(800.f, groundY - shape.getSize().y));
    velocity.x = 200.f;
}

void Obstacle::update(float dt) {
    
}
void Obstacle::update(float dt, const Ground& ground) {
   shape.move(sf::Vector2f(-200.f * dt, 0.f));
   //float totalTime += dt;
   //for (auto& seg : shape) {
   //    seg.move(sf::Vector2f(-speed * dt, 0.f));
   //    // 超出左屏则移到最右侧
   //    // 计算当前片段的X位置（用于相位偏移）
       float xPos = shape.getPosition().x;


    float terrainHeight = ground.getY() - 50.0f * std::sin(xPos * 0.01f + ground.getTotalTime() * 2.0f);
    float groundLevel = terrainHeight - shape.getSize().y;  // 地面的Y坐标
    //强制跟随地形
    shape.setPosition(sf::Vector2f(xPos, groundLevel));
}

void Obstacle::render(sf::RenderWindow& window) const {
    window.draw(shape);
}

sf::FloatRect Obstacle::getBounds() const {
    return shape.getGlobalBounds();
}
sf::Vector2f Obstacle::getPosition() const {
    return shape.getPosition();
}
bool Obstacle::isOffScreen() const {
    // 获取障碍物位置和尺寸
    const sf::Vector2f pos = getPosition();
    const sf::Vector2f size = shape.getSize();

    // 当障碍物完全离开屏幕左侧时返回true
    return (pos.x + size.x) < 0;
}