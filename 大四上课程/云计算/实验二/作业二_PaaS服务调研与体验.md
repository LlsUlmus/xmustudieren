# 国内外云计算厂商 PaaS 服务调研、对比分析与实践体验

## 一、主要厂商 PaaS 服务调研

### 1.1 阿里云：Serverless 应用引擎（SAE）

阿里云面向应用托管的 PaaS 产品主要包括 Serverless 应用引擎 SAE、函数计算 FC 及企业级分布式应用服务 EDAS。其中 SAE 是其主推的应用级 Serverless 平台，底层基于阿里云容器服务构建，但对用户屏蔽了 Kubernetes 集群的管理细节。用户可通过 JAR/WAR 包、Python/PHP 代码包或容器镜像完成部署。

SAE 的突出特点在于对 Java 微服务体系的深度支持：Spring Cloud、Dubbo 应用可在基本不修改代码的前提下完成迁移，平台内置服务注册与发现、灰度发布、弹性策略以及与应用实时监控服务 ARMS 的集成。计费方面，SAE 按应用实例实际占用的 vCPU 与内存时长计费，应用停止后不再产生计算资源费用。

### 1.2 腾讯云：云开发 CloudBase 与云托管

腾讯云 PaaS 服务中最具差异化的是云开发 CloudBase。该产品将云函数（SCF）、文档型数据库、对象存储、静态网站托管及云托管（容器化应用托管）整合为一体化后端服务，并与微信生态深度结合。“微信云托管”支持在免鉴权条件下调用微信开放接口，显著降低了小程序与公众号类应用的后端开发成本。面向企业级微服务场景，腾讯云另提供微服务平台 TSF 及 TKE Serverless 集群。

### 1.3 华为云：ServiceStage 与 FunctionGraph

华为云应用管理与运维平台 ServiceStage 定位于企业级应用全生命周期管理，覆盖源码构建、部署、运行、监控与运维等环节，支持虚拟机与容器两类部署环境，兼容 Java、Go、Node.js、Python 等主流语言及 Spring Cloud、ServiceComb 等微服务框架。函数计算方向由 FunctionGraph 承载。华为云的优势领域集中于政企市场及国产化适配，如鲲鹏（ARM 架构）处理器与昇腾 AI 算力生态。

### 1.4 Amazon Web Services：Elastic Beanstalk、App Runner 与 Lambda

AWS 以 IaaS 起家，其 PaaS 产品呈多元化分布。Elastic Beanstalk 是其传统应用托管服务，支持 Java、.NET、PHP、Node.js、Python、Ruby、Go 及 Docker；服务本身不单独收费，用户仅为底层 EC2、Elastic Load Balancing 等资源付费。由于 Beanstalk 创建的计算实例对用户可见且可登录，其本质可视为对 IaaS 资源的自动化编排。App Runner 则面向容器化 Web 服务提供更高抽象层次的托管能力；Lambda 作为函数计算的开创性产品，是 FaaS 领域的事实标杆。

### 1.5 Microsoft Azure：App Service

Azure App Service 是典型的托管式 PaaS，支持 .NET、Java、Node.js、PHP、Python 及自定义容器。其优势在于与微软开发工具链的无缝集成，包括 Visual Studio、Visual Studio Code、GitHub Actions 与 Azure DevOps；部署槽（Deployment Slot）机制为蓝绿发布与回滚提供了原生支持。App Service 提供 F1 免费层，适用于学习与功能验证，但存在 CPU 时间配额限制，且不支持自动扩缩容。在中国大陆，Azure 由世纪互联独立运营，部分功能上线时间晚于国际版。

### 1.6 Google Cloud：App Engine 与 Cloud Run

Google App Engine 是最早面向公众提供的 PaaS 服务之一，标准环境支持 Python、Java、Node.js、PHP、Ruby、Go，具备缩容至零实例的能力并提供每日免费配额。近年来 Google 的战略重心转向 Cloud Run：该服务基于开源项目 Knative 构建，以容器为部署单元，亦可通过 Cloud Buildpacks 直接从源代码构建镜像；计费粒度精确到请求处理期间的资源占用，并提供月度免费额度。

