# Android实验项目分析报告

## 目录
1. [第一次Android实验案例分析](#第一次android实验案例分析)
2. [第二次Android实验案例分析](#第二次android实验案例分析)
3. [Android与Linux设备驱动对比分析](#android与linux设备驱动对比分析)

---

## 第一次Android实验案例分析

### 案例选择
根据要求，从以下类别中各选择1个案例进行分析：
- **d) ActionBar和菜单**：ActionBarDemo
- **f) 组件间通讯**：ActivityCommunication
- **g) 数据库系统与访问**：SQLiteExam2
- **i) 图形图像**：GraphicAnimation
- **k) Android应用项目**：NoteApp

---

## 案例1：ActionBarDemo（ActionBar和菜单）

### 1.1 工程文件结构分析

#### 1.1.1 Android项目目录结构

```
ActionBarDemo/
├── app/
│   ├── src/
│   │   ├── main/
│   │   │   ├── java/
│   │   │   │   └── com/farsight/actionbardemo/
│   │   │   │       └── ActionBarDemo.java          # 主Activity类
│   │   │   ├── res/
│   │   │   │   ├── layout/
│   │   │   │   │   └── activity_main.xml           # 布局文件
│   │   │   │   ├── values/
│   │   │   │   │   └── strings.xml                 # 字符串资源
│   │   │   │   └── ...
│   │   │   └── AndroidManifest.xml                 # 应用清单文件
│   │   ├── build.gradle                            # 模块构建配置
│   └── ...
├── build.gradle                                    # 项目构建配置
└── settings.gradle                                 # 项目设置
```

#### 1.1.2 AndroidManifest.xml结构分析

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application>
        <!-- Activity组件声明 -->
        <activity android:name=".ActionBarDemo"
            android:theme="@style/Theme.AppCompat.Light"
            android:exported="true">
            <!-- Intent过滤器：定义应用入口 -->
            <intent-filter>
                <action android:name="android.intent.action.MAIN"/>
                <category android:name="android.intent.category.LAUNCHER"/>
            </intent-filter>
        </activity>
    </application>
</manifest>
```

**关键点分析：**
- `<activity>`：声明Activity组件，这是Android四大组件之一
- `android:name=".ActionBarDemo"`：指定Activity类名
- `<intent-filter>`：定义Intent过滤器，MAIN/LAUNCHER表示这是应用入口
- `android:exported="true"`：允许其他应用启动此Activity

### 1.2 核心代码剖析

#### 1.2.1 ActionBarDemo.java核心代码

```java
package com.farsight.actionbardemo;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.app.ActionBar;

public class ActionBarDemo extends AppCompatActivity {
    private ActionBar actionBar;                    // ActionBar对象引用
    private boolean threadOn = false;               // 线程控制标志
    private final int HIDE = 1;                     // Handler消息类型：隐藏
    private final int SHOW = 2;                     // Handler消息类型：显示
    
    // Handler：用于在主线程中更新UI
    private Handler handler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case HIDE:
                    if (actionBar != null) {
                        actionBar.hide();           // 隐藏ActionBar
                    }
                    break;
                case SHOW:
                    if (actionBar != null) {
                        actionBar.show();           // 显示ActionBar
                    }
                    break;
            }
        }
    };
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        // 获取ActionBar实例（使用Support库）
        actionBar = getSupportActionBar();
        
        if (actionBar != null) {
            actionBar.setTitle("华清远见");          // 设置ActionBar标题
            actionBar.hide();                       // 初始状态：隐藏
        }
        
        // 启动后台线程，循环显示/隐藏ActionBar
        actionBarHideAndShow = new ActionBarHideAndShow();
        threadOn = true;
        actionBarHideAndShow.start();
    }
    
    // 内部线程类：实现ActionBar的自动显示/隐藏
    private class ActionBarHideAndShow extends Thread {
        @Override
        public void run() {
            while (threadOn) {
                try {
                    sleep(500);                     // 每500ms执行一次
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                count--;
                if (count == 0) {
                    count = 10;
                    // 切换显示/隐藏状态
                    old = old == HIDE ? SHOW : HIDE;
                    // 通过Handler发送消息到主线程
                    handler.sendEmptyMessage(old);
                }
            }
        }
    }
}
```

**代码关键点解析：**

1. **Activity组件应用**：
   - `extends AppCompatActivity`：继承自支持库的Activity基类
   - `onCreate()`：Activity生命周期方法，初始化UI和逻辑

2. **Handler机制**：
   - Android不允许在子线程中直接更新UI
   - Handler用于实现线程间通信，将子线程的消息传递到主线程
   - `handleMessage()`：处理来自子线程的消息

3. **ActionBar操作**：
   - `getSupportActionBar()`：获取ActionBar实例
   - `actionBar.hide()/show()`：控制ActionBar的显示和隐藏

### 1.3 文件调用关系图

```
AndroidManifest.xml
    │
    └── 声明并注册ActionBarDemo Activity
            │
            └── ActionBarDemo.java (Activity)
                    │
                    ├── onCreate()
                    │   ├── setContentView() → activity_main.xml (布局)
                    │   ├── getSupportActionBar() → ActionBar对象
                    │   └── 启动ActionBarHideAndShow线程
                    │
                    ├── Handler
                    │   └── handleMessage() → 更新UI（显示/隐藏ActionBar）
                    │
                    └── ActionBarHideAndShow (Thread)
                            └── run() → 发送消息到Handler
```

### 1.4 流程图

```
开始
  │
  ├─→ onCreate() 执行
  │     │
  │     ├─→ 加载布局文件 activity_main.xml
  │     │
  │     ├─→ 获取ActionBar实例
  │     │     │
  │     │     └─→ 设置标题："华清远见"
  │     │     └─→ 初始隐藏ActionBar
  │     │
  │     └─→ 启动ActionBarHideAndShow线程
  │
  ├─→ ActionBarHideAndShow线程运行
  │     │
  │     ├─→ 循环执行（每500ms）
  │     │     │
  │     │     ├─→ count递减
  │     │     │
  │     │     └─→ count == 0?
  │     │           │
  │     │           ├─→ 是：切换HIDE/SHOW状态
  │     │           │     │
  │     │           │     └─→ handler.sendEmptyMessage()
  │     │           │           │
  │     │           │           └─→ Handler.handleMessage()
  │     │           │                 │
  │     │           │                 ├─→ HIDE → actionBar.hide()
  │     │           │                 └─→ SHOW → actionBar.show()
  │     │           │
  │     │           └─→ 否：继续循环
  │     │
  │     └─→ threadOn == false → 线程结束
  │
  └─→ onDestroy() → 清理资源
```

---

## 案例2：ActivityCommunication（组件间通讯）

### 2.1 工程文件结构分析

```
ActivityCommunication/
├── app/
│   ├── src/main/
│   │   ├── java/com/farsight/activitycommunication/
│   │   │   ├── MainActivity.java          # 主Activity
│   │   │   ├── activity1.java             # 子Activity1
│   │   │   └── activity2.java            # 子Activity2
│   │   ├── res/layout/
│   │   │   ├── activity_main.xml          # 主Activity布局
│   │   │   ├── activity1.xml              # Activity1布局
│   │   │   └── activity2.xml              # Activity2布局
│   │   └── AndroidManifest.xml
```

#### 2.1.1 AndroidManifest.xml分析

```xml
<application>
    <!-- 主Activity -->
    <activity android:name=".MainActivity" android:exported="true">
        <intent-filter>
            <action android:name="android.intent.action.MAIN" />
            <category android:name="android.intent.category.LAUNCHER" />
        </intent-filter>
    </activity>
    
    <!-- 子Activity1：用于数据输入和返回 -->
    <activity android:name=".activity1" />
    
    <!-- 子Activity2：用于简单返回 -->
    <activity android:name=".activity2" />
</application>
```

**关键点：**
- 三个Activity都在清单文件中注册
- 只有MainActivity有LAUNCHER intent-filter，作为应用入口
- activity1和activity2没有intent-filter，只能通过显式Intent启动

### 2.2 核心代码剖析

#### 2.2.1 MainActivity.java（Intent应用）

```java
public class MainActivity extends AppCompatActivity {
    private static final int SUBACTIVITY1 = 1;      // Activity1请求码
    private static final int SUBACTIVITY2 = 2;      // Activity2请求码
    TextView textView;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        textView = (TextView)findViewById(R.id.textshow);
        Button button1 = (Button) findViewById(R.id.button1);
        Button button2 = (Button) findViewById(R.id.button2);
        
