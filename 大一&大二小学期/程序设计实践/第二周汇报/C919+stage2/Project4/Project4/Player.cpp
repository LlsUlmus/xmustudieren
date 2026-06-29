#include "Player.h"
#include <stdexcept>
#include <SFML/Window/Keyboard.hpp>
#include"Ground.h"
Ground ground(200.f);

Player::Player(float startX, float startY)
    : velocity(0.f, 0.f),
    gravity(GRAVITY),
    jumpImpulse(JUMP_IMPULSE)
{
    if (!texture.loadFromFile("player.png"))
        throw std::runtime_error("Failed to load player.png");

    shape.setSize({ 50.f, 50.f });
    shape.setOrigin(shape.getSize() / 2.f);
    shape.setTexture(&texture);                 // 贴纹理到矩形
    shape.setPosition({ startX, startY });
}
void Player::update(float dt, const Ground& ground, PlayerState state) {
    this->state = state;
    float xPos = shape.getPosition().x;
    float terrainHeight = ground.getY() - 50.0f * std::sin(xPos * 0.01f + ground.getTotalTime() * 2.0f);
    float groundLevel = terrainHeight - shape.getSize().y;  // 地面的Y坐标
    using Key = sf::Keyboard::Key;
    // Space 跳跃
    if (sf::Keyboard::isKeyPressed(Key::Space) && state == PlayerState::GROUNDED) {
        velocity.y = jumpStrength;
        this->state = PlayerState::JUMPING;
    }
    // 重力
    if (state == PlayerState::JUMPING) {
        velocity.y += gravity * dt;
        shape.move(sf::Vector2f(0, velocity.y * dt));  // 只在Y轴移动

        // 碰撞检测：如果落到地面
        if (shape.getPosition().y >= groundLevel) {
            shape.setPosition(sf::Vector2f(xPos, groundLevel));
            velocity.y = 0;
            this->state = PlayerState::GROUNDED;
        }
    }
    else {
        // 在地面时，强制跟随地形
        shape.setPosition(sf::Vector2f(xPos, groundLevel));
    }
}

//void Player::update(float dt)
//{
//    float halfH = shape.getSize().y / 2.f;
//    float floorY = GROUND_Y_LEFT - halfH;
//
//    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Scancode::Space) &&
//        shape.getPosition().y >= floorY - 1.f)
//    {
//        velocity.y = jumpImpulse;
//    }
//
//    velocity.y += gravity * dt;
//    shape.move({ 0.f, velocity.y * dt });
//
//    if (shape.getPosition().y > floorY) {
//        shape.setPosition({ shape.getPosition().x, floorY });
//        velocity.y = 0.f;
//    }
//}

Player::PlayerState Player::getState() const {
    return state;
}
void Player::render(sf::RenderWindow& win) const {
    win.draw(shape);
}



//#include "Player.h"
//#include <stdexcept>
//#include <SFML/Window/Keyboard.hpp> 
//
////Player::Player(float startX, float startY)
////    : velocity(0.f, 0.f),
////    gravity(GRAVITY),
////    jumpImpulse(JUMP_IMPULSE)
////{
////    if (!texture.loadFromFile("res/player.png"))
////        throw std::runtime_error("Failed to load player.png");
////
////    sprite.setTexture(texture);
////    sprite.setOrigin({ texture.getSize().x / 2.f, texture.getSize().y / 2.f });
////    sprite.setPosition({ startX, startY });
////}
//
//Player::Player(float startX, float startY)
//    : velocity(0.f, 0.f),
//    gravity(GRAVITY),
//    jumpImpulse(JUMP_IMPULSE)
//{
//    if (!texture.loadFromFile("res/player.png"))
//        throw std::runtime_error("Failed to load player.png");
//
//    sprite.setTexture(texture);
//    sprite.setOrigin({ texture.getSize().x / 2.f, texture.getSize().y / 2.f });
//    sprite.setPosition({ startX, startY });
//}
//
//void Player::update(float dt)
//{
//    float halfHeight = sprite.getGlobalBounds().size.y / 2.f;        // ★ 修改 height→size.y
//    float floorY = GROUND_Y_LEFT - halfHeight;
//
//    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Scancode::Space) && // ★ 用 Scancode::Space
//        sprite.getPosition().y >= floorY - 1.f) {
//        velocity.y = jumpImpulse;
//    }
//
//    velocity.y += gravity * dt;
//    sprite.move({ 0.f, velocity.y * dt });
//
//    if (sprite.getPosition().y > floorY) {
//        sprite.setPosition({ sprite.getPosition().x, floorY });
//        velocity.y = 0.f;
//    }
//}
//
//
////void Player::update(float dt)
////{
////    float halfHeight = sprite.getGlobalBounds().height / 2.f;
////    float floorY = GROUND_Y_LEFT - halfHeight;
////
////    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Space) &&
////        sprite.getPosition().y >= floorY - 1.f)
////    {
////        velocity.y = jumpImpulse;
////    }
////
////    velocity.y += gravity * dt;
////    sprite.move({ 0.f, velocity.y * dt });
////
////    if (sprite.getPosition().y > floorY) {
////        sprite.setPosition({ sprite.getPosition().x, floorY });
////        velocity.y = 0.f;
////    }
////}
//
//void Player::render(sf::RenderWindow& win) const {
//    win.draw(sprite);
//}



