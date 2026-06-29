# SSAS实验代码示例和配置文件

## 1. 数据源连接配置

### 连接字符串示例
```xml
<!-- 在项目文件中 -->
<DataSources>
  <DataSource>
    <ID>AdventureWorksDW2019</ID>
    <Name>AdventureWorksDW2019</Name>
    <ConnectionString>Provider=SQLNCLI11.1;Data Source=localhost;Initial Catalog=AdventureWorksDW2019;Integrated Security=SSPI;Auto Translate=False;</ConnectionString>
    <ImpersonationInfo>
      <ImpersonationMode>ImpersonateServiceAccount</ImpersonationMode>
    </ImpersonationInfo>
  </DataSource>
</DataSources>
```

## 2. 数据源视图定义

### 核心表选择
```sql
-- 事实表
FactInternetSales
- SalesOrderNumber (销售订单号)
- SalesOrderLineNumber (销售订单行号)
- OrderDate (订单日期)
- DueDate (到期日期)
- ShipDate (发货日期)
- ProductKey (产品键)
- CustomerKey (客户键)
- SalesTerritoryKey (销售区域键)
- OrderQuantity (订单数量)
- UnitPrice (单价)
- ExtendedAmount (扩展金额)
- UnitPriceDiscountPct (单价折扣百分比)
- DiscountAmount (折扣金额)
- ProductStandardCost (产品标准成本)
- TotalProductCost (总产品成本)
- SalesAmount (销售额)

-- 维度表
DimProduct
- ProductKey (产品键)
- ProductAlternateKey (产品备用键)
- ProductSubcategoryKey (产品子类别键)
- WeightUnitMeasureCode (重量单位代码)
- SizeUnitMeasureCode (尺寸单位代码)
- EnglishProductName (英文产品名称)
- SpanishProductName (西班牙文产品名称)
- FrenchProductName (法文产品名称)
- StandardCost (标准成本)
- FinishedGoodsFlag (成品标志)
- Color (颜色)
- SafetyStockLevel (安全库存水平)
- ReorderPoint (再订货点)
- ListPrice (标价)
- Size (尺寸)
- SizeRange (尺寸范围)
- Weight (重量)
- DaysToManufacture (制造天数)
- ProductLine (产品线)
- DealerPrice (经销商价格)
- Class (类别)
- Style (样式)
- ModelName (型号名称)
- LargePhoto (大图)
- EnglishDescription (英文描述)
- FrenchDescription (法文描述)
- ChineseDescription (中文描述)
- ArabicDescription (阿拉伯文描述)
- HebrewDescription (希伯来文描述)
- ThaiDescription (泰文描述)
- GermanDescription (德文描述)
- JapaneseDescription (日文描述)
- TurkishDescription (土耳其文描述)
- StartDate (开始日期)
- EndDate (结束日期)
- Status (状态)

DimDate
- DateKey (日期键)
- FullDateAlternateKey (完整日期备用键)
- DayNumberOfWeek (周内天数)
- EnglishDayNameOfWeek (英文周内天名)
- SpanishDayNameOfWeek (西班牙文周内天名)
- FrenchDayNameOfWeek (法文周内天名)
- DayNumberOfMonth (月内天数)
- DayNumberOfYear (年内天数)
- WeekNumberOfYear (年内周数)
- EnglishMonthName (英文月名)
- SpanishMonthName (西班牙文月名)
- FrenchMonthName (法文月名)
- MonthNumberOfYear (年内月数)
- CalendarQuarter (日历季度)
- CalendarYear (日历年份)
- CalendarSemester (日历学期)
- FiscalQuarter (财政季度)
- FiscalYear (财政年份)
- FiscalSemester (财政学期)

DimCustomer
- CustomerKey (客户键)
- GeographyKey (地理键)
- CustomerAlternateKey (客户备用键)
- Title (头衔)
- FirstName (名字)
- MiddleName (中间名)
- LastName (姓氏)
- NameStyle (姓名样式)
- BirthDate (出生日期)
- MaritalStatus (婚姻状况)
- Suffix (后缀)
- Gender (性别)
- EmailAddress (电子邮件地址)
- YearlyIncome (年收入)
- TotalChildren (总子女数)
- NumberChildrenAtHome (在家子女数)
- EnglishEducation (英文教育)
- SpanishEducation (西班牙文教育)
- FrenchEducation (法文教育)
- EnglishOccupation (英文职业)
- SpanishOccupation (西班牙文职业)
- FrenchOccupation (法文职业)
- HouseOwnerFlag (房主标志)
- NumberCarsOwned (拥有汽车数)
- AddressLine1 (地址行1)
- AddressLine2 (地址行2)
- Phone (电话)
- DateFirstPurchase (首次购买日期)
- CommuteDistance (通勤距离)

DimGeography
- GeographyKey (地理键)
- GeographyType (地理类型)
- ContinentName (大陆名称)
- CountryRegionCode (国家地区代码)
- StateProvinceCode (州省代码)
- StateProvinceName (州省名称)
- CityName (城市名称)
- PostalCode (邮政编码)
- SalesTerritoryKey (销售区域键)
- IpAddressLocator (IP地址定位器)

DimSalesTerritory
- SalesTerritoryKey (销售区域键)
- SalesTerritoryAlternateKey (销售区域备用键)
- SalesTerritoryRegion (销售区域地区)
- SalesTerritoryCountry (销售区域国家)
- SalesTerritoryGroup (销售区域组)
- SalesTerritoryImage (销售区域图像)

DimProductCategory
- ProductCategoryKey (产品类别键)
- ProductCategoryAlternateKey (产品类别备用键)
- EnglishProductCategoryName (英文产品类别名称)
- SpanishProductCategoryName (西班牙文产品类别名称)
- FrenchProductCategoryName (法文产品类别名称)

DimProductSubcategory
- ProductSubcategoryKey (产品子类别键)
- ProductSubcategoryAlternateKey (产品子类别备用键)
- ProductCategoryKey (产品类别键)
- EnglishProductSubcategoryName (英文产品子类别名称)
- SpanishProductSubcategoryName (西班牙文产品子类别名称)
- FrenchProductSubcategoryName (法文产品子类别名称)
```

