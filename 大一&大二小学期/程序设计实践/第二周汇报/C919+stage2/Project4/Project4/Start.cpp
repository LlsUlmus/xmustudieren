#include "Start.h"
#include "Game.h"

Start::Start():running(true), enterPressed(false),window(sf::VideoMode(sf::Vector2u(800u, 600u), 32), "Debug Adventure") {
    if (titleFont.openFromFile("CHILLER.TTF"))
    {
        // error...
    }
    if (!startFont.openFromFile("CHILLER.TTF"))
    {
        // error...
    }
    // 设置开始游戏文本
    sf::Text titleText(titleFont);
    sf::Text startText(startFont);
    titleText.setString("DEBUG ADVENTURE");
    titleText.setCharacterSize(120);
    titleText.setFillColor(sf::Color::White);
    titleText.setPosition({ 10.f, 150.f });
    titleText.setStyle(sf::Text::Bold);
    startText.setString("Press Enter to start");
    startText.setCharacterSize(60);
    startText.setFillColor(sf::Color::White);
    startText.setPosition({ 220.f, 350.f });
    window.clear(sf::Color::Black);
    window.draw(titleText);
    window.draw(startText);
    window.display();
    printf("1");

}

bool Start::checkEnterPressed() const {
    return sf::Keyboard::isKeyPressed(sf::Keyboard::Key::Enter);
}

void Start::update(float dt) { printf("1");
    while (running)
    {
        while (const std::optional event = window.pollEvent())
        {
            if (checkEnterPressed()) {
                Game game;
                game.run();
                running = false;
            }
        }
    }
}
