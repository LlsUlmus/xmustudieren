#include "Ground.h"
#include <stdexcept>
Ground::Ground(float groundY) : speed(200.f), segmentHeight(200.f), y(groundY), totalTime(0.f) {
    // 创建 4 个矩形地块拼接 (800px 宽窗口)
    for (int i = 0; i < 160; ++i) {
        sf::RectangleShape seg;
        seg.setSize(sf::Vector2f(5.f, segmentHeight));
        seg.setFillColor(sf::Color::Black);
        seg.setPosition(sf::Vector2f(i * 5.f, y));
        segments.push_back(seg);
    }
}
void Ground::update(float dt) {
    totalTime += dt;
    for (auto& seg : segments) {
        seg.move(sf::Vector2f(-speed * dt, 0.f));
        // 超出左屏则移到最右侧
        // 计算当前片段的X位置（用于相位偏移）
        xPos = seg.getPosition().x;

        // 计算正弦高度偏移
        float heightOffset = 50.0f * std::sin(xPos * 0.01f + totalTime * 2.0f);
        // 设置新高度（这里修改为设置位置，因为矩形是从左上角开始的）
        seg.setSize(sf::Vector2f(5.f, segmentHeight + heightOffset));
        seg.setPosition(sf::Vector2f(xPos, y - heightOffset));
        if (seg.getPosition().x + 5.f < 0.f) {
            float maxX = 0.f;
            for (auto& s : segments) {
                maxX = std::max(maxX, s.getPosition().x);
            }
            seg.setPosition(sf::Vector2f(maxX + 5.f, y));
        }
    }
}
void Ground::render(sf::RenderWindow& window) const {
    for (const auto& seg : segments) window.draw(seg);
}