## 3. 多维数据集定义

### 度量值组配置
```xml
<MeasureGroups>
  <MeasureGroup>
    <ID>Internet Sales</ID>
    <Name>Internet Sales</Name>
    <Type>Regular</Type>
    <Measures>
      <Measure>
        <ID>Sales Amount</ID>
        <Name>销售额</Name>
        <AggregateFunction>Sum</AggregateFunction>
        <DataType>Double</DataType>
        <Source>
          <SourceColumnID>SalesAmount</SourceColumnID>
        </Source>
      </Measure>
      <Measure>
        <ID>Order Quantity</ID>
        <Name>订单数量</Name>
        <AggregateFunction>Sum</AggregateFunction>
        <DataType>Integer</DataType>
        <Source>
          <SourceColumnID>OrderQuantity</SourceColumnID>
        </Source>
      </Measure>
      <Measure>
        <ID>Unit Price</ID>
        <Name>单价</Name>
        <AggregateFunction>Average</AggregateFunction>
        <DataType>Double</DataType>
        <Source>
          <SourceColumnID>UnitPrice</SourceColumnID>
        </Source>
      </Measure>
      <Measure>
        <ID>Extended Amount</ID>
        <Name>扩展金额</Name>
        <AggregateFunction>Sum</AggregateFunction>
        <DataType>Double</DataType>
        <Source>
          <SourceColumnID>ExtendedAmount</SourceColumnID>
        </Source>
      </Measure>
      <Measure>
        <ID>Discount Amount</ID>
        <Name>折扣金额</Name>
        <AggregateFunction>Sum</AggregateFunction>
        <DataType>Double</DataType>
        <Source>
          <SourceColumnID>DiscountAmount</SourceColumnID>
        </Source>
      </Measure>
    </Measures>
  </MeasureGroup>
</MeasureGroups>
```

## 4. 维度定义