### 1.7 Heroku（Salesforce）

Heroku 是以开发者体验著称的老牌 PaaS，开发者通过 Git 推送即可触发构建与部署，Buildpack 机制可自动识别应用语言与依赖；其 Add-ons 市场提供 PostgreSQL、Redis 等托管组件的一键集成。2022 年 11 月，Heroku 终止免费套餐，此举导致部分个人开发者与开源项目迁移至 Render、Railway、Fly.io 等新兴平台，也在一定程度上反映出 PaaS 市场的商业化压力。

## 二、PaaS 服务对比分析

### 2.1 综合对比

| 对比维度 | 阿里云 SAE | 腾讯云 CloudBase | 华为云 ServiceStage | AWS Beanstalk / App Runner | Azure App Service | Google App Engine / Cloud Run | Heroku |
|---|---|---|---|---|---|---|---|
| 部署形态 | 代码包、容器镜像 | 源码、镜像、云函数 | 软件包、镜像、源码构建 | 代码包、Docker、源码仓库 | 代码、容器、CI/CD 流水线 | 源码（Buildpacks）、容器 | Git 推送、容器 |
| 语言与运行时 | Java 为重点，兼容 PHP、Python、Go 及任意镜像 | Node.js、Python、Go、Java、PHP 等 | Java、Go、Node.js、Python 等 | 主流语言及 Docker | .NET 为重点，兼容 Java、Node.js、PHP、Python | 主流语言及任意容器 | 主流语言（基于 Buildpack） |
| 弹性伸缩 | 定时与指标弹性，支持缩容至零 | 请求驱动自动扩缩，支持缩容至零 | 基于指标的弹性伸缩 | Beanstalk 依托 Auto Scaling 组；App Runner 自动扩缩 | 基于规则扩展，免费层不支持 | 请求驱动自动扩缩，支持缩容至零 | 以手动调整 Dyno 为主，高阶套餐支持自动扩缩 |
| 计费模式 | 按 vCPU 与内存实际使用时长计费，可叠加资源包 | 按量计费与套餐包 | 平台能力部分免费，底层资源按量计费 | 平台免费，底层资源按量计费 | 按应用服务计划（实例规格）计费 | 按实例时长或请求处理时长计费 | 按 Dyno 规格包月 |
| 生态集成 | 微服务引擎 MSE、ARMS 监控 | 微信及小程序生态 | 政企行业、国产化软硬件 | 云服务种类最全 | 微软开发工具链、GitHub | Kubernetes / Knative 开源生态 | Add-ons 组件市场 |
| 地域与合规 | 境内节点完善，支持 ICP 备案 | 境内节点完善，支持 ICP 备案 | 境内节点完善，满足政企合规要求 | 中国区需独立账号 | 中国区由世纪互联运营 | 中国大陆无节点 | 中国大陆无节点 |
| 学习门槛 | 中等 | 较低 | 中等偏高 | 中等偏高 | 较低 | 较低 | 低 |

### 2.2 分维度分析

**（1）部署形态与抽象层次。** 从抽象层次看，现有 PaaS 可分为代码级与容器级两类。代码级 PaaS（如 Heroku、App Engine 标准环境）由平台负责语言识别与构建，使用门槛最低，但运行环境可定制性有限，依赖特定系统库的应用易出现兼容问题。容器级 PaaS（如 Cloud Run、SAE、腾讯云托管）以 OCI 镜像为交付物，在运行环境一致性与可移植性方面更具优势，代价是用户需具备容器化能力。从产业趋势看，容器级 PaaS 已成为主流方向。

**（2）语言支持与技术侧重。** 容器化使平台在理论上具备运行任意语言的能力，因此各厂商的差异更多体现在对特定技术栈的增强支持上：Azure App Service 对 .NET 提供原生支持，阿里云 SAE 针对 Java 应用提供启动加速与微服务治理能力，腾讯云 CloudBase 则围绕小程序开发中常用的 Node.js 构建工具链。

