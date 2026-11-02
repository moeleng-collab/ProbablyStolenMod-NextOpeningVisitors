# 公共工程说明

本文只记录跨功能共享规则。单个功能的补丁细节、行为、测试和验收见[总索引](README.md#功能手册)。

## 1. 运行环境与事实源

- 游戏：《Probably Stolen》正式版 `0.46D`；
- 引擎：Unity `2021.3.45f2`；
- 运行时：Mono；
- Mod 目标框架：`.NET Standard 2.1`；
- 加载方式：游戏正式 `IMod` 加载器与 Harmony；
- 当前开发系统：Windows。

事实源优先级：

1. 当前游戏版本的公开 Mod API 与可观察行为；
2. 当前项目 `src/`；
3. 当前游戏 `Assembly-CSharp.dll` 的版本适配分析；
4. 当前功能手册中的规则整理；
5. 玩家描述用于提出需求，不代替代码或运行核验。

游戏更新后必须重新核对补丁目标、方法签名、字段、常量和结算顺序。不得把 `analysis/` 中的反编译代码复制进 Mod 源码或发布包。`manifest.xml` 的游戏版本声明不是完整兼容保证。

## 2. 分层与线程

项目采用“纯规则＋游戏适配／UI＋加载器入口”三层结构：

- **纯规则层**：调度、冷却、案值与调查计算、报价状态、颜色和风险判断、鉴定条件及本地化；通过简单模型、`IList<T>` 和委托隔离 Unity。
- **游戏适配层**：读取 `SecData`、物品、客户、服务和特长，只在明确功能分支写入真实状态。
- **UI／平台层**：Harmony、`IMod`、电话、UI、日志、注册与 `PlayerStore.ModData()`。

游戏对象访问和 UI 修改保持在游戏主线程。预测只读真实对象并写入临时模型。高亮和自动电话的持久宿主仅做低频主线程刷新或重试，不引入后台线程或跨线程游戏对象访问。

## 3. 入口、生命周期与补丁索引

`NextOpeningVisitorsMod` 负责：

- `Init(ModManifest)`：创建 `ModLog`；
- `OnEnable()`：启用预测、高亮、鉴定和检测 UI，订阅 `OnGameLoadedLate`、`OnNewGameStarted`、`OnWakingUpLate`，记录 `v1.6.0` 初始化日志；
- `OnDisable()`：先禁用各 UI，再退订事件，关闭自动电话控制器并记录日志。

源码不自行调用 `Harmony.PatchAll()`，补丁激活由游戏 Mod 加载环境负责。完整禁用和重启用验收应按加载器要求重启，不能假设所有 Harmony 修改都会即时撤销。

| 领域 | 目标／事件 | 功能手册 |
| --- | --- | --- |
| 访客队列 | `PlayerStore.HandleFutureClientQueue` Prefix／Postfix | [访客调度](features/visitor-scheduling.md) |
| 电话冷却按钮 | `PhoneUIManager.OpenUI` Postfix | [电话冷却清除](features/phone-cooldown.md) |
| 电话文案 | `LocHelper.GetLocalizedDialoguePhone` Postfix | [电话本地化](features/phone-localization.md) |
| 交易预测 | `NegociationUIManager.Rebuild / RefreshNow` | [交易预测](features/trade-forecast.md) |
| 日终预测 | `EndOfDayUIManager.InitUI / RefreshUI` | [夜间预测](features/night-forecast.md) |
| 自动服务与电话 | 新日／读档事件、`StorePhoneClient.GetCallDialog`、持久宿主 | [自动证据清除](features/auto-evidence-cleanup.md) |
| 物品高亮 | `GameItemElement.Validate`、`DemandHighlightHost.Update` | [需求与盘查高亮](features/demand-highlight.md) |
| 自动鉴定 | `InspectionUIManager.OnInspectItem` Postfix | [自动鉴定](features/appraisal-assistant.md) |
| 改造人检测 | `StoreUIManager.Enter/ExitMagnifyingMode` Postfix | [改造人检测](features/aug-detection.md) |

对称清理原则：

- 预测 UI 禁用后恢复布局并销毁新增文本，后续补丁不得重新创建内容；
- 高亮禁用后销毁持久宿主和全部边框；
- 鉴定／检测禁用后销毁提示，后续补丁不再自动填值或创建 UI；
- 自动电话 `Shutdown()` 退订事件、清除内存状态和注入对话、销毁宿主，并调用 `RemoveModUpgrades(Source)`；
- 既有电话按钮和 Harmony 反补丁不在当前版本额外重写，仍需真实禁用／重启用验收。

## 4. 公共本地化与失败策略

支持语言：`en`、`zh`、`ja`、`de`、`ru`、`fr`、`es`、`pl`、`ptBR`、`ko`、`it`。语言规范化忽略 `-` 与 `_`，中文区域代码归入 `zh`，葡萄牙语归入 `ptBR`，未知语言回退英文。各功能的 key、参数和特殊回退规则记录在对应手册。

通用失败原则：

- 队列补丁失败时记录警告并保留原版调度；
- 预测失败时隐藏新增区域并尽量恢复原版布局；
- 高亮失败时记录警告，不阻断原生物品交互；
- 自动鉴定失败时保留原输入，不提交估值；
- 自动来电展示失败不扣款、不清调查、不涨价；
- 接受异常只按该功能设计尝试回滚，不通过删除治安官“修复”状态；
- 正常预测和高亮刷新不写逐帧日志。

任何失败策略都必须通过对应游戏内场景验证，不能仅凭 `try/catch` 推断恢复完整。

## 5. 项目结构与构建

```text
NextOpeningVisitors/
├─ manifest.xml
├─ build.py
├─ run_tests.py
├─ run_scan.py
├─ src/
├─ Properties/AssemblyInfo.cs
├─ tests/
├─ analysis/
├─ build/
└─ release/NextOpeningVisitors/
```

项目没有 `.sln` 或 `.csproj`，构建脚本直接调用 Roslyn。以下路径相对于游戏根目录：

- `.port-tools/dotnet/dotnet.exe`；
- `.port-tools/dotnet/sdk/8.0.427/Roslyn/bincore/csc.dll`；
- `.port-tools/dotnet/packs/Microsoft.NETCore.App.Ref/8.0.33/ref/net8.0/`；
- `Probably Stolen_Data/Managed/`。

脚本通过 `Path(__file__).resolve().parents[2]` 定位游戏根目录，因此应保持 `Modding/NextOpeningVisitors` 的相对位置，或同步修改脚本。

在游戏根目录执行：

```bash
python Modding/NextOpeningVisitors/run_tests.py
python Modding/NextOpeningVisitors/build.py
python Modding/NextOpeningVisitors/run_scan.py
```

`run_tests.py` 每次重新编译纯规则测试；测试宿主使用 .NET 8 引用包，但 Mod 运行时仍为 `.NET Standard 2.1`。`build.py` 编译全部 `src/*.cs` 与程序集信息，生成 DLL 和开发用 PDB，再把 DLL 与清单安装到 `Mods/NextOpeningVisitors/`。它不会自动更新 `release/` 或上传 Workshop。必须先构建再扫描，确保扫描最新 DLL。

当前实际结果只见[当前验证记录](validation/current.md)。

## 6. 加载、存档与兼容边界

需要区分：

- **已发现**：加载器识别到清单；
- **已启用**：玩家允许加载；
- **已初始化**：日志出现当前版本的 `Initialized` 记录。

游戏内 F8 可查看 Mod／调试日志，程序集或初始化错误还应检查 `Player.log`。本地与 Workshop 不应同时加载两份副本。

访客与冷却使用原版字段；自动服务另使用自定义 `NetworkUpgrade` 和 `PlayerStore.ModData()`，具体键见功能手册。高亮、预测、鉴定和改造人检测不新增存档数据。卸载 Mod 不会自动恢复已经写入存档的预约、冷却、现金或罪案字段，升级和回滚前应备份存档。

其他修改相同交易、日结、电话、调查、物品或客户规则的 Mod 可能改变补丁顺序或使预测与真实结果不一致。测试高亮和鉴定功能前，应停用 `ClientDemandHighlight` 与 `AppraisalAssistant` 并重启，避免重复注入；不要修改第三方 Mod 文件。

## 7. 发布与版本一致性

发布目录只应包含：

```text
NextOpeningVisitors/
├─ manifest.xml
└─ NextOpeningVisitors.dll
```

不得打入游戏原始程序集、反编译源码或资源、`analysis/`、`tests/`、`build/`、PDB、日志、缓存、本地配置或上传工具状态。

更新版本时至少同步：

1. `manifest.xml` 的 `ModVersion`；
2. `Properties/AssemblyInfo.cs` 的程序集版本和信息版本；
3. `NextOpeningVisitorsMod.cs` 的初始化日志版本；
4. 受影响的功能手册与[当前验证记录](validation/current.md)；
5. Workshop 玩家功能与兼容性说明；
6. `release/NextOpeningVisitors/` 中的清单和 DLL。

保持 `manifest.xml` 的 `ID` 为 `nextopeningvisitors`，不要因版本更新更换 Mod 身份或新建重复 Workshop 项目。既有 Workshop 项目 ID 为 `3829299781`；发布状态和实际上传结果以对应版本记录为准，不从本地构建推断远程已更新。
