# 设计实验1-1：按键控制LED灯 - 实验思路

## 实验要求

1. **物理按键控制LED**：
   - 按KEY1键，LED1灯亮；再按KEY1键，LED1灯灭（Toggle模式）
   - 按KEY2键，LED2灯亮；再按KEY2键，LED2灯灭（Toggle模式）

2. **按键状态在屏幕上的反馈**：
   - 按KEY1键（KEY2键）时，液晶屏上的KEY1键（KEY2键）图标会改变
   - 按KEY1键（KEY2键）时，液晶屏上的LED1（LED2）图标会变化

3. **触摸屏控制LED**：
   - 点击液晶屏上的LED1（LED2）图标时，开发板上的LED1（LED2）灯会亮/灭

## 实验工程命名
**qt5KeyLedDevice**

## 编程思路

### 一、实验功能分析

本实验需要实现**双向控制**：
- **物理按键 → LED控制 + 屏幕图标更新**
- **触摸屏图标点击 → LED控制 + 屏幕图标更新**

核心功能点：
1. 物理按键事件监听（KEY1、KEY2）
2. 触摸屏事件处理（LED图标点击）
3. LED状态管理（LED1、LED2的亮/灭状态）
4. 图标状态同步（按键图标、LED图标的显示更新）

### 二、参考现有实验代码

#### 1. LED实验的参考价值
现有的LED实验已经实现了：
- LED图标点击控制LED亮/灭
- 图标状态切换（pic_bulbon ↔ pic_bulboff）
- LED设备操作（LedOn1()/LedOff1()、LedOn2()/LedOff2()）

#### 2. LED实验的关键代码片段
```java
// LED实验中点击图标控制LED的代码
LedButton_1.setOnClickListener(new View.OnClickListener() {
    @Override
    public void onClick(View view) {
        if (IsLight_1_On) {
            ((ImageButton)view).setBackground(getDrawable(R.drawable.pic_bulboff));
            led.open();
            led.LedOff1();
            led.close();
        } else {
            ((ImageButton)view).setBackground(getDrawable(R.drawable.pic_bulbon));
            led.open();
            led.LedOn1();
            led.close();
        }
        IsLight_1_On = !IsLight_1_On;
    }
});
```

### 三、实现方案

#### 1. 界面布局设计（activity_main.xml）

**需要包含的元素：**
- 2个按键图标：KEY1图标、KEY2图标（显示按键状态）
- 2个LED图标：LED1图标、LED2图标（显示LED状态，可点击）

**布局建议：**
```xml
<?xml version="1.0" encoding="utf-8"?>
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:layout_width="match_parent"
    android:layout_height="match_parent"
    android:orientation="vertical"
    android:gravity="center">

    <!-- 第一行：KEY1和LED1 -->
    <LinearLayout
        android:layout_width="wrap_content"
        android:layout_height="wrap_content"
        android:orientation="horizontal"
        android:gravity="center">
        
        <!-- KEY1按键图标（仅显示，不可点击）-->
        <ImageView
            android:id="@+id/key1Icon"
            android:layout_width="wrap_content"
            android:layout_height="wrap_content"
            android:src="@drawable/key_unpressed"
            android:layout_margin="20dp" />
        
        <!-- LED1图标（可点击）-->
        <ImageButton
            android:id="@+id/led1Icon"
            android:layout_width="wrap_content"
            android:layout_height="wrap_content"
            android:background="@drawable/pic_bulboff"
            android:layout_margin="20dp" />
    </LinearLayout>

    <!-- 第二行：KEY2和LED2 -->
    <LinearLayout
        android:layout_width="wrap_content"
        android:layout_height="wrap_content"
        android:orientation="horizontal"
        android:gravity="center">
        
        <!-- KEY2按键图标（仅显示，不可点击）-->
        <ImageView
            android:id="@+id/key2Icon"
            android:layout_width="wrap_content"
            android:layout_height="wrap_content"
            android:src="@drawable/key_unpressed"
            android:layout_margin="20dp" />
        
        <!-- LED2图标（可点击）-->
        <ImageButton
            android:id="@+id/led2Icon"
            android:layout_width="wrap_content"
            android:layout_height="wrap_content"
            android:background="@drawable/pic_bulboff"
            android:layout_margin="20dp" />
    </LinearLayout>

</LinearLayout>
```

#### 2. 图标资源准备（res/drawable）

**需要的图标资源：**
- `key_unpressed.png`：按键未按下状态的图标
- `key_pressed.png`：按键按下状态的图标
- `pic_bulbon.gif`：LED亮状态图标（已有）
- `pic_bulboff.gif`：LED灭状态图标（已有）