### 产品维度配置
```xml
<Dimension>
  <ID>Product</ID>
  <Name>Product</Name>
  <Type>Regular</Type>
  <Attributes>
    <Attribute>
      <ID>Product Key</ID>
      <Name>产品ID</Name>
      <Type>Key</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>ProductKey</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>ProductKey</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Product Name</ID>
      <Name>产品名称</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>ProductKey</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>EnglishProductName</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Color</ID>
      <Name>颜色</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>Color</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>Color</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Size</ID>
      <Name>尺寸</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>Size</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>Size</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Weight</ID>
      <Name>重量</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>Weight</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>Weight</SourceColumnID>
      </NameColumn>
    </Attribute>
  </Attributes>
</Dimension>
```

### 日期维度配置
```xml
<Dimension>
  <ID>Date</ID>
  <Name>Date</Name>
  <Type>Time</Type>
  <Attributes>
    <Attribute>
      <ID>Date Key</ID>
      <Name>日期ID</Name>
      <Type>Key</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>DateKey</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>FullDateAlternateKey</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Calendar Year</ID>
      <Name>日历年</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>CalendarYear</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>CalendarYear</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Calendar Quarter</ID>
      <Name>日历季度</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>CalendarQuarter</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>CalendarQuarter</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Month</ID>
      <Name>月份</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>MonthNumberOfYear</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>EnglishMonthName</SourceColumnID>
      </NameColumn>
    </Attribute>
    <Attribute>
      <ID>Day</ID>
      <Name>日期</Name>
      <Type>Regular</Type>
      <KeyColumns>
        <KeyColumn>
          <SourceColumnID>DateKey</SourceColumnID>
        </KeyColumn>
      </KeyColumns>
      <NameColumn>
        <SourceColumnID>FullDateAlternateKey</SourceColumnID>
      </NameColumn>
    </Attribute>
  </Attributes>
</Dimension>
```

## 5. 层次结构定义

### 产品层次结构
```xml
<Hierarchy>
  <ID>Product Categories</ID>
  <Name>产品类别</Name>
  <Levels>
    <Level>
      <ID>Product Category</ID>
      <Name>产品类别</Name>
      <SourceAttributeID>Product Category</SourceAttributeID>
    </Level>
    <Level>
      <ID>Product Subcategory</ID>
      <Name>产品子类别</Name>
      <SourceAttributeID>Product Subcategory</SourceAttributeID>
    </Level>
    <Level>
      <ID>Product Name</ID>
      <Name>产品名称</Name>
      <SourceAttributeID>Product Name</SourceAttributeID>
    </Level>
  </Levels>
</Hierarchy>
```

### 时间层次结构
```xml
<Hierarchy>
  <ID>Calendar</ID>
  <Name>日历</Name>
  <Levels>
    <Level>
      <ID>Calendar Year</ID>
      <Name>日历年</Name>
      <SourceAttributeID>Calendar Year</SourceAttributeID>
    </Level>
    <Level>
      <ID>Calendar Quarter</ID>
      <Name>日历季度</Name>
      <SourceAttributeID>Calendar Quarter</SourceAttributeID>
    </Level>
    <Level>
      <ID>Month</ID>
      <Name>月份</Name>
      <SourceAttributeID>Month</SourceAttributeID>
    </Level>
    <Level>
      <ID>Day</ID>
      <Name>日期</Name>
      <SourceAttributeID>Day</SourceAttributeID>
    </Level>
  </Levels>
</Hierarchy>
```

## 6. 属性关系定义

### 产品维度属性关系
```xml
<AttributeRelations>
  <AttributeRelation>
    <AttributeID>Product Subcategory</AttributeID>
    <RelatedAttributeID>Product Category</RelatedAttributeID>
    <RelationshipType>Rigid</RelationshipType>
  </AttributeRelation>
  <AttributeRelation>
    <AttributeID>Product Name</AttributeID>
    <RelatedAttributeID>Product Subcategory</RelatedAttributeID>
    <RelationshipType>Rigid</RelationshipType>
  </AttributeRelation>
</AttributeRelations>
```