**（3）弹性伸缩能力。** 是否支持缩容至零实例，直接决定了低流量场景下的资源成本。Cloud Run、SAE 与腾讯云托管均支持该特性；Elastic Beanstalk 以 EC2 实例组为基础，至少需保留一个运行实例；Azure App Service 按服务计划计费，实例在运行期间持续产生费用。需要指出的是，缩容至零会引入冷启动延迟，对于 JVM 类应用影响尤为显著。

**（4）计费模式。** 传统 PaaS（Azure App Service、Heroku）采用按规格计费方式，成本可预测性较强；Serverless 型 PaaS（Cloud Run、SAE）按实际资源消耗计费，在流量较低或波动较大的场景下成本优势明显，但在持续高负载场景下未必经济，且费用预估难度较高。Elastic Beanstalk 平台本身不收费，但其关联的计算、负载均衡与存储资源叠加后，总成本容易被低估。

**（5）生态集成与地域合规。** 若目标用户位于中国大陆，阿里云、腾讯云与华为云在网络时延、ICP 备案、数据合规及支付方式上具备明显优势。AWS 与 Azure 的中国区与国际版在账号体系上相互独立，服务上线存在滞后；Google Cloud 与 Heroku 在大陆地区缺乏节点，且注册需境外支付方式。反之，面向海外市场或开源社区的项目，国外平台在文档完整性与社区资源方面更为成熟。

## 三、实践体验

### 3.1 实验环境与内容

为对比国内外平台在实际使用中的差异，本次实践选取阿里云 SAE 与 Google Cloud Run 两个平台，分别部署如下应用：

阿里云 SAE：基于 Spring Boot 的 RESTful 接口服务，以 JAR 包方式部署；

Google Cloud Run：基于 Flask 的 Python Web 应用，以源代码方式部署，由 Cloud Buildpacks 自动构建镜像。

### 3.2 部署过程

**阿里云 SAE。** 首先在本地通过 `mvn package` 构建可执行 JAR 包。在 SAE 控制台创建应用时，平台要求预先指定命名空间、专有网络 VPC 及虚拟交换机，因此需先在专有网络控制台完成网络资源创建。随后选择 JAR 包部署方式，上传构建产物，指定 Java 运行时版本，实例规格配置为 0.5 vCPU / 1 GiB，实例数为 1。部署完成后，应用默认仅可在 VPC 内部访问，需额外绑定公网 CLB（负载均衡）实例方可对外提供服务。完成绑定后，接口可正常响应。此外，本次实践还验证了实时日志、Webshell 登录实例及应用启停等功能。

**Google Cloud Run。** 在本地准备 `app.py` 与 `requirements.txt` 后，未编写 Dockerfile，直接执行以下命令部署：

```bash
gcloud run deploy flask-demo --source . --region asia-east1 --allow-unauthenticated
```

首次执行时，命令行工具提示启用 Cloud Build、Artifact Registry 等依赖 API。确认后平台自动完成镜像构建、推送与服务发布，并返回一个启用 HTTPS 的服务地址。修改代码并再次部署后，控制台中生成新的修订版本（Revision），可按比例在不同版本之间分配流量，实现灰度发布。

### 3.3 体验分析

**1. PaaS 降低了运维复杂度，但并未完全屏蔽底层基础设施概念。**
在两个平台上，应用部署本身耗时较短，主要时间成本集中于前置配置环节。SAE 要求用户理解 VPC、交换机、负载均衡等网络概念；Cloud Run 则要求完成账号结算信息绑定及多项依赖 API 的启用。由此可见，当前主流 PaaS 并非对底层资源的完全抽象，而是以更高层次的配置接口重新暴露基础设施能力。使用者仍需具备一定的 IaaS 与网络基础知识，才能高效完成部署。

