# Next Opening: Suppliers & Odin

《Probably Stolen》的本地／Steam Workshop Mod。当前代码版本为 **1.6.0**，目标游戏版本为 **0.46D**。

## 当前功能

- 让四位供货商与奥丁在下一次开店时优先到访；
- 合并同一联系人的重复预约，保留最早的一项；
- 在电话界面一键清除目标联系人的冷却，同时保护已有预约；
- 将相关电话文案同步为“下一次开店”；
- 在交易平板显示当前调查进度，以及当前案值、成交后案值和本次新增案值；
- 在日终平板根据当前收尾人选择预测夜间案值、调查增量和次日调查进度；
- 调查达到 100 后，由王哥自动来电提供付费清除服务；
- 递归高亮顾客需要的物品及治安盘查可发现的物品：绿色为普通需求、黄色为赃物、红色为违禁品；
- 武器定制与改造订单高亮合适的基础武器和配件，并按当前订单进度撤掉已达成条件的配件提示；
- 鉴定服务使用放大镜查看物品时自动填入真实估值，但不会自动提交；
- 放大镜显示注射器、香烟、食品券与肋明治的真伪原因，机械手、肋明治、燃料匣的逐项价值，以及荧光莓品质；
- 放大镜模式检测改造人并显示理由；改造体或通缉犯到店时无需放大镜即显示身份、真名、改造等级与举报奖励；
- 上述新增界面、电话与提示接入游戏语言设置，新增鉴定详情提供中文及英文回退。

预测只读取当前状态，不执行交易或夜间结算。自动清除只重置调查进度与案件状态，**不会清除案值、取消既有调查流程或移除已派遣治安官**，也不消耗 Wild Favor。高亮不移动物品或执行盘查；自动鉴定不会替玩家确认成交。

> **验证边界：**当前自动化结果统一见[当前验证记录](docs/validation/current.md)。用户已确认库存香烟鉴定窗口可见；动态配件高亮、其他真实 UI、电话、存档和禁用／重启用仍待游戏内验收，不能把自动化结果当作完整验收通过。

## 文档导航

| 文档 | 用途 |
| --- | --- |
| [文档总索引](docs/README.md) | 按功能查询行为、实现、边界与验收，以及文档更新规则 |
| [当前验证记录](docs/validation/current.md) | 当前自动化结果、全局未测项与发布检查 |
| [公共工程说明](docs/shared-engineering.md) | 三层架构、主线程、生命周期、构建与发布约束 |
| [1.6.0 更新说明](docs/v1.6.0-update.md) | 高亮、自动鉴定与改造人检测的历史版本快照 |