        // 按钮1：启动Activity1并等待返回结果
        button1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                // 创建显式Intent，指定目标Activity
                Intent intent = new Intent(MainActivity.this, activity1.class);
                // 启动Activity并等待结果（使用startActivityForResult）
                startActivityForResult(intent, SUBACTIVITY1);
            }
        });
        
        // 按钮2：启动Activity2并等待返回结果
        button2.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(MainActivity.this, activity2.class);
                startActivityForResult(intent, SUBACTIVITY2);
            }
        });
    }
    
    // 接收子Activity返回的结果
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        
        switch(requestCode){
            case SUBACTIVITY1:
                // 检查返回结果是否成功
                if (resultCode == RESULT_OK){
                    // 从Intent中获取返回的数据（URI）
                    Uri uriData = data.getData();
                    textView.setText(uriData.toString());
                }
                break;
            case SUBACTIVITY2:
                // Activity2没有返回数据
                break;
        }
    }
}
```

**Intent机制解析：**

1. **显式Intent**：
   - `new Intent(MainActivity.this, activity1.class)`：明确指定目标Activity
   - 用于应用内部组件间通信

2. **startActivityForResult()**：
   - 启动Activity并等待返回结果
   - 第二个参数是请求码（requestCode），用于区分不同的请求

3. **onActivityResult()**：
   - 接收子Activity返回的结果
   - `requestCode`：区分是哪个Activity返回的
   - `resultCode`：返回状态（RESULT_OK/RESULT_CANCELED）
   - `data`：返回的数据（Intent对象）

#### 2.2.2 activity1.java（返回数据）

```java
public class activity1 extends Activity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity1);
        
        final EditText editText = (EditText)findViewById(R.id.text1);
        Button btnOK = (Button)findViewById(R.id.button1);
        Button btnCancel = (Button)findViewById(R.id.button2);
        
        // OK按钮：返回数据
        btnOK.setOnClickListener(new View.OnClickListener(){
            public void onClick(View view){
                String uriString = editText.getText().toString();
                Uri data = Uri.parse(uriString);        // 将字符串转换为URI
                Intent result = new Intent(null, data);  // 创建结果Intent
                setResult(RESULT_OK, result);           // 设置返回结果
                finish();                               // 关闭Activity
            }
        });
        
        // Cancel按钮：取消操作
        btnCancel.setOnClickListener(new View.OnClickListener(){
            public void onClick(View view){
                setResult(RESULT_CANCELED, null);       // 设置取消状态
                finish();
            }
        });
    }
}
```

**关键点：**
- `setResult()`：设置返回给父Activity的结果
- `finish()`：关闭当前Activity，返回到父Activity

### 2.3 文件调用关系图

```
AndroidManifest.xml
    │
    ├── MainActivity (主Activity)
    │     │
    │     ├── onCreate()
    │     │   ├── 加载 activity_main.xml
    │     │   └── 设置按钮监听器
    │     │
    │     ├── Button1点击
    │     │   └── Intent → activity1
    │     │         └── startActivityForResult()
    │     │
    │     ├── Button2点击
    │     │   └── Intent → activity2
    │     │         └── startActivityForResult()
    │     │
    │     └── onActivityResult()
    │           └── 处理返回数据
    │
    ├── activity1 (子Activity)
    │     │
    │     ├── onCreate()
    │     │   └── 加载 activity1.xml
    │     │
    │     ├── OK按钮
    │     │   └── setResult(RESULT_OK, Intent)
    │     │         └── finish() → 返回到MainActivity
    │     │
    │     └── Cancel按钮
    │           └── setResult(RESULT_CANCELED)
    │                 └── finish()
    │
    └── activity2 (子Activity)
          │
          └── onCreate()
                └── 加载 activity2.xml
                      └── 返回按钮 → finish()
```

### 2.4 Activity间通信流程图

```
MainActivity启动
  │
  ├─→ 用户点击Button1
  │     │
  │     └─→ 创建Intent (显式Intent)
  │           │
  │           └─→ startActivityForResult(intent, SUBACTIVITY1)
  │                 │
  │                 └─→ Activity1启动
  │                       │
  │                       ├─→ 用户输入数据
  │                       │
  │                       ├─→ 点击OK按钮
  │                       │     │
  │                       │     ├─→ 创建结果Intent
  │                       │     │     └─→ 包含URI数据
  │                       │     │
  │                       │     ├─→ setResult(RESULT_OK, result)
  │                       │     │
  │                       │     └─→ finish()
  │                       │           │
  │                       │           └─→ Activity1关闭
  │                       │
  │                       └─→ 点击Cancel按钮
  │                             │
  │                             ├─→ setResult(RESULT_CANCELED)
  │                             │
  │                             └─→ finish()
  │
  └─→ onActivityResult()被调用
        │
        ├─→ requestCode == SUBACTIVITY1?
        │     │
        │     ├─→ 是
        │     │     │
        │     │     ├─→ resultCode == RESULT_OK?
        │     │     │     │
        │     │     │     ├─→ 是：从Intent中提取URI数据
        │     │     │     │     │
        │     │     │     │     └─→ 更新TextView显示
        │     │     │     │
        │     │     │     └─→ 否：不处理
        │     │     │
        │     │     └─→ 结束
        │     │
        │     └─→ 否：不处理
        │
        └─→ 流程结束
```

---

## 案例3：SQLiteExam2（数据库系统与访问）

### 3.1 工程文件结构分析

```
SQLiteExam2/
├── app/src/main/
│   ├── java/com/farsight/sqliteexam2/
│   │   └── MainActivity.java              # 主Activity（包含数据库操作）
│   ├── res/
│   │   ├── layout/
│   │   │   ├── listcontent.xml            # ListView项布局
│   │   │   └── ...
│   │   └── ...
│   └── AndroidManifest.xml
```

### 3.2 核心代码剖析

#### 3.2.1 MainActivity.java（SQLite数据库应用）

```java
public class MainActivity extends AppCompatActivity {
    private final String DATABASE_NAME = "school";           // 数据库名称
    private SQLiteDatabase db;                              // 数据库对象
    private ListView lv;                                     // 列表视图
    private ArrayList<Map<String, Object>> data;            // 数据列表
    
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        // 创建ListView（动态创建，未使用XML布局）
        lv = new ListView(this);
        
        // 创建数据库辅助类实例
        MyDatabaseHelper myDBHelper = new MyDatabaseHelper(
            this, DATABASE_NAME, null, 3);
        
        // 获取可写数据库（如果不存在会自动创建）
        db = myDBHelper.getWritableDatabase();
        
        // 初始化数据：插入一条学生记录
        initData("tom", "09393", "1106");
        
        // 查询STUDENTINFO表中的所有数据
        String sql = "SELECT * FROM STUDENTINFO;";
        Cursor result = db.rawQuery(sql, null);
        
        // 将查询结果转换为List
        data.clear();
        while (!result.isLast()) {
            result.moveToNext();
            HashMap<String, Object> mymap = new HashMap<>();
            // 遍历所有列
            for (int i = 0; i < result.getColumnCount(); i++) {
                mymap.put(result.getColumnName(i), result.getString(i));
            }
            data.add(mymap);
        }
        
        // 创建SimpleAdapter适配器
        SimpleAdapter adapter = new SimpleAdapter(
            this, data,
            R.layout.listcontent,                            // 列表项布局
            new String[] { "ID", "NAME", "PHONE", "CLASS" }, // 数据键
            new int[] { R.id.textView1, R.id.textView2,      // 视图ID
                       R.id.textView3, R.id.textView4 });
        
        lv.setAdapter(adapter);
        setContentView(lv);
        
