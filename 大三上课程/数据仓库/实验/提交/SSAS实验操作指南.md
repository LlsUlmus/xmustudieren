# SSAS多维建模实验操作指南

## 实验环境准备

### 1. 软件要求
- SQL Server 2019 (包含Analysis Services)
- SQL Server Data Tools (SSDT) 2019或更高版本
- AdventureWorksDW2019示例数据库

### 2. 环境检查
```sql
-- 检查SQL Server服务状态
SELECT SERVERPROPERTY('ProductVersion') as SQLServerVersion;
SELECT SERVERPROPERTY('Edition') as SQLServerEdition;

-- 检查Analysis Services服务
-- 在SQL Server Configuration Manager中确认SSAS服务正在运行
```

## 第1课：定义数据源视图

### 步骤1：创建Analysis Services项目

1. **启动SSDT**
   - 打开SQL Server Data Tools
   - 选择"文件" → "新建" → "项目"

2. **选择项目模板**
   ```
   项目类型：商业智能
   模板：Analysis Services多维和数据挖掘项目
   项目名称：Adventure Works Tutorial
   位置：选择合适的保存路径
   ```

3. **项目结构**
   创建后的项目包含以下文件夹：
   - 数据源 (Data Sources)
   - 数据源视图 (Data Source Views)
   - 多维数据集 (Cubes)
   - 维度 (Dimensions)
   - 挖掘结构 (Mining Structures)
   - 角色 (Roles)
   - 程序集 (Assemblies)

### 步骤2：定义数据源

1. **创建数据源连接**
   - 右键单击"数据源"文件夹
   - 选择"新建数据源"
   - 在数据源向导中点击"新建"

2. **配置连接管理器**
   ```
   服务器名称：localhost 或 (local)
   身份验证：Windows身份验证
   数据库：AdventureWorksDW2019
   ```

3. **连接测试**
   - 点击"测试连接"按钮
   - 确认连接成功
   - 完成数据源创建

### 步骤3：定义数据源视图

1. **启动数据源视图向导**
   - 右键单击"数据源视图"文件夹
   - 选择"新建数据源视图"

2. **选择数据源**
   - 选择刚创建的AdventureWorksDW2019数据源
   - 点击"下一步"

3. **选择表和视图**
   选择以下核心表：
   ```
   FactInternetSales (事实表)
   DimProduct (产品维度)
   DimDate (日期维度)
   DimCustomer (客户维度)
   DimGeography (地理维度)
   DimSalesTerritory (销售区域维度)
   DimProductCategory (产品类别维度)
   DimProductSubcategory (产品子类别维度)
   ```

4. **完成数据源视图创建**
   - 检查表间关系
   - 完成向导

### 步骤4：修改表名

1. **重命名表**
   在数据源视图设计器中：
   ```
   FactInternetSales → Internet Sales
   DimProduct → Product
   DimDate → Date
   DimCustomer → Customer
   DimGeography → Geography
   DimSalesTerritory → Sales Territory
   DimProductCategory → Product Category
   DimProductSubcategory → Product Subcategory
   ```

2. **验证关系**
   - 检查表间关系是否正确
   - 确保外键关系完整

## 第2课：定义和部署多维数据集

### 步骤1：创建多维数据集

1. **启动多维数据集向导**
   - 右键单击"多维数据集"文件夹
   - 选择"新建多维数据集"

2. **选择创建方法**
   - 选择"使用现有表"
   - 点击"下一步"

3. **选择事实表**
   - 选择"Internet Sales"表
   - 选择度量值组：Internet Sales
   - 选择度量值：
     ```
     Sales Amount (销售额)
     Order Quantity (订单数量)
     Unit Price (单价)
     Extended Amount (扩展金额)
     Discount Amount (折扣金额)
     ```

4. **选择维度**
   选择以下维度：
   ```
   Product (产品)
   Date (日期)
   Customer (客户)
   Sales Territory (销售区域)
   ```

5. **完成多维数据集创建**
   - 检查多维数据集结构
   - 完成向导

### 步骤2：配置项目属性

