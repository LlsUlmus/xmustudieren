#pragma once
#include <SFML/Graphics.hpp>
#include "Config.h"
#include "Ground.h"
#include "Obstacle.h"
#include <vector>
#include <stdexcept>


class Player
{
public:
    enum class PlayerState {
        GROUNDED,
        JUMPING
    };
    Player(float startX, float startY);   // 位置由 Game 注入
    void  update(float dt);               // 每帧更新
    void  render(sf::RenderWindow& win) const;
    PlayerState getState() const;
    void update(float dt, const Ground& ground, PlayerState state);

    
private:
   // sf::Sprite         sprite;        // 用 sprite 替代 shape
   // sf::Texture        texture;       // 加载 player.png
   //// sf::RectangleShape shape;             // 红色方块表示角色
   // sf::Vector2f       velocity;          // 仅保存 y 方向速度
    sf::RectangleShape shape;     // ✅ 改回 Rectangle
    sf::Texture        texture;   // 贴 player.png
    sf::Vector2f       velocity;
    float              gravity;
    float              jumpImpulse;
    float              jumpStrength = -500.f;
    float              groundY;
    PlayerState state;
};



//class Player {
//public:
//    Player();
//
//    Player(float x, float y);
//    void update(float dt, const Ground& ground);
//    void render(sf::RenderWindow& window) const;
//
//private:
//    sf::RectangleShape shape;
//    sf::Vector2f velocity;
//    float gravity;
//    float jumpImpulse;
//};