        result.close();                                      // 关闭Cursor
        db.close();                                          // 关闭数据库
    }
    
    // 插入数据到数据库
    private void initData(String name, String phone, String sclass) {
        ContentValues values = new ContentValues();          // 键值对容器
        values.put("NAME", name);
        values.put("PHONE", phone);
        values.put("CLASS", sclass);
        // 插入数据（ID为自增主键，自动生成）
        db.insert("STUDENTINFO", "ID", values);
    }
    
    // 数据库辅助类：继承SQLiteOpenHelper
    class MyDatabaseHelper extends SQLiteOpenHelper {
        public String DATABASE_TABLE = "STUDENTINFO";
        
        // 创建表的SQL语句
        public final String DB_CREATE_TABLE = 
            "CREATE TABLE " + DATABASE_TABLE
            + "( " + "ID     INTEGER    NOT NULL,"
            + "NAME   CHAR(20)   NOT NULL," 
            + "PHONE  CHAR(20),"
            + "CLASS  CHAR(50)," 
            + "PRIMARY KEY(ID) );";
        
        // 构造函数
        public MyDatabaseHelper(Context context, String name,
                                CursorFactory factory, int version) {
            super(context, name, factory, version);
        }
        
        // 数据库首次创建时调用
        @Override
        public void onCreate(SQLiteDatabase db) {
            db.execSQL(DB_CREATE_TABLE);                     // 执行建表SQL
        }
        
        // 数据库版本升级时调用
        @Override
        public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
            // 升级逻辑（本案例未实现）
        }
    }
}
```

**SQLite数据库关键点：**

1. **SQLiteOpenHelper**：
   - 管理数据库的创建和版本控制
   - `onCreate()`：首次创建数据库时调用
   - `onUpgrade()`：数据库版本升级时调用

2. **数据库操作**：
   - `getWritableDatabase()`：获取可写数据库
   - `db.insert()`：插入数据
   - `db.rawQuery()`：执行SQL查询
   - `Cursor`：查询结果游标，用于遍历数据

3. **ContentValues**：
   - 用于存储键值对数据
   - 配合`insert()`和`update()`使用

### 3.3 文件调用关系图

```
AndroidManifest.xml
    │
    └── MainActivity
          │
          ├── onCreate()
          │     │
          │     ├── 创建ListView
          │     │
          │     ├── 创建MyDatabaseHelper
          │     │     │
          │     │     └── SQLiteOpenHelper
          │     │           │
          │     │           ├── onCreate() → 创建STUDENTINFO表
          │     │           └── getWritableDatabase() → 打开数据库
          │     │
          │     ├── initData()
          │     │     │
          │     │     ├── ContentValues → 封装数据
          │     │     │
          │     │     └── db.insert() → 插入数据
          │     │
          │     ├── db.rawQuery() → 查询数据
          │     │     │
          │     │     └── Cursor → 遍历结果
          │     │           │
          │     │           └── 转换为ArrayList<Map>
          │     │
          │     ├── SimpleAdapter → 数据适配器
          │     │     │
          │     │     └── listcontent.xml → 列表项布局
          │     │
          │     └── lv.setAdapter() → 显示数据
          │
          └── MyDatabaseHelper (内部类)
                │
                ├── onCreate() → 创建表结构
                └── onUpgrade() → 版本升级
```

### 3.4 数据库操作流程图

```
应用启动
  │
  ├─→ MainActivity.onCreate()
  │     │
  │     ├─→ 创建MyDatabaseHelper实例
  │     │     │
  │     │     └─→ 调用getWritableDatabase()
  │     │           │
  │     │           ├─→ 数据库不存在？
  │     │           │     │
  │     │           │     ├─→ 是：调用onCreate()
  │     │           │     │     │
  │     │           │     │     └─→ 执行CREATE TABLE语句
  │     │           │     │           │
  │     │           │     │           └─→ 创建STUDENTINFO表
  │     │           │     │
  │     │           │     └─→ 否：直接打开数据库
  │     │           │
  │     │           └─→ 返回SQLiteDatabase对象
  │     │
  │     ├─→ initData() → 插入数据
  │     │     │
  │     │     ├─→ 创建ContentValues
  │     │     │     │
  │     │     │     └─→ 添加键值对（NAME, PHONE, CLASS）
  │     │     │
  │     │     └─→ db.insert("STUDENTINFO", "ID", values)
  │     │           │
  │     │           └─→ 数据库插入记录（ID自动递增）
  │     │
  │     ├─→ db.rawQuery("SELECT * FROM STUDENTINFO", null)
  │     │     │
  │     │     └─→ 返回Cursor对象
  │     │           │
  │     │           └─→ 遍历Cursor
  │     │                 │
  │     │                 ├─→ moveToNext() → 移动到下一行
  │     │                 │
  │     │                 ├─→ getColumnName(i) → 获取列名
  │     │                 │
  │     │                 └─→ getString(i) → 获取列值
  │     │                       │
  │     │                       └─→ 转换为HashMap → 添加到List
  │     │
  │     ├─→ 创建SimpleAdapter
  │     │     │
  │     │     └─→ 绑定数据到ListView
  │     │
  │     └─→ 显示ListView
  │
  └─→ 关闭Cursor和数据库
```

---

## 案例4：GraphicAnimation（图形图像）

### 4.1 工程文件结构分析

```
GraphicAnimation/
├── app/src/main/
│   ├── java/com/farsight/graphicanimation/
│   │   └── MainActivity.java              # 主Activity
│   ├── res/
│   │   ├── layout/
│   │   │   └── main.xml                   # 主布局文件
│   │   ├── anim/                          # 动画资源目录
│   │   │   ├── alpha.xml                  # 透明度动画
│   │   │   ├── rotate.xml                 # 旋转动画
│   │   │   ├── scale.xml                  # 缩放动画
│   │   │   ├── translate.xml              # 平移动画
│   │   │   └── combined.xml               # 组合动画
│   │   └── ...
│   └── AndroidManifest.xml
```

### 4.2 核心代码剖析

#### 4.2.1 MainActivity.java（动画应用）

```java
public class MainActivity extends AppCompatActivity implements View.OnClickListener {
    private ImageView mImageView;
    private final int STATICMODE = 1;              // 代码方式创建动画
    private final int XMLMODE = 2;                 // XML方式创建动画
    private int mode = XMLMODE;                    // 当前模式
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        
        initView();
        // 设置按钮点击监听器
        findViewById(R.id.btn_scale).setOnClickListener(this);
        findViewById(R.id.btn_alpha).setOnClickListener(this);
        findViewById(R.id.btn_translate).setOnClickListener(this);
        findViewById(R.id.btn_rotate).setOnClickListener(this);
        findViewById(R.id.btn_comb).setOnClickListener(this);
    }
    
    private void initView() {
        mImageView = (ImageView) findViewById(R.id.img_view);
    }
    
    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            case R.id.btn_scale:
                if (mode == STATICMODE) {
                    scaleStatic();                  // 代码方式：缩放动画
                } else {
                    scaleXml();                     // XML方式：缩放动画
                }
                break;
            // ... 其他按钮类似
        }
    }
    
    // XML方式：加载XML动画资源
    private void scaleXml() {
        int resourceId = R.anim.scale;
        xmlAnimation(resourceId);
    }
    
    // 通用XML动画加载方法
    private void xmlAnimation(int resourceId) {
        Animation animation = AnimationUtils.loadAnimation(this, resourceId);
        mImageView.startAnimation(animation);
    }
    
    // 代码方式：创建缩放动画
    private void scaleStatic() {
        Animation scaleAnimation = new ScaleAnimation(
            0f, 1f,                                // X轴起始和结束缩放比例
            0f, 1f,                                // Y轴起始和结束缩放比例
            Animation.RELATIVE_TO_SELF, 0.5f,      // X轴缩放中心点
            Animation.RELATIVE_TO_SELF, 0.5f);      // Y轴缩放中心点
        scaleAnimation.setDuration(3000);           // 动画持续时间3秒
        mImageView.startAnimation(scaleAnimation);
    }
    
    // 组合动画：使用AnimationSet
    private void combinedSetStatic() {
        AnimationSet animationSet = new AnimationSet(true);
        
        // 从XML加载各个动画
        Animation scaleAnimation = AnimationUtils.loadAnimation(this, R.anim.scale);
        Animation rotateAnimation = AnimationUtils.loadAnimation(this, R.anim.rotate);
        Animation translateAnimation = AnimationUtils.loadAnimation(this, R.anim.translate);
        Animation alphaAnimation = AnimationUtils.loadAnimation(this, R.anim.alpha);
        
        // 添加到动画集合
        animationSet.addAnimation(rotateAnimation);
        animationSet.addAnimation(translateAnimation);
        animationSet.addAnimation(alphaAnimation);
        animationSet.addAnimation(scaleAnimation);
        
        // 设置插值器
        animationSet.setInterpolator(this, android.R.anim.linear_interpolator);
        
        // 启动组合动画
        mImageView.startAnimation(animationSet);
    }
}
```

**动画机制解析：**

1. **Animation类体系**：
   - `AlphaAnimation`：透明度动画
   - `RotateAnimation`：旋转动画
   - `ScaleAnimation`：缩放动画
   - `TranslateAnimation`：平移动画
   - `AnimationSet`：动画集合，可组合多个动画

2. **两种创建方式**：
   - **代码方式**：使用Java代码创建Animation对象
   - **XML方式**：在res/anim/目录下定义XML，使用`AnimationUtils.loadAnimation()`加载

3. **AnimationUtils**：
   - `loadAnimation()`：从XML资源加载动画
   - 简化动画资源的加载过程

### 4.3 文件调用关系图

```
AndroidManifest.xml
    │
    └── MainActivity
          │
          ├── onCreate()
          │     │
          │     ├── setContentView() → main.xml
          │     │     │
          │     │     └── ImageView (动画目标视图)
          │     │
          │     └── 设置按钮监听器
          │
          ├── onClick() → 按钮点击处理
          │     │
          │     ├── scaleXml() / scaleStatic()
          │     │     │
          │     │     ├── XML方式：
          │     │     │     │
          │     │     │     └── AnimationUtils.loadAnimation()
          │     │     │           │
          │     │     │           └── R.anim.scale.xml
          │     │     │
          │     │     └── 代码方式：
          │     │           │
          │     │           └── new ScaleAnimation() → 创建动画对象
          │     │
          │     ├── rotateXml() / rotateStatic()
          │     │     └── R.anim.rotate.xml / RotateAnimation
          │     │
          │     ├── translateXml() / translateStatic()
          │     │     └── R.anim.translate.xml / TranslateAnimation
          │     │
          │     ├── alphaXml() / alphaStatic()
          │     │     └── R.anim.alpha.xml / AlphaAnimation
          │     │
          │     └── combinedSetStatic()
          │           │
          │           ├── 加载多个XML动画
          │           │
          │           └── AnimationSet → 组合动画
          │                 │
          │                 └── mImageView.startAnimation()
          │
          └── res/anim/
                ├── alpha.xml
                ├── rotate.xml
                ├── scale.xml
                ├── translate.xml
                └── combined.xml