1. **设置部署属性**
   - 右键单击项目名称
   - 选择"属性"
   - 在"部署"选项卡中设置：
     ```
     服务器：localhost
     数据库：Adventure Works Tutorial
     ```

2. **验证配置**
   - 检查所有设置正确
   - 保存项目

### 步骤3：部署多维数据集

1. **部署项目**
   - 右键单击项目名称
   - 选择"部署"
   - 查看部署进度

2. **验证部署**
   - 检查部署是否成功
   - 在SQL Server Management Studio中连接Analysis Services
   - 验证多维数据集是否存在

## 第3课：修改度量值、属性和层次结构

### 步骤1：修改度量值

1. **打开多维数据集设计器**
   - 双击多维数据集
   - 选择"度量值"选项卡

2. **修改度量值属性**
   ```
   Sales Amount:
   - 名称：销售额
   - 格式：货币
   - 聚合函数：Sum
   
   Order Quantity:
   - 名称：订单数量
   - 格式：数字
   - 聚合函数：Sum
   
   Unit Price:
   - 名称：单价
   - 格式：货币
   - 聚合函数：Average
   ```

### 步骤2：修改维度属性

1. **打开维度设计器**
   - 双击Product维度
   - 选择"属性"选项卡

2. **修改属性设置**
   ```
   ProductKey:
   - 名称：产品ID
   - 键列：ProductKey
   - 名称列：ProductKey
   
   ProductName:
   - 名称：产品名称
   - 键列：ProductKey
   - 名称列：EnglishProductName
   
   Color:
   - 名称：颜色
   - 键列：Color
   - 名称列：Color
   ```

### 步骤3：创建层次结构

1. **产品层次结构**
   - 在Product维度设计器中
   - 选择"层次结构"选项卡
   - 创建层次结构：
     ```
     产品类别 → 产品子类别 → 产品名称
     ```

2. **时间层次结构**
   - 在Date维度设计器中
   - 创建层次结构：
     ```
     年份 → 季度 → 月份 → 日期
     ```

3. **地理层次结构**
   - 在Geography维度设计器中
   - 创建层次结构：
     ```
     国家 → 地区 → 城市
     ```

### 步骤4：定义属性关系

1. **产品维度属性关系**
   - 在Product维度设计器中
   - 选择"属性关系"选项卡
   - 创建关系：
     ```
     产品子类别 → 产品类别
     产品名称 → 产品子类别
     ```

2. **日期维度属性关系**
   - 在Date维度设计器中
   - 创建关系：
     ```
     月份 → 季度
     季度 → 年份
     日期 → 月份
     ```

## 部署和测试

### 最终部署

1. **重新部署项目**
   - 右键单击项目
   - 选择"部署"
   - 等待部署完成

2. **处理多维数据集**
   - 在SQL Server Management Studio中
   - 右键单击多维数据集
   - 选择"处理"
   - 等待处理完成

### 测试查询

1. **使用MDX查询测试**
   ```mdx
   SELECT 
   [Measures].[Sales Amount] ON COLUMNS,
   [Product].[Product Category].[Category] ON ROWS
   FROM [Internet Sales]
   ```

2. **验证层次结构**
   - 测试钻取功能
   - 验证上卷功能
   - 检查属性关系

## 常见问题解决

### 1. 连接问题
- 检查SQL Server服务状态
- 确认Analysis Services服务运行
- 验证身份验证设置

### 2. 部署问题
- 检查服务器权限
- 确认数据库名称正确
- 查看部署日志

### 3. 处理问题
- 检查数据源连接
- 验证表间关系
- 查看处理日志

## 实验验证清单

- [ ] 项目创建成功
- [ ] 数据源连接正常
- [ ] 数据源视图包含所有必要表
- [ ] 多维数据集创建完成
- [ ] 项目部署成功
- [ ] 度量值修改正确
- [ ] 属性设置合理
- [ ] 层次结构创建完成
- [ ] 属性关系定义正确
- [ ] 多维数据集处理成功
- [ ] 查询测试通过

---

**注意事项：**
1. 确保所有服务正常运行
2. 定期保存项目文件
3. 查看部署和处理日志
4. 测试所有功能正常