面向玩家的中英双语文案位于 `WORKSHOP_DESCRIPTION.md` 和 `WORKSHOP_DESCRIPTION_BBCode.txt`。1.6.0 已于 2026-11-02 更新到既有 [Steam 创意工坊项目](https://steamcommunity.com/sharedfiles/filedetails/?id=3829299781)（项目 ID：`3829299781`）；上传内容、更新日志和公开接口状态已经远程核验，页面仍显示“仅你可见”的社区审查提示，详见[发布记录](docs/validation/v1.6.0.md)。

## 运行与兼容环境

已核对的目标环境：

- 游戏：《Probably Stolen》正式版 `0.46D`；
- 引擎：Unity `2021.3.45f2`；
- 运行时：Mono；
- Mod 目标框架：`.NET Standard 2.1`；
- Mod 入口：游戏正式 `IMod` 加载器与 Harmony；
- 当前开发系统：Windows。

`manifest.xml` 中的游戏版本声明不是完整兼容保证。游戏更新后，必须重新核对补丁目标、访客 ID、UI 字段、罪案公式和规则测试。其他修改相同交易、日结、电话或调查规则的 Mod 可能改变补丁顺序或使预测与真实结果不一致。

## 项目结构

```text
NextOpeningVisitors/
├─ manifest.xml                     # Mod 身份、版本、程序集与能力声明
├─ build.py                         # 编译并安装到本地 Mods
├─ run_tests.py                     # 编译并运行纯规则测试
├─ run_scan.py                      # 调用游戏 ModScanner 扫描构建产物
├─ src/
│  ├─ NextOpeningVisitorsMod.cs     # IMod 入口与访客队列补丁
│  ├─ VisitorRules.cs               # 预约识别、去重、延迟归一化与稳定优先排序
│  ├─ CooldownResetRules.cs         # 冷却清除与待到访保护
│  ├─ PhoneCooldownResetButton.cs   # 电话冷却按钮
│  ├─ PhoneDialogueText*.cs         # 电话文案补丁与规则
│  ├─ CrimeForecastModels.cs        # 交易与夜间预测输入／输出模型
│  ├─ CrimeForecastRules.cs         # 纯案值与调查公式
│  ├─ CrimeForecastAdapter.cs       # 当前游戏状态的只读快照适配
│  ├─ CrimeForecastLocalization.cs  # 预测与自动服务 11 语文案
│  ├─ TradeForecastUI.cs            # 交易预测 UI
│  ├─ NightForecastUI.cs            # 日终预测 UI
│  ├─ DemandHighlightRules.cs       # 高亮分类、风险优先级与递归边界
│  ├─ DemandHighlightUI.cs          # 库存扫描、边框与低频刷新宿主
│  ├─ AppraisalAssistantRules.cs    # 鉴定条件、价值公式、品质与提醒规则
│  ├─ AppraisalItemDetails.cs       # 原版物品只读适配、真伪原因与价值对账
│  ├─ AppraisalAssistantUI.cs       # 自动填值、物品详情与到店提醒
│  ├─ AssistantLocalization.cs      # 检测提示 11 语文案
│  ├─ AssistantAppraisalLocalization.cs # 鉴定与身份提醒文本
│  ├─ AutoEvidenceCallState.cs      # 报价状态、序列化与接受校验
│  └─ AutoEvidenceCallController.cs # 服务注册、自动来电、支付与清理
├─ Properties/AssemblyInfo.cs       # 程序集版本与目标框架
├─ tests/                          # 规则测试与官方扫描器宿主
├─ analysis/                       # 版本相关分析材料，不进入发布包
├─ build/                          # 本地编译输出
└─ release/NextOpeningVisitors/     # 发布文件：manifest.xml 与 Mod DLL
```

项目没有 `.sln` 或 `.csproj`；构建脚本直接调用 Roslyn 编译器。

## 构建前检查

以下路径均相对于游戏根目录：

- `.port-tools/dotnet/dotnet.exe`：便携 .NET 运行时；
- `.port-tools/dotnet/sdk/8.0.427/Roslyn/bincore/csc.dll`：编译器；
- `.port-tools/dotnet/packs/Microsoft.NETCore.App.Ref/8.0.33/ref/net8.0/`：测试宿主引用；
- `Probably Stolen_Data/Managed/`：游戏、Unity、Harmony 等程序集。

脚本通过 `Path(__file__).resolve().parents[2]` 定位游戏根目录，因此应保持 `Modding/NextOpeningVisitors` 这一相对位置，或同步修改脚本。

## 构建、测试与本地安装

在游戏根目录执行完整验证：

```bash
python Modding/NextOpeningVisitors/run_tests.py
python Modding/NextOpeningVisitors/build.py
python Modding/NextOpeningVisitors/run_scan.py
```

也可以在本 Mod 目录分别执行 `python run_tests.py`、`python build.py` 和 `python run_scan.py`。

当前断言数、构建和扫描结果不在此重复维护，统一见[当前验证记录](docs/validation/current.md)。`build.py` 会编译全部 `src/*.cs` 与程序集信息，生成 DLL 和便携 PDB，再把 DLL 与清单安装到 `Mods/NextOpeningVisitors/`。它不会自动更新 `release/`，也不会上传 Workshop。扫描通过不等于游戏内行为正确，仍须执行[全局游戏内验收](docs/validation/current.md#4-全局游戏内验收)及各功能手册中的专项验收。

本地启用时，在游戏的 Mod 管理界面检查并启用 Mod，按提示重启。需要区分：

- **已发现**：加载器识别到清单；
- **已启用**：玩家允许加载；
- **已初始化**：日志出现 `Initialized: Next Opening Visitors v1.6.0`。

游戏内 F8 可查看 Mod／调试日志，程序集或初始化错误还应检查 `Player.log`。当前尚未取得 1.6.0 的真实加载与初始化验收记录。测试高亮和鉴定前，应停用 `ClientDemandHighlight` 与 `AppraisalAssistant` 并重启，避免同功能 Mod 重复注入。

## 手动发布

发布目录只应包含：

```text
NextOpeningVisitors/
├─ manifest.xml
└─ NextOpeningVisitors.dll
```

不要打入游戏原始程序集、反编译源码或资源、`analysis/`、`tests/`、`build/`、PDB、日志、缓存、本地配置或上传工具状态。

更新版本时至少同步：

1. `manifest.xml` 的 `ModVersion`；
2. `Properties/AssemblyInfo.cs` 的程序集版本；
3. `NextOpeningVisitorsMod.cs` 的初始化日志版本；
4. Workshop 功能与兼容性说明；
5. 受影响的功能手册与当前验证记录；
6. `release/NextOpeningVisitors/` 中的清单和 DLL。

保持 `manifest.xml` 的 `ID` 为 `nextopeningvisitors`，不要因版本更新更换 Mod 身份或新建重复 Workshop 项目。

## 禁用、存档与故障回退

`OnDisable()` 会退订游戏事件，销毁自动电话与高亮持久宿主，注销自定义服务，并清理预测、高亮和放大镜 UI。禁用后的补丁不再创建文本、边框或自动填值。Harmony 补丁和既有电话按钮仍由游戏加载环境管理，禁用／启用验收应按加载器要求重启，不能假设所有运行时修改都会即时撤销。

预测失败会隐藏新增内容并尽量恢复原版布局；队列补丁失败会保留原版调度；自动来电展示失败不扣款、不清调查、不涨价。接受操作会重新校验报价、日期、调查和现金，并在异常时尝试回滚本次现金、调查、案件状态、价格与报价状态修改。

自动服务状态通过 `PlayerStore.ModData()` 随存档保存，价格保存在独立的自定义 `NetworkUpgrade` 中。卸载 Mod 不会自动恢复已经写入存档的预约、冷却、现金或罪案字段；升级和回滚前应备份存档。

## 版权与分发

`analysis/` 仅用于本地版本适配分析，不是可再分发源码。发布包不得包含游戏程序集、反编译代码、加载器文件或无授权的第三方资源。