```

### 4.4 动画执行流程图

```
用户点击按钮
  │
  ├─→ onClick()被调用
  │     │
  │     ├─→ 判断模式（STATICMODE / XMLMODE）
  │     │
  │     ├─→ XML模式
  │     │     │
  │     │     ├─→ 调用对应的xml方法（如scaleXml()）
  │     │     │     │
  │     │     │     └─→ xmlAnimation(resourceId)
  │     │     │           │
  │     │     │           ├─→ AnimationUtils.loadAnimation()
  │     │     │           │     │
  │     │     │           │     └─→ 解析XML文件（如scale.xml）
  │     │     │           │           │
  │     │     │           │           └─→ 创建Animation对象
  │     │     │           │
  │     │     │           └─→ mImageView.startAnimation(animation)
  │     │     │                 │
  │     │     │                 └─→ 动画开始执行
  │     │     │
  │     └─→ 代码模式
  │           │
  │           ├─→ 调用对应的static方法（如scaleStatic()）
  │           │     │
  │           │     ├─→ new ScaleAnimation(...)
  │           │     │     │
  │           │     │     └─→ 创建Animation对象
  │           │     │           │
  │           │     │           ├─→ 设置参数（起始值、结束值、中心点等）
  │           │     │           │
  │           │     │           └─→ setDuration(3000)
  │           │     │
  │           │     └─→ mImageView.startAnimation(animation)
  │           │           │
  │           │           └─→ 动画开始执行
  │
  └─→ 动画执行过程
        │
        ├─→ 根据插值器（Interpolator）计算每一帧的属性值
        │
        ├─→ 更新ImageView的属性（透明度、旋转角度、缩放比例、位置等）
        │
        └─→ 持续到动画结束
```

---

## 案例5：NoteApp（Android应用项目）

### 5.1 工程文件结构分析

```
NoteApp/
├── app/src/main/
│   ├── java/com/farsight/noteapp/
│   │   ├── MainActivity.java              # 主Activity（笔记列表）
│   │   ├── AddActivity.java               # 添加笔记Activity
│   │   ├── EditActivity.java              # 编辑笔记Activity
│   │   ├── NoteDbOpenHelper.java          # 数据库辅助类
│   │   ├── adapter/
│   │   │   └── MyAdapter.java             # RecyclerView适配器
│   │   ├── bean/
│   │   │   └── Note.java                  # 笔记数据模型
│   │   └── util/
│   │       ├── SpfUtil.java               # SharedPreferences工具类
│   │       └── ToastUtil.java             # Toast工具类
│   ├── res/
│   │   ├── layout/
│   │   │   ├── activity_main.xml          # 主界面布局
│   │   │   ├── activity_add.xml           # 添加界面布局
│   │   │   ├── activity_edit.xml          # 编辑界面布局
│   │   │   └── item_note.xml              # 列表项布局
│   │   ├── menu/
│   │   │   └── menu_main.xml               # 选项菜单
│   │   └── ...
│   └── AndroidManifest.xml
```

#### 5.1.1 AndroidManifest.xml分析

```xml
<application>
    <!-- 主Activity：笔记列表 -->
    <activity android:name=".MainActivity"
        android:label="记事列表"
        android:exported="true">
        <intent-filter>
            <action android:name="android.intent.action.MAIN" />
            <category android:name="android.intent.category.LAUNCHER" />
        </intent-filter>
    </activity>
    
    <!-- 添加笔记Activity -->
    <activity android:name=".AddActivity"
        android:label="添加记事"
        android:parentActivityName=".MainActivity" />
    
    <!-- 编辑笔记Activity -->
    <activity android:name=".EditActivity"
        android:label="修改记事"
        android:parentActivityName=".MainActivity" />
</application>
```

**关键点：**
- 三个Activity组成完整的应用
- `android:parentActivityName`：定义父Activity，用于导航返回
- 使用Intent在Activity间传递数据

### 5.2 核心代码剖析

#### 5.2.1 MainActivity.java（Activity + Intent + SQLite）

```java
public class MainActivity extends AppCompatActivity {
    private RecyclerView mRecyclerView;
    private FloatingActionButton mBtnAdd;
    private List<Note> mNotes;
    private MyAdapter mMyAdapter;
    private NoteDbOpenHelper mNoteDbOpenHelper;     // 数据库辅助类
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initView();
        initData();
        initEvent();
    }
    
    @Override
    protected void onResume() {
        super.onResume();
        // Activity恢复时刷新数据
        refreshDataFromDb();
        setListLayout();
    }
    
    private void initData() {
        mNotes = new ArrayList<>();
        mNoteDbOpenHelper = new NoteDbOpenHelper(this);
    }
    
    // 从数据库获取所有笔记
    private List<Note> getDataFromDB() {
        return mNoteDbOpenHelper.queryAllFromDb();
    }
    
    // 刷新数据
    private void refreshDataFromDb() {
        mNotes = getDataFromDB();
        mMyAdapter.refreshData(mNotes);
    }
    
    // 添加按钮点击：启动AddActivity
    public void add(View view) {
        Intent intent = new Intent(this, AddActivity.class);
        startActivity(intent);                       // 启动Activity
    }
    
    // 创建选项菜单
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_main, menu);
        
        // 搜索功能
        SearchView searchView = (SearchView) menu.findItem(R.id.menu_search)
            .getActionView();
        
        searchView.setOnQueryTextListener(new SearchView.OnQueryTextListener() {
            @Override
            public boolean onQueryTextChange(String newText) {
                // 根据标题搜索笔记
                mNotes = mNoteDbOpenHelper.queryFromDbByTitle(newText);
                mMyAdapter.refreshData(mNotes);
                return true;
            }
        });
        return super.onCreateOptionsMenu(menu);
    }
    
    // 菜单项选择处理
    @Override
    public boolean onOptionsItemSelected(@NonNull MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_linear:
                setToLinearList();                  // 设置为线性布局
                currentListLayoutMode = MODE_LINEAR;
                SpfUtil.saveInt(this, KEY_LAYOUT_MODE, MODE_LINEAR);
                return true;
            case R.id.menu_grid:
                setToGridList();                    // 设置为网格布局
                currentListLayoutMode = MODE_GRID;
                SpfUtil.saveInt(this, KEY_LAYOUT_MODE, MODE_GRID);
                return true;
        }
        return super.onOptionsItemSelected(item);
    }
}
```

**关键组件应用：**

1. **Activity生命周期**：
   - `onCreate()`：初始化
   - `onResume()`：每次显示时刷新数据

2. **Intent应用**：
   - `new Intent(this, AddActivity.class)`：显式Intent启动AddActivity
   - `startActivity(intent)`：启动Activity

3. **SQLite数据库**：
   - `NoteDbOpenHelper`：数据库辅助类
   - `queryAllFromDb()`：查询所有笔记
   - `queryFromDbByTitle()`：按标题搜索

#### 5.2.2 NoteDbOpenHelper.java（SQLite数据库）

```java
public class NoteDbOpenHelper extends SQLiteOpenHelper {
    private static final String DB_NAME = "noteSQLite.db";
    private static final String TABLE_NAME_NOTE = "note";
    