**2. 国外平台侧重开发者体验，国内平台侧重企业级治理能力。**
Cloud Run 通过单条命令即可完成从源码到 HTTPS 服务的发布，并默认提供日志、版本管理与流量切分，交互流程简洁。SAE 控制台则集成了灰度发布、弹性策略、微服务治理、JVM 监控等大量功能，初次使用存在一定学习成本，但这些能力正是生产环境中 Java 微服务所必需的。两者定位存在明显差异：Cloud Run 更适用于轻量级服务与快速原型验证，SAE 更适用于已有 Spring Cloud 体系、需要从 ECS 或自建 Kubernetes 迁移的企业团队。

**3. 按量计费需关注关联资源的独立计费。**
Cloud Run 在“仅在请求处理期间分配 CPU”的配置下，实验期间的资源消耗基本处于免费额度范围内。SAE 应用停止后计算资源不再计费，但与之关联的公网 CLB、镜像仓库等资源仍独立计费，并不会随应用停止而释放。这表明 Serverless 型 PaaS 的“按需付费”通常仅针对计算资源本身，负载均衡、NAT 网关、日志存储等周边资源需单独管理。实践中应在实验结束后逐项释放资源，并预先配置费用预警。

**4. 缩容至零与冷启动延迟之间存在权衡。**
在缩容至零的配置下，Cloud Run 上的 Flask 应用首次请求存在秒级延迟，SAE 上的 Spring Boot 应用由于 JVM 启动及健康检查耗时，恢复服务所需时间明显更长。对于对响应时延敏感的线上业务，通常需要设置最小实例数，或借助平台提供的启动加速能力，这将相应增加常驻成本。因此，是否启用缩容至零应结合业务访问模式与时延要求综合评估。

**5. 平台深度绑定带来潜在的迁移成本。**
SAE 在服务注册、配置管理与监控等方面推荐使用阿里云 MSE、ARMS 等配套产品，集成度高、使用便捷，但依赖程度越深，后续跨云迁移成本越高。相比之下，Cloud Run 基于开源 Knative 规范与标准 OCI 镜像，应用具备较好的可移植性。Heroku 终止免费套餐后引发的用户迁移也表明，PaaS 选型除关注当前的易用性与成本外，还应将供应商锁定风险纳入评估。

## 四、总结

综合调研与实践结果，PaaS 已由早期受限于特定语言与框架的应用托管平台，演进为以容器为交付单元、以 Serverless 为资源与计费模型的应用运行平台。国内厂商在境内网络质量、合规备案、行业生态（微服务治理、微信生态、政企国产化）方面具备优势；国外厂商在开发者体验、开源生态兼容性及技术文档与社区方面更为成熟。

在实际选型中，建议从以下几个方面综合考量：目标用户的地域分布与合规要求、团队现有技术栈、业务流量模型及对冷启动的容忍度、总体拥有成本（含关联资源），以及对供应商锁定风险的接受程度。

## 参考文献

1. 阿里云. Serverless 应用引擎 SAE 产品文档[EB/OL]. https://help.aliyun.com/product/118957.html
2. 腾讯云. 云开发 CloudBase 产品文档[EB/OL]. https://cloud.tencent.com/document/product/876
3. 华为云. 应用管理与运维平台 ServiceStage 产品文档[EB/OL]. https://support.huaweicloud.com/servicestage/
4. Amazon Web Services. AWS Elastic Beanstalk Developer Guide[EB/OL]. https://docs.aws.amazon.com/elasticbeanstalk/
5. Microsoft. Azure App Service Documentation[EB/OL]. https://learn.microsoft.com/azure/app-service/
6. Google Cloud. Cloud Run Documentation[EB/OL]. https://cloud.google.com/run/docs
7. Heroku. Heroku Dev Center[EB/OL]. https://devcenter.heroku.com/
8. 中国信息通信研究院. 云计算白皮书[R]. 北京: 中国信息通信研究院.
9. Mell P, Grance T. The NIST Definition of Cloud Computing[R]. NIST Special Publication 800-145, 2011.