//#include "Player.h"
//
//Player::Player(float startX, float startY)
//    : velocity(0.f, 0.f),
//    gravity(GRAVITY),
//    jumpImpulse(JUMP_IMPULSE)
//{
//    if (!texture.loadFromFile("res/player.png")) {
//        throw std::runtime_error("Failed to load player.png");
//    }
//    sprite.setTexture(texture);
//    sprite.setOrigin({ texture.getSize().x / 2.f, texture.getSize().y / 2.f });
//    sprite.setPosition({ startX, startY });
//    //// 50×50 的红色矩形
//    //shape.setSize({ 50.f, 50.f });
//    //shape.setOrigin(shape.getSize() / 2.f);
//    //shape.setFillColor(sf::Color::Red);
//    //shape.setPosition({ startX, startY });
//}
//void Player::update(float dt)
//{
//    // 跳跃（按空格）
//    if (sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Space))          // ➜ SFML‑3 正确写法
//    {
//        // 简单脚下碰撞检测：只有落到地面才能再次跳
//        if (sprite.getPosition().y >= GROUND_Y_LEFT - sprite.getSize().y / 2.f - 1.f)
//            velocity.y = jumpImpulse;
//    }
//
//    // 重力
//    velocity.y += gravity * dt;
//    sprite.move({ 0.f, velocity.y * dt });
//
//    // 与地面碰撞，防止穿透
//    float floorY = GROUND_Y_LEFT - sprite.getSize().y / 2.f;
//    if (sprite.getPosition().y > floorY)
//    {
//        sprite.setPosition({ shape.getPosition().x, floorY });
//        velocity.y = 0.f;
//    }
//}
//
//void Player::render(sf::RenderWindow& win) const
//{
//    win.draw(sprite);
//}
//
////#ifndef PLAYER_H
////#define PLAYER_H
////
////#include <SFML/Graphics.hpp>
////#include "Player.h"
////#include "Config.h"
////
////
////class Player {
////private:
////    sf::Sprite sprite;
////    sf::Vector2f position;
////    sf::Vector2f velocity;
////    float gravity;
////    float groundY; // 新增groundY声明
////
////public:
////    // 修正构造函数声明，假设需要初始位置
////    Player(float x, float y);
////
////    // 修正update函数声明，接受时间增量
////    void update(float deltaTime);
////
////    // 其他成员函数...
////};
////
////#endif // PLAYER_H
//
//
////#include "Player.h"
////Player::Player(float groundY) : groundY(groundY) {
////    shape.setSize(sf::Vector2f(40.f, 40.f));
////    shape.setFillColor(sf::Color::Red);
////    shape.setPosition(sf::Vector2f(100.f, groundY - shape.getSize().y));
////
////    //velocity = sf::Vector2f(150.f, 0.f); // 预留横向速度
////    gravity = 1800.f;
////}
////void Player::update(float dt) {
////    using Key = sf::Keyboard::Key;
////    // Space 跳跃
////    if (sf::Keyboard::isKeyPressed(Key::Space) &&
////        shape.getPosition().y >= groundY - shape.getSize().y - 0.1f) {
////        velocity.y = -800.f;
////    }
////    // 重力
////    velocity.y += gravity * dt;
////    shape.move(velocity * dt);
////    // 与地面碰撞
////    if (shape.getPosition().y >= groundY - shape.getSize().y) {
////        shape.setPosition(sf::Vector2f(shape.getPosition().x, groundY - shape.getSize().y));
////        velocity.y = 0.f;
////    }
////}
////void Player::render(sf::RenderWindow& window) const {
////    window.draw(shape);
////}