    // 创建表的SQL语句
    private static final String CREATE_TABLE_SQL = 
        "create table " + TABLE_NAME_NOTE 
        + " (id integer primary key autoincrement, "
        + "title text, content text, create_time text)";
    
    public NoteDbOpenHelper(Context context) {
        super(context, DB_NAME, null, 1);
    }
    
    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL(CREATE_TABLE_SQL);              // 创建表
    }
    
    // 插入数据
    public long insertData(Note note) {
        SQLiteDatabase db = getWritableDatabase();
        ContentValues values = new ContentValues();
        values.put("title", note.getTitle());
        values.put("content", note.getContent());
        values.put("create_time", note.getCreatedTime());
        return db.insert(TABLE_NAME_NOTE, null, values);
    }
    
    // 查询所有数据
    public List<Note> queryAllFromDb() {
        SQLiteDatabase db = getWritableDatabase();
        List<Note> noteList = new ArrayList<>();
        
        Cursor cursor = db.query(TABLE_NAME_NOTE, null, null, null, null, null, null);
        if (cursor != null) {
            while (cursor.moveToNext()) {
                Note note = new Note();
                note.setId(cursor.getString(cursor.getColumnIndex("id")));
                note.setTitle(cursor.getString(cursor.getColumnIndex("title")));
                note.setContent(cursor.getString(cursor.getColumnIndex("content")));
                note.setCreatedTime(cursor.getString(cursor.getColumnIndex("create_time")));
                noteList.add(note);
            }
            cursor.close();
        }
        return noteList;
    }
    
    // 按标题搜索
    public List<Note> queryFromDbByTitle(String title) {
        if (TextUtils.isEmpty(title)) {
            return queryAllFromDb();
        }
        SQLiteDatabase db = getWritableDatabase();
        // 使用LIKE进行模糊查询
        Cursor cursor = db.query(TABLE_NAME_NOTE, null, 
            "title like ?", new String[]{"%"+title+"%"}, null, null, null);
        // ... 处理结果
    }
}
```

#### 5.2.3 AddActivity.java（Intent返回）

```java
public class AddActivity extends AppCompatActivity {
    private EditText etTitle, etContent;
    private NoteDbOpenHelper mNoteDbOpenHelper;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add);
        mNoteDbOpenHelper = new NoteDbOpenHelper(this);
    }
    
    // 添加笔记
    public void add(View view) {
        String title = etTitle.getText().toString();
        String content = etContent.getText().toString();
        
        if (TextUtils.isEmpty(title)) {
            ToastUtil.toastShort(this, "标题不能为空！");
            return;
        }
        
        Note note = new Note();
        note.setTitle(title);
        note.setContent(content);
        note.setCreatedTime(getCurrentTimeFormat());
        
        // 插入数据库
        long row = mNoteDbOpenHelper.insertData(note);
        if (row != -1) {
            ToastUtil.toastShort(this, "添加成功！");
            this.finish();                          // 关闭Activity，返回MainActivity
        }
    }
}
```

### 5.3 文件调用关系图

```
AndroidManifest.xml
    │
    ├── MainActivity (主Activity)
    │     │
    │     ├── onCreate()
    │     │     │
    │     │     ├── initView() → activity_main.xml
    │     │     │
    │     │     ├── initData()
    │     │     │     │
    │     │     │     └── NoteDbOpenHelper → 创建数据库辅助类
    │     │     │
    │     │     └── initEvent()
    │     │           │
    │     │           └── RecyclerView + MyAdapter
    │     │
    │     ├── onResume()
    │     │     │
    │     │     ├── refreshDataFromDb()
    │     │     │     │
    │     │     │     └── NoteDbOpenHelper.queryAllFromDb()
    │     │     │           │
    │     │     │           └── SQLiteDatabase.query()
    │     │     │                 │
    │     │     │                 └── Cursor → 转换为List<Note>
    │     │     │
    │     │     └── mMyAdapter.refreshData()
    │     │
    │     ├── add() → 添加按钮点击
    │     │     │
    │     │     └── Intent → AddActivity
    │     │           │
    │     │           └── startActivity()
    │     │
    │     └── onCreateOptionsMenu()
    │           │
    │           └── menu_main.xml → 选项菜单
    │                 │
    │                 ├── SearchView → 搜索功能
    │                 │     │
    │                 │     └── queryFromDbByTitle()
    │                 │
    │                 └── 布局切换（线性/网格）
    │
    ├── AddActivity
    │     │
    │     ├── onCreate() → activity_add.xml
    │     │
    │     └── add() → 添加笔记
    │           │
    │           ├── NoteDbOpenHelper.insertData()
    │           │
    │           └── finish() → 返回MainActivity
    │
    ├── EditActivity
    │     │
    │     └── 编辑笔记功能（类似AddActivity）
    │
    └── NoteDbOpenHelper (SQLiteOpenHelper)
          │
          ├── onCreate() → 创建note表
          │
          ├── insertData() → 插入数据
          │
          ├── queryAllFromDb() → 查询所有
          │
          └── queryFromDbByTitle() → 按标题搜索
```

### 5.4 应用流程图

```
应用启动
  │
  ├─→ MainActivity.onCreate()
  │     │
  │     ├─→ 初始化数据库（NoteDbOpenHelper）
  │     │     │
  │     │     └─→ 如果数据库不存在，创建note表
  │     │
  │     ├─→ 初始化RecyclerView和适配器
  │     │
  │     └─→ 加载数据
  │           │
  │           └─→ queryAllFromDb() → 显示笔记列表
  │
  ├─→ MainActivity.onResume()
  │     │
  │     └─→ 刷新数据（从数据库重新加载）
  │
  ├─→ 用户操作
  │     │
  │     ├─→ 点击添加按钮（FloatingActionButton）
  │     │     │
  │     │     └─→ Intent → AddActivity
  │     │           │
  │     │           └─→ AddActivity.onCreate()
  │     │                 │
  │     │                 └─→ 显示添加界面
  │     │                       │
  │     │                       └─→ 用户输入标题和内容
  │     │                             │
  │     │                             └─→ 点击保存
  │     │                                   │
  │     │                                   ├─→ insertData() → 插入数据库
  │     │                                   │
  │     │                                   └─→ finish() → 返回MainActivity
  │     │                                         │
  │     │                                         └─→ MainActivity.onResume()
  │     │                                               │
  │     │                                               └─→ 刷新列表
  │     │
  │     ├─→ 搜索功能（SearchView）
  │     │     │
  │     │     └─→ queryFromDbByTitle() → 模糊查询
  │     │           │
  │     │           └─→ 更新列表显示
  │     │
  │     └─→ 切换布局（菜单）
  │           │
  │           ├─→ 线性布局
  │           │
  │           └─→ 网格布局
  │
  └─→ 数据持久化
        │
        └─→ SQLite数据库存储
```

---

## 第二次Android实验案例分析

### 案例选择
分析以下4个GPIO外设控制案例：
- LED灯控制
- 蜂鸣器控制
- 温度采集
- 串口通信

---

## 案例1：LED灯控制

### 1.1 工程文件结构分析

```
LED/
├── app/src/main/
│   ├── java/com/farsight/led/
│   │   ├── MainActivity.java              # 主Activity
│   │   └── LED.java                       # JNI接口类
│   ├── cpp/
│   │   └── native-lib.cpp                 # JNI实现（C++）
│   ├── res/
│   │   └── layout/
│   │       └── activity_main.xml          # UI布局
│   └── AndroidManifest.xml
```

### 1.2 核心代码剖析

#### 1.2.1 MainActivity.java（Activity应用）

```java
public class MainActivity extends AppCompatActivity {
    LED led = new LED();                    // LED驱动类实例
    boolean IsLight_1_On = false;           // LED1状态标志
    boolean IsLight_2_On = false;          // LED2状态标志
    private ImageButton LedButton_1;
    private ImageButton LedButton_2;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        LedButton_1 = (ImageButton)findViewById(R.id.buttonOne);
        