### 日期维度属性关系
```xml
<AttributeRelations>
  <AttributeRelation>
    <AttributeID>Month</AttributeID>
    <RelatedAttributeID>Calendar Quarter</RelatedAttributeID>
    <RelationshipType>Rigid</RelationshipType>
  </AttributeRelation>
  <AttributeRelation>
    <AttributeID>Calendar Quarter</AttributeID>
    <RelatedAttributeID>Calendar Year</RelatedAttributeID>
    <RelationshipType>Rigid</RelationshipType>
  </AttributeRelation>
  <AttributeRelation>
    <AttributeID>Day</AttributeID>
    <RelatedAttributeID>Month</RelatedAttributeID>
    <RelationshipType>Rigid</RelationshipType>
  </AttributeRelation>
</AttributeRelations>
```

## 7. MDX查询示例

### 基本查询
```mdx
-- 查询销售额按产品类别
SELECT 
  [Measures].[Sales Amount] ON COLUMNS,
  [Product].[Product Categories].[Category] ON ROWS
FROM [Internet Sales]

-- 查询销售额按时间
SELECT 
  [Measures].[Sales Amount] ON COLUMNS,
  [Date].[Calendar].[Year] ON ROWS
FROM [Internet Sales]

-- 查询销售额按客户
SELECT 
  [Measures].[Sales Amount] ON COLUMNS,
  [Customer].[Customer].[Customer] ON ROWS
FROM [Internet Sales]
```

### 复杂查询
```mdx
-- 查询销售额按产品类别和时间
SELECT 
  [Measures].[Sales Amount] ON COLUMNS,
  [Product].[Product Categories].[Category] ON ROWS
FROM [Internet Sales]
WHERE [Date].[Calendar].[Calendar Year].&[2013]

-- 查询销售额按销售区域
SELECT 
  [Measures].[Sales Amount] ON COLUMNS,
  [Sales Territory].[Sales Territory].[Sales Territory Country] ON ROWS
FROM [Internet Sales]

-- 查询订单数量按产品子类别
SELECT 
  [Measures].[Order Quantity] ON COLUMNS,
  [Product].[Product Categories].[Subcategory] ON ROWS
FROM [Internet Sales]
```

## 8. 部署配置

### 项目属性配置
```xml
<Project>
  <PropertyGroup>
    <TargetServerVersion>SQL Server 2019</TargetServerVersion>
    <TargetDatabaseID>Adventure Works Tutorial</TargetDatabaseID>
    <TargetServerID>localhost</TargetServerID>
    <TargetConnectionString>Data Source=localhost;Provider=MSOLAP;Impersonation Level=Impersonate;</TargetConnectionString>
  </PropertyGroup>
</Project>
```

### 部署脚本
```powershell
# 部署脚本示例
$projectPath = "C:\Projects\Adventure Works Tutorial\Adventure Works Tutorial.sln"
$deploymentServer = "localhost"
$deploymentDatabase = "Adventure Works Tutorial"

# 使用SSDT命令行工具部署
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\devenv.exe" $projectPath /deploy
```

## 9. 处理脚本

### 处理多维数据集
```sql
-- 处理多维数据集
<Process xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <Object>
    <DatabaseID>Adventure Works Tutorial</DatabaseID>
    <CubeID>Internet Sales</CubeID>
  </Object>
  <Type>ProcessFull</Type>
</Process>
```

### 处理维度
```sql
-- 处理维度
<Process xmlns="http://schemas.microsoft.com/analysisservices/2003/engine">
  <Object>
    <DatabaseID>Adventure Works Tutorial</DatabaseID>
    <DimensionID>Product</DimensionID>
  </Object>
  <Type>ProcessFull</Type>
</Process>
```

## 10. 验证查询

### 连接测试
```sql
-- 测试Analysis Services连接
SELECT * FROM $SYSTEM.DBSCHEMA_CATALOGS

-- 测试多维数据集
SELECT * FROM $SYSTEM.DBSCHEMA_CUBES

-- 测试维度
SELECT * FROM $SYSTEM.DBSCHEMA_DIMENSIONS

-- 测试度量值
SELECT * FROM $SYSTEM.DBSCHEMA_MEASURES
```

---

**注意事项：**
1. 确保所有表名和列名与实际数据库一致
2. 检查数据类型匹配
3. 验证外键关系
4. 测试所有MDX查询
5. 定期备份项目文件
