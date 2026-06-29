#pragma once
#include <SFML/Graphics.hpp>
#include <random>  // 新增头文件
#include <vector>
#include "Player.h"
#include "Ground.h"
#include "Obstacle.h"
#include "Config.h"
#include "Ground.h"
#include <SFML/Graphics.hpp>
#include <vector>
#include <stdexcept>

class Game {
public:
    //explicit Game(sf::RenderWindow& win);  // 接收引用
    Game();
    void run();
private:
    void processEvents();
    void update(float dt);
    void render();
    void spawnObstacle();
    float generateRandomTime();

    sf::RenderWindow window;
    Player          player;
    Ground          ground;
    Player::PlayerState playerState;
    //O
// bstacle        obstacle;
    std::vector<Obstacle> obstacles;  // 管理多个障碍物
    float spawnTimer;           // 计时器
    float nextSpawnTime;        // 下一次显示的随机时间
    sf::Texture  backgroundTexture;
   // sf::Sprite   backgroundSprite;
    enum class State { Playing };
    State state;
};