        // LED1按钮点击事件
        LedButton_1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (IsLight_1_On) {
                    // 关闭LED1
                    ((ImageButton)view).setBackground(getDrawable(R.drawable.pic_bulboff));
                    led.open();             // 打开设备文件
                    led.LedOff1();          // 调用JNI方法关闭LED1
                    led.close();            // 关闭设备文件
                } else {
                    // 打开LED1
                    ((ImageButton)view).setBackground(getDrawable(R.drawable.pic_bulbon));
                    led.open();
                    led.LedOn1();           // 调用JNI方法打开LED1
                    led.close();
                }
                IsLight_1_On = !IsLight_1_On;  // 切换状态
            }
        });
        
        // LED2按钮类似处理
        // ...
    }
}
```

**关键点：**
- Activity负责UI交互
- 通过LED类调用JNI方法控制硬件
- 每次操作都需要open() → 控制 → close()

#### 1.2.2 LED.java（JNI接口）

```java
public class LED {
    static {
        System.loadLibrary("led");          // 加载JNI库（libled.so）
    }
    
    // JNI方法声明（native关键字）
    public native int open();               // 打开设备文件
    public native int close();              // 关闭设备文件
    public native int LedOn1();             // 打开LED1
    public native int LedOff1();            // 关闭LED1
    public native int LedOn2();             // 打开LED2
    public native int LedOff2();            // 关闭LED2
}
```

**JNI机制：**
- `static{}`：类加载时执行，加载本地库
- `native`：声明本地方法，实现在C/C++中
- 库名"led"对应`libled.so`文件

#### 1.2.3 native-lib.cpp（JNI实现 - GPIO驱动）

```cpp
#include <jni.h>
#include <fcntl.h>
#include <sys/ioctl.h>
#include <unistd.h>

// IO控制命令定义（与Linux驱动中的定义一致）
#define LED1_ON    _IO('x',1)              // LED1打开命令
#define LED1_OFF   _IO('x',0)              // LED1关闭命令
#define LED2_ON    _IO('z',1)              // LED2打开命令
#define LED2_OFF   _IO('z',0)              // LED2关闭命令

int fd = 0;                                // 设备文件描述符

// JNI函数：打开LED设备
extern "C" JNIEXPORT jint JNICALL
Java_com_farsight_led_LED_open(JNIEnv* env, jobject /* this */) {
    // 打开Linux设备文件（字符设备驱动）
    fd = open("/dev/leds_ctl", O_RDWR);
    if (-1 == fd) {
        __android_log_print(ANDROID_LOG_INFO, "led", "open /dev/leds_ctl Error");
    } else {
        __android_log_print(ANDROID_LOG_INFO, "led", "open /dev/leds_ctl success");
    }
    return fd;
}

// JNI函数：打开LED1
extern "C" JNIEXPORT jint JNICALL
Java_com_farsight_led_LED_LedOn1(JNIEnv* env, jobject /* this */) {
    // 使用ioctl系统调用控制设备
    ioctl(fd, LED1_ON);                    // 发送LED1_ON命令到驱动
    return 0;
}

// JNI函数：关闭LED1
extern "C" JNIEXPORT jint JNICALL
Java_com_farsight_led_LED_LedOff1(JNIEnv* env, jobject /* this */) {
    ioctl(fd, LED1_OFF);                   // 发送LED1_OFF命令到驱动
    return 0;
}

// LED2类似实现
// ...
```

**GPIO驱动关键点：**

1. **设备文件操作**：
   - `/dev/leds_ctl`：Linux字符设备文件
   - `open()`：打开设备文件，返回文件描述符
   - `close()`：关闭设备文件

2. **ioctl系统调用**：
   - `ioctl(fd, cmd)`：向设备发送控制命令
   - `LED1_ON/OFF`：自定义IO控制命令
   - 驱动根据命令控制GPIO引脚

3. **JNI函数命名规则**：
   - `Java_包名_类名_方法名`
   - 例如：`Java_com_farsight_led_LED_open`

### 1.3 GPIO驱动流程图

```
用户点击LED按钮
  │
  ├─→ MainActivity.onClick()
  │     │
  │     ├─→ 更新UI（切换按钮图标）
  │     │
  │     └─→ led.open()
  │           │
  │           └─→ JNI调用
  │                 │
  │                 └─→ native-lib.cpp::Java_com_farsight_led_LED_open()
  │                       │
  │                       └─→ open("/dev/leds_ctl", O_RDWR)
  │                             │
  │                             └─→ Linux系统调用
  │                                   │
  │                                   └─→ 打开字符设备文件
  │                                         │
  │                                         └─→ 返回文件描述符fd
  │
  ├─→ led.LedOn1() / led.LedOff1()
  │     │
  │     └─→ JNI调用
  │           │
  │           └─→ native-lib.cpp::Java_com_farsight_led_LED_LedOn1()
  │                 │
  │                 └─→ ioctl(fd, LED1_ON)
  │                       │
  │                       └─→ Linux系统调用
  │                             │
  │                             └─→ 内核空间
  │                                   │
  │                                   └─→ LED字符设备驱动
  │                                         │
  │                                         ├─→ 解析ioctl命令
  │                                         │
  │                                         ├─→ 控制GPIO寄存器
  │                                         │     │
  │                                         │     └─→ 设置GPIO引脚电平
  │                                         │           │
  │                                         │           └─→ LED硬件状态改变
  │                                         │
  │                                         └─→ 返回结果
  │
  └─→ led.close()
        │
        └─→ close(fd) → 关闭设备文件
```

### 1.4 工程文件调用关系

```
AndroidManifest.xml
    │
    └── MainActivity
          │
          ├── onCreate()
          │     │
          │     └── setContentView() → activity_main.xml
          │
          ├── 按钮点击事件
          │     │
          │     └─→ LED类方法调用
          │           │
          │           ├─→ LED.open()
          │           │     │
          │           │     └─→ JNI → native-lib.cpp
          │           │           │
          │           │           └─→ open("/dev/leds_ctl")
          │           │                 │
          │           │                 └─→ Linux字符设备驱动
          │           │
          │           ├─→ LED.LedOn1() / LedOff1()
          │           │     │
          │           │     └─→ JNI → native-lib.cpp
          │           │           │
          │           │           └─→ ioctl(fd, LED1_ON/OFF)
          │           │                 │
          │           │                 └─→ Linux驱动 → GPIO控制
          │           │
          │           └─→ LED.close()
          │                 │
          │                 └─→ JNI → native-lib.cpp
          │                       │
          │                       └─→ close(fd)
          │
          └── libled.so (JNI库)
                │
                └─→ 编译自native-lib.cpp
```

---

## 案例2：蜂鸣器控制

### 2.1 核心代码剖析

#### 2.1.1 MainActivity.java

```java
public class MainActivity extends AppCompatActivity implements View.OnClickListener {
    Buzzer buzzer = new Buzzer();
    private Button start;
    private Button stop;
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        start = (Button) findViewById(R.id.button1);
        stop = (Button) findViewById(R.id.button2);
        start.setOnClickListener(this);
        stop.setOnClickListener(this);
    }
    
    @Override
    public void onClick(View view) {
        switch (view.getId()) {
            case R.id.button1:
                // 打开设备
                if(buzzer.open() == -1) {
                    Toast.makeText(this, "设备打开失败！", Toast.LENGTH_SH_SHORT).show();
                    return;
                }
                buzzer.BuzzerOn();          // 打开蜂鸣器
                buzzer.close();
                break;
                
            case R.id.button2:
                if(buzzer.open() == -1) {
                    Toast.makeText(this, "设备打开失败！", Toast.LENGTH_SHORT).show();
                    return;
                }
                buzzer.BuzzerOff();         // 关闭蜂鸣器
                buzzer.close();
                break;
        }
    }
}
```

#### 2.1.2 Buzzer.java（JNI接口）

```java
public class Buzzer {
    static {
        System.loadLibrary("buzzer");       // 加载libbuzzer.so
    }
    public native int open();
    public native int close();
    public native int BuzzerOn();           // 打开蜂鸣器
    public native int BuzzerOff();         // 关闭蜂鸣器
}
```

**与LED类似，通过JNI调用C++代码控制GPIO**

---

## 案例3：温度采集

### 3.1 核心代码剖析

#### 3.1.1 MainActivity.java（多线程应用）

```java
public class MainActivity extends AppCompatActivity {
    TextView val;                           // 温度值显示
    Button start_btn;
    Button close_btn;
    Boolean sensorflag = false;             // 传感器状态标志
    double data = 0;
    temp tmp = new temp();                  // 温度传感器驱动类
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        val = findViewById(R.id.text);
        