**图标说明：**
- 按键图标：用于显示KEY1和KEY2的物理状态
- LED图标：复用LED实验中的图标资源

#### 3. 按键事件处理

**物理按键监听方法：**

在Android中监听物理按键，需要重写`onKeyDown()`方法：

```java
@Override
public boolean onKeyDown(int keyCode, KeyEvent event) {
    switch (keyCode) {
        case KeyEvent.KEYCODE_1:  // 假设KEY1对应按键码KEYCODE_1
        case KeyEvent.KEYCODE_F1: // 或者根据实际情况使用其他按键码
            // 处理KEY1按键
            handleKey1Press();
            return true;
            
        case KeyEvent.KEYCODE_2:  // 假设KEY2对应按键码KEYCODE_2
        case KeyEvent.KEYCODE_F2: // 或者根据实际情况使用其他按键码
            // 处理KEY2按键
            handleKey2Press();
            return true;
    }
    return super.onKeyDown(keyCode, event);
}
```

**注意：** 
- 实际的按键码需要根据开发板硬件确定
- 可能需要通过JNI调用底层驱动读取按键状态
- 或者通过监听系统按键事件文件（如`/dev/input/event0`）

#### 4. Java代码实现（MainActivity.java）

**核心实现逻辑：**

```java
package com.farsight.qt5keyleddevice;

import androidx.appcompat.app.AppCompatActivity;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.widget.ImageButton;
import android.widget.ImageView;

public class MainActivity extends AppCompatActivity {
    LED led = new LED();
    
    // LED状态标志位
    boolean isLed1On = false;
    boolean isLed2On = false;
    
    // 按键状态标志位（用于控制按键图标显示）
    boolean isKey1Pressed = false;
    boolean isKey2Pressed = false;
    
    // UI控件
    private ImageView key1Icon;
    private ImageView key2Icon;
    private ImageButton led1Icon;
    private ImageButton led2Icon;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        // 初始化UI控件
        key1Icon = (ImageView) findViewById(R.id.key1Icon);
        key2Icon = (ImageView) findViewById(R.id.key2Icon);
        led1Icon = (ImageButton) findViewById(R.id.led1Icon);
        led2Icon = (ImageButton) findViewById(R.id.led2Icon);
        
        // 设置LED1图标点击事件
        led1Icon.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                toggleLed1();
            }
        });
        
        // 设置LED2图标点击事件
        led2Icon.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                toggleLed2();
            }
        });
    }
    
    // 处理KEY1按键按下
    private void handleKey1Press() {
        // 更新按键图标（按下状态）
        key1Icon.setImageResource(R.drawable.key_pressed);
        isKey1Pressed = true;
        
        // 切换LED1状态
        toggleLed1();
        
        // 按键释放后恢复图标（可以使用延时或监听按键释放事件）
        // 这里简化处理，在toggleLed1中更新按键图标
    }
    
    // 处理KEY2按键按下
    private void handleKey2Press() {
        // 更新按键图标（按下状态）
        key2Icon.setImageResource(R.drawable.key_pressed);
        isKey2Pressed = true;
        
        // 切换LED2状态
        toggleLed2();
    }
    
    // 切换LED1状态（统一的状态切换函数）
    private void toggleLed1() {
        if (isLed1On) {
            // 当前LED1亮，切换到灭
            led1Icon.setBackground(getDrawable(R.drawable.pic_bulboff));
            led.open();
            led.LedOff1();
            led.close();
            isLed1On = false;
        } else {
            // 当前LED1灭，切换到亮
            led1Icon.setBackground(getDrawable(R.drawable.pic_bulbon));
            led.open();
            led.LedOn1();
            led.close();
            isLed1On = true;
        }
        
        // 恢复KEY1图标（如果是从按键触发的）
        if (isKey1Pressed) {
            key1Icon.setImageResource(R.drawable.key_unpressed);
            isKey1Pressed = false;
        }
    }
    
    // 切换LED2状态（统一的状态切换函数）
    private void toggleLed2() {
        if (isLed2On) {
            // 当前LED2亮，切换到灭
            led2Icon.setBackground(getDrawable(R.drawable.pic_bulboff));
            led.open();
            led.LedOff2();
            led.close();
            isLed2On = false;
        } else {
            // 当前LED2灭，切换到亮
            led2Icon.setBackground(getDrawable(R.drawable.pic_bulbon));
            led.open();
            led.LedOn2();
            led.close();
            isLed2On = true;
        }
        
        // 恢复KEY2图标（如果是从按键触发的）
        if (isKey2Pressed) {
            key2Icon.setImageResource(R.drawable.key_unpressed);
            isKey2Pressed = false;
        }
    }
    
    // 重写物理按键监听方法
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        switch (keyCode) {
            case KeyEvent.KEYCODE_1:
            case KeyEvent.KEYCODE_F1:
                // 根据实际硬件确定KEY1的按键码
                handleKey1Press();
                return true;
                
            case KeyEvent.KEYCODE_2:
            case KeyEvent.KEYCODE_F2:
                // 根据实际硬件确定KEY2的按键码
                handleKey2Press();
                return true;
        }
        return super.onKeyDown(keyCode, event);
    }
    
    // 可选：监听按键释放事件，更准确地控制按键图标显示
    @Override
    public boolean onKeyUp(int keyCode, KeyEvent event) {
        switch (keyCode) {
            case KeyEvent.KEYCODE_1:
            case KeyEvent.KEYCODE_F1:
                key1Icon.setImageResource(R.drawable.key_unpressed);
                return true;
                
            case KeyEvent.KEYCODE_2:
            case KeyEvent.KEYCODE_F2:
                key2Icon.setImageResource(R.drawable.key_unpressed);
                return true;
        }
        return super.onKeyUp(keyCode, event);
    }
}
```