        start_btn = findViewById(R.id.start_btn);
        start_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (sensorflag == false) {
                    sensorflag = true;
                    tmp.open();             // 打开温度传感器设备
                    new TimeThread().start(); // 启动数据采集线程
                }
            }
        });
        
        close_btn = findViewById(R.id.close_btn);
        close_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (sensorflag == true) {
                    tmp.close();            // 关闭设备
                    sensorflag = false;
                }
            }
        });
    }
    
    // 数据采集线程：每隔200ms读取一次温度
    public class TimeThread extends Thread {
        @Override
        public void run() {
            super.run();
            do {
                if (sensorflag == true) {
                    try {
                        Thread.sleep(200);  // 休眠200ms
                        Message msg = new Message();
                        msg.what = 1;
                        handler.sendMessage(msg);  // 发送消息到主线程
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                } else {
                    try {
                        Thread.sleep(200);
                        Message msg = new Message();
                        msg.what = 2;
                        handler.sendMessage(msg);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }
                }
            } while (true);
        }
    }
    
    // Handler：在主线程中更新UI
    private Handler handler = new Handler(new Handler.Callback() {
        @Override
        public boolean handleMessage(Message msg) {
            switch (msg.what) {
                case 1:
                    data = tmp.read();      // 读取温度值（JNI调用）
                    val.setText(String.format("%.3f", data) + "°C");
                    break;
                case 2:
                    val.setText("传感器未打开");
                    break;
            }
            return false;
        }
    });
}
```

**关键机制：**

1. **多线程**：
   - `TimeThread`：后台线程，定期读取传感器数据
   - 避免阻塞主线程（UI线程）

2. **Handler机制**：
   - 子线程不能直接更新UI
   - 通过Handler将数据传递到主线程更新UI

3. **持续采集**：
   - 使用`do-while(true)`循环
   - 每隔200ms读取一次温度

#### 3.1.2 temp.java（JNI接口）

```java
public class temp {
    static {
        System.loadLibrary("temperature");  // 加载libtemperature.so
    }
    public native int open();               // 打开温度传感器设备
    public native double read();            // 读取温度值
    public native int close();               // 关闭设备
}
```

**与LED/Buzzer的区别：**
- `read()`返回`double`类型（温度值）
- 需要持续读取，不是一次性控制

### 3.2 温度采集流程图

```
用户点击开始按钮
  │
  ├─→ tmp.open() → 打开温度传感器设备
  │
  ├─→ 启动TimeThread线程
  │     │
  │     └─→ TimeThread.run()
  │           │
  │           └─→ 循环执行
  │                 │
  │                 ├─→ Thread.sleep(200ms)
  │                 │
  │                 ├─→ handler.sendMessage(msg)
  │                 │     │
  │                 │     └─→ 发送消息到主线程
  │                 │
  │                 └─→ 继续循环
  │
  ├─→ Handler.handleMessage()
  │     │
  │     ├─→ msg.what == 1?
  │     │     │
  │     │     ├─→ 是：
  │     │     │     │
  │     │     │     ├─→ tmp.read() → JNI调用
  │     │     │     │     │
  │     │     │     │     └─→ native-lib.cpp
  │     │     │     │           │
  │     │     │     │           └─→ 读取温度传感器数据
  │     │     │     │                 │
  │     │     │     │                 └─→ 返回温度值（double）
  │     │     │     │
  │     │     │     └─→ val.setText() → 更新UI显示
  │     │     │
  │     │     └─→ 否：显示"传感器未打开"
  │     │
  │     └─→ 返回
  │
  └─→ 用户点击关闭按钮
        │
        └─→ tmp.close() → 关闭设备
              │
              └─→ sensorflag = false → 停止采集
```

---

## 案例4：串口通信

### 4.1 核心代码剖析

#### 4.1.1 MainActivity.java（串口通信）

```java
public class MainActivity extends AppCompatActivity implements OnClickListener {
    serial com = new serial();              // 串口驱动类
    private EditText ET1;                   // 发送数据输入框
    private Button SEND;
    private Button OPENSERIAL;
    private Button CLOSESERIAL;
    private TextView msglist;               // 消息显示列表
    private Spinner spinner;                // 串口选择
    private Spinner spinner2;               // 波特率选择
    private int serial;                     // 选中的串口索引
    private int Baudrate;                   // 选中的波特率索引
    
    MyThread myThread = null;               // 接收数据线程
    
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        // 初始化UI组件
        ET1 = findViewById(R.id.edit1);
        SEND = findViewById(R.id.send1);
        OPENSERIAL = findViewById(R.id.open_serial);
        CLOSESERIAL = findViewById(R.id.close_serial);
        msglist = findViewById(R.id.msglist);
        
        // 串口选择下拉框
        spinner = (Spinner) findViewById(R.id.spinner);
        data_list = new ArrayList<String>();
        data_list.add("/dev/ttyS0");
        data_list.add("/dev/ttyS4");
        arr_adapter = new ArrayAdapter<String>(this, 
            android.R.layout.simple_spinner_item, data_list);
        spinner.setAdapter(arr_adapter);
        
        // 波特率选择下拉框
        spinner2 = (Spinner) findViewById(R.id.spinner2);
        data_list2 = new ArrayList<String>();
        data_list2.add("2400");
        data_list2.add("4800");
        data_list2.add("9600");
        data_list2.add("19200");
        data_list2.add("38400");
        data_list2.add("57600");
        data_list2.add("115200");
        // ... 设置适配器
    }
    
    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.send1:
                // 发送数据
                String m = ET1.getText().toString() + "\n";
                com.write(m.getBytes());     // 写入串口
                msglist.append("[发送]" + m);
                ET1.setText("");
                break;
                
            case R.id.open_serial:
                // 打开串口
                int ret = com.open(serial, Baudrate);  // 传入串口和波特率
                if (ret > 0) {
                    Toast.makeText(MainActivity.this, "串口打开成功", Toast.LENGTH_LONG).show();
                    OPENSERIAL.setEnabled(false);
                    CLOSESERIAL.setEnabled(true);
                    SEND.setEnabled(true);
                    // 启动接收线程
                    myThread = new MyThread();
                    myThread.start();
                }
                break;
                
            case R.id.close_serial:
                // 关闭串口
                if (myThread != null) {
                    com.close();
                    myThread = null;
                }
                SEND.setEnabled(false);
                break;
        }
    }
    
    // 接收数据线程
    class MyThread extends Thread {
        @Override
        public void run() {
            while (true) {
                byte[] bytes = com.read();  // 读取串口数据
                if(bytes == null) {
                    break;                  // 读取失败，退出循环
                }
                String string = new String(bytes);
                String finalString = string;
                // 在主线程中更新UI
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        msglist.append("[接收]" + finalString + "\n");
                    }
                });
            }
            // 串口关闭后的UI更新
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    OPENSERIAL.setEnabled(true);
                    CLOSESERIAL.setEnabled(false);
                }
            });
        }
    }
}
```

**串口通信关键点：**

1. **串口参数配置**：
   - 串口设备：`/dev/ttyS0`或`/dev/ttyS4`
   - 波特率：2400-115200

2. **双向通信**：
   - `write()`：发送数据
   - `read()`：接收数据（阻塞读取）

3. **多线程处理**：
   - `MyThread`：后台线程持续读取串口数据
   - `runOnUiThread()`：在主线程中更新UI

#### 4.1.2 serial.java（JNI接口）

```java
public class serial {
    static {
        System.loadLibrary("serial");       // 加载libserial.so
    }
    // Port是串口选择索引，Rate是波特率选择索引
    public native int open(int Port, int Rate);
    public native int close();
    public native byte[] read();             // 读取数据，返回字节数组
    public native int write(byte[] buffer);  // 写入数据
}
```

### 4.2 串口通信流程图

```
应用启动
  │
  ├─→ 初始化UI（串口选择、波特率选择）
  │
  ├─→ 用户选择串口和波特率
  │
  ├─→ 用户点击"打开串口"
  │     │
  │     └─→ com.open(serial, Baudrate)
  │           │
  │           └─→ JNI → native-lib.cpp
  │                 │
  │                 └─→ 打开串口设备文件
  │                       │
  │                       └─→ 配置串口参数（波特率、数据位、停止位等）
  │
  ├─→ 启动MyThread接收线程
  │     │
  │     └─→ MyThread.run()
  │           │
  │           └─→ 循环读取
  │                 │
  │                 ├─→ com.read() → JNI调用
  │                 │     │
  │                 │     └─→ 阻塞读取串口数据
  │                 │           │
  │                 │           └─→ 返回byte[]
  │                 │
  │                 ├─→ runOnUiThread()
  │                 │     │
  │                 │     └─→ 更新UI显示接收的数据
  │                 │
  │                 └─→ 继续循环
  │
  ├─→ 用户输入数据并点击"发送"
  │     │
  │     └─→ com.write(m.getBytes())
  │           │
  │           └─→ JNI → native-lib.cpp
  │                 │
  │                 └─→ write()系统调用
  │                       │
  │                       └─→ 数据写入串口设备
  │                             │
  │                             └─→ 通过串口硬件发送
  │
  └─→ 用户点击"关闭串口"
        │
        └─→ com.close()
              │
              └─→ 关闭串口设备
                    │
                    └─→ MyThread检测到读取失败，退出循环
```

---

## Android与Linux设备驱动对比分析

### 对比案例：LED灯控制

#### Android方式（本实验）

**架构层次：**
```
应用层（Java）
    │
    ├─→ MainActivity.java (Activity)
    │     │
    │     └─→ LED.java (JNI接口)
    │           │
    │           └─→ JNI桥接
    │                 │
    └─→ native-lib.cpp (JNI实现)
          │
          └─→ Linux系统调用
                │
                ├─→ open("/dev/leds_ctl", O_RDWR)
                │
                └─→ ioctl(fd, LED1_ON/OFF)
                      │
                      └─→ 内核空间
                            │
                            └─→ LED字符设备驱动
                                  │
                                  └─→ GPIO控制
```

**特点：**
1. **多层架构**：Java应用 → JNI → C++ → Linux系统调用 → 驱动
2. **设备文件抽象**：通过`/dev/leds_ctl`字符设备文件操作
3. **ioctl控制**：使用`ioctl()`系统调用发送控制命令
4. **应用层隔离**：Java应用不直接访问硬件，通过系统调用间接访问

#### Linux原生方式（传统驱动开发）

**架构层次：**
```
应用层（C/C++）
    │
    └─→ LED控制程序 (C程序)
          │
          └─→ Linux系统调用
                │
                ├─→ open("/dev/leds", O_RDWR)
                │
                └─→ ioctl(fd, LED_ON/OFF)
                      │
                      └─→ 内核空间
                            │
                            └─→ LED字符设备驱动
                                  │
                                  ├─→ file_operations结构体
                                  │     │
                                  │     ├─→ led_open()
                                  │     ├─→ led_ioctl()
                                  │     └─→ led_release()
                                  │
                                  └─→ GPIO操作函数
                                        │
                                        ├─→ gpio_request()
                                        ├─→ gpio_direction_output()
                                        └─→ gpio_set_value()
```

**驱动代码示例（Linux内核模块）：**

```c
#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/device.h>
#include <linux/gpio.h>
#include <linux/uaccess.h>

#define LED_MAJOR 200
#define LED_ON  1
#define LED_OFF 0

static int led_gpio = 60;  // GPIO引脚号

// 设备文件操作结构体
static struct file_operations led_fops = {
    .owner = THIS_MODULE,
    .open = led_open,
    .release = led_release,
    .unlocked_ioctl = led_ioctl,
};

// 打开设备
static int led_open(struct inode *inode, struct file *file) {
    // 申请GPIO
    gpio_request(led_gpio, "led");
    // 设置为输出模式
    gpio_direction_output(led_gpio, 0);
    return 0;
}

// IO控制
static long led_ioctl(struct file *file, unsigned int cmd, unsigned long arg) {
    switch(cmd) {
        case LED_ON:
            gpio_set_value(led_gpio, 1);  // 设置GPIO为高电平
            break;
        case LED_OFF:
            gpio_set_value(led_gpio, 0);  // 设置GPIO为低电平
            break;
    }
    return 0;
}

// 关闭设备
static int led_release(struct inode *inode, struct file *file) {
    gpio_free(led_gpio);  // 释放GPIO
    return 0;
}
```

### 对比分析表

| 对比项 | Android方式 | Linux原生方式 |
|--------|------------|--------------|
| **开发语言** | Java + JNI + C++ | C/C++ |
| **应用层** | Android Activity | Linux用户空间程序 |
| **接口层** | JNI桥接 | 直接系统调用 |
| **系统调用** | 通过JNI调用 | 直接调用 |
| **设备文件** | `/dev/leds_ctl` | `/dev/leds` |
| **控制方式** | ioctl() | ioctl() |
| **驱动层** | Linux字符设备驱动 | Linux字符设备驱动 |
| **GPIO操作** | 驱动内部实现 | 驱动内部实现 |
| **代码复杂度** | 较高（多层） | 较低（直接） |
| **可移植性** | Android平台 | Linux平台 |
| **安全性** | 应用层隔离 | 需要权限管理 |

### 关键差异分析

#### 1. **架构差异**

**Android方式：**
- 需要JNI层作为Java和C++的桥梁
- 应用层使用Java，底层使用C++
- 通过JNI函数映射调用本地代码

**Linux方式：**
- 应用层直接使用C/C++
- 直接调用系统调用，无需中间层
- 代码更简洁直接

#### 2. **开发流程差异**

**Android方式：**
```
1. 编写Java应用（MainActivity）
2. 定义JNI接口类（LED.java）
3. 实现JNI C++代码（native-lib.cpp）
4. 编译生成JNI库（libled.so）
5. 打包到APK
```

**Linux方式：**
```
1. 编写内核驱动模块（led_driver.c）
2. 编写应用层程序（led_app.c）
3. 编译驱动模块（.ko文件）
4. 加载驱动模块（insmod）
5. 运行应用程序
```

#### 3. **GPIO控制流程对比**

**Android流程：**
```
Java代码 → JNI调用 → C++代码 → open() → ioctl() → 驱动 → GPIO
```

**Linux流程：**
```
C代码 → open() → ioctl() → 驱动 → GPIO
```

#### 4. **优势对比**

**Android优势：**
- 丰富的UI框架和组件
- 跨平台应用开发
- 完善的开发工具链
- 应用层代码易于维护

**Linux优势：**
- 代码简洁，性能更高
- 直接系统调用，延迟更低
- 适合嵌入式系统开发
- 资源占用更少

### 总结

Android和Linux在设备驱动控制上的本质是相同的：
1. **都使用Linux内核驱动**：底层都是Linux字符设备驱动
2. **都通过设备文件操作**：使用`open()`、`ioctl()`、`close()`
3. **GPIO控制方式相同**：驱动内部使用GPIO API控制硬件

**主要区别在于应用层：**
- Android需要通过JNI桥接Java和C++
- Linux可以直接使用C/C++调用系统调用
- Android更适合移动应用开发，Linux更适合嵌入式系统开发

---

## 总结

本报告详细分析了：

1. **第一次Android实验的5个案例**：
   - ActionBarDemo：Activity和Handler机制
   - ActivityCommunication：Intent和Activity间通信
   - SQLiteExam2：SQLite数据库操作
   - GraphicAnimation：动画机制
   - NoteApp：完整的Android应用（Activity + Intent + SQLite）

2. **第二次Android实验的4个GPIO控制案例**：
   - LED灯：通过JNI控制GPIO
   - 蜂鸣器：类似LED的控制方式
   - 温度采集：持续读取传感器数据
   - 串口通信：双向数据传输

3. **Android与Linux设备驱动对比**：
   - 架构差异：Android需要JNI层，Linux直接调用
   - 开发流程：Android更复杂但功能丰富，Linux更简洁
   - 本质相同：都使用Linux内核驱动和系统调用

通过以上分析，深入理解了Android应用开发的四大组件（Activity、Intent、Content Provider、Service）的实际应用，以及Android平台如何通过JNI机制访问Linux底层硬件资源。