#### 5. LED.java类（保持不变）
继续使用现有的LED类，包含`open()`、`close()`、`LedOn1()`、`LedOff1()`、`LedOn2()`、`LedOff2()`方法。

### 四、实现步骤

1. **创建新项目**
   - 创建名为`qt5KeyLedDevice`的Android项目
   - 或者基于LED实验项目进行修改

2. **准备图标资源**
   - 准备按键图标：`key_pressed.png`、`key_unpressed.png`
   - 复用LED图标：`pic_bulbon.gif`、`pic_bulboff.gif`
   - 将图标放入`app/src/main/res/drawable/`目录

3. **设计界面布局**
   - 创建或修改`activity_main.xml`
   - 添加KEY1、KEY2的ImageView控件（显示按键状态）
   - 添加LED1、LED2的ImageButton控件（可点击控制LED）

4. **实现Java代码**
   - 复制LED实验的LED.java类
   - 实现MainActivity.java：
     - 初始化UI控件
     - 实现LED图标的点击事件监听
     - 重写`onKeyDown()`方法监听物理按键
     - 实现统一的状态切换函数

5. **确定物理按键码**
   - 查阅开发板文档，确定KEY1和KEY2对应的按键码
   - 或者通过日志输出测试确定按键码
   - 修改`onKeyDown()`中的case语句

6. **测试验证**
   - 测试物理按键KEY1能否控制LED1并更新图标
   - 测试物理按键KEY2能否控制LED2并更新图标
   - 测试点击LED1图标能否控制LED1
   - 测试点击LED2图标能否控制LED2
   - 验证按键图标和LED图标的状态同步

### 五、关键点总结

1. **双向控制**：物理按键和触摸屏图标都能控制LED
2. **状态管理**：使用boolean标志位记录LED和按键的状态
3. **图标同步**：按键图标反映物理按键状态，LED图标反映LED状态
4. **统一函数**：使用`toggleLed1()`和`toggleLed2()`统一处理状态切换
5. **按键监听**：通过重写`onKeyDown()`和`onKeyUp()`方法处理物理按键事件
6. **按键码确定**：需要根据实际硬件确定KEY1和KEY2的按键码

### 六、注意事项

1. **物理按键码的确定**：
   - 不同开发板的按键码可能不同
   - 可以通过`Log.d()`输出keyCode来测试
   - 可能需要通过JNI调用底层驱动读取按键

2. **按键图标显示时机**：
   - 可以在`onKeyDown()`中显示按下图标
   - 在`onKeyUp()`中恢复未按下图标
   - 或者在状态切换后延时恢复

3. **状态同步**：
   - 无论通过物理按键还是触摸屏控制，都要更新LED图标
   - 确保物理按键和触摸屏控制的状态标志位一致

4. **设备操作**：
   - 每次操作LED前都要调用`led.open()`
   - 操作完成后调用`led.close()`
   - 设备打开失败时要有错误提示

5. **图标资源**：
   - 按键图标需要清晰地显示按下/未按下两种状态
   - LED图标复用LED实验中的资源即可
   - 图标尺寸建议保持一致

### 七、可能的技术难点

1. **物理按键事件捕获**：
   - Android应用默认可能无法捕获所有物理按键事件
   - 可能需要修改AndroidManifest.xml，添加相应权限
   - 或者使用JNI调用底层驱动直接读取按键状态

2. **按键码映射**：
   - 需要查阅开发板文档或测试确定KEY1和KEY2的实际按键码
   - 可能需要处理按键的长按、短按等不同情况

3. **实时性要求**：
   - 按键图标需要及时响应物理按键的按下和释放
   - 可能需要使用线程或定时器来处理按键图标的显示

