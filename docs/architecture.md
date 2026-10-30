# 现有项目架构

- 适用版本：`1.5.0`
- 目标游戏：《Probably Stolen》`0.46D`
- 状态：当前源码实现说明

## 1. 架构概览

项目采用“纯规则＋游戏适配／UI＋加载器入口”三层结构：

```text
IMod：NextOpeningVisitorsMod
  ├─ 生命周期、日志、ModHook 订阅
  ├─ HandleFutureClientQueuePatch
  │    └─ VisitorRules → 预约队列／开店访客栈
  ├─ TradeForecastUI / NightForecastUI 启用与清理
  └─ AutoEvidenceCallController.Shutdown

电话
  ├─ PhoneCooldownResetButton → CooldownResetRules
  ├─ PhoneDialogueTextPatch → PhoneDialogueTextRules
  └─ AutoEvidencePhoneOpenPatch → 待处理报价恢复

只读预测
  ├─ CrimeForecastAdapter → 当前游戏状态快照
  ├─ CrimeForecastRules   → 纯计算
  ├─ CrimeForecastModels  → 输入／输出模型
  └─ TradeForecastUI / NightForecastUI → 文本与布局

自动证据清除
  ├─ AutoEvidenceCallState / Rules → 报价、序列化、校验、下一价格
  ├─ AutoEvidenceCallController    → 注册、持久化、电话、支付
  ├─ AutoEvidenceCallDialogPatch   → 当前 Wild 报价对话注入
  └─ AutoEvidenceCallHost          → 每 0.75 秒低频展示重试
```

纯规则通过简单模型、`IList<T>` 和委托隔离 Unity，使调度、公式、报价状态和本地化可以无游戏启动测试。游戏对象读取、UI 与真实状态写入均留在当前游戏调用链或 `MonoBehaviour.Update()` 主线程中。

## 2. 模块职责

| 文件 | 分层 | 职责 |
| --- | --- | --- |
| `NextOpeningVisitorsMod.cs` | 入口／适配 | `IMod`、日志、事件订阅、访客队列补丁、对称清理 |
| `VisitorRules.cs` | 纯规则 | 目标 ID、预约键、去重、延迟归一化、稳定置顶 |
| `CooldownResetRules.cs` | 纯规则 | 联系人目标、预约保护、冷却重置 |
| `PhoneCooldownResetButton.cs` | UI／适配 | 冷却按钮创建、可见性、游戏状态写入与反馈 |
| `PhoneDialogueTextPatch.cs` | 适配 | 对目标电话本地化结果后置替换 |
| `PhoneDialogueTextRules.cs` | 纯规则 | 21 个 key 的映射与参数化文案 |
| `ModLocalization.cs` | 纯规则 | 语言规范化、冷却按钮与结果文案 |
| `CrimeForecastModels.cs` | 纯模型 | 交易物品、交易／夜间预测输入与结果 |
| `CrimeForecastRules.cs` | 纯规则 | 案值、收尾人、衰减、调查档位、特长与取整 |
| `CrimeForecastAdapter.cs` | 只读适配 | 从 `PlayerStore`、客户、物品、服务和声望构造输入 |
| `TradeForecastUI.cs` | UI／适配 | 交易两行预测、原生布局、禁用与异常恢复 |
| `NightForecastUI.cs` | UI／适配 | 日终三行预测、动态刷新、上限警示与清理 |
| `CrimeForecastLocalization.cs` | 纯规则 | 预测、服务、电话与结果的 11 语文本 |
| `AutoEvidenceCallState.cs` | 纯规则 | 每日报价、pending、序列化、接受校验和价格增长 |
| `AutoEvidenceCallController.cs` | 适配／平台 | 服务注册、存档状态、电话状态机、支付与回滚 |

## 3. 生命周期与清理

`NextOpeningVisitorsMod`：

- `Init(ModManifest)`：创建 `ModLog`；
- `OnEnable()`：启用两个预测 UI，订阅 `OnGameLoadedLate`、`OnNewGameStarted`、`OnWakingUpLate`，记录 `v1.5.0` 初始化日志；
- `OnDisable()`：先禁用预测 UI，再退订事件，关闭自动电话控制器并记录日志。

预测 UI 的 `SetEnabled(false)` 会恢复布局并销毁新增文本；之后的 Harmony 刷新只执行隐藏／恢复，不再创建内容。自动电话 `Shutdown()` 清除 armed、接受锁、内存状态与注入对话，销毁持久宿主，并调用 `RemoveModUpgrades(Source)`。

源码不自行调用 `Harmony.PatchAll()`，补丁激活由游戏 Mod 加载环境负责。既有电话冷却按钮和 Harmony 反补丁不在本版本额外重写，完整禁用验收仍按加载器要求重启。

## 4. Harmony 补丁与事件

### 4.1 访客与旧电话功能

- `PlayerStore.HandleFutureClientQueue`：Prefix 去重与延迟归一化，Postfix 稳定置顶；
- `PhoneUIManager.OpenUI`：创建／重建冷却按钮；
- `LocHelper.GetLocalizedDialoguePhone`：替换已知 key，未知输入保持原文。

### 4.2 交易预测

- `NegociationUIManager.Rebuild` Prefix：`TradeForecastUI.Invalidate()`，清理将被原版重建的旧文本；
- `NegociationUIManager.RefreshNow` Postfix：读取当前客户和 `GetSellableItem()`，计算并刷新两行；
- 异常：记录警告并调用 `Hide()`，不阻断原版交易。

`TradeForecastUI` 保存当前 frame、原始信息区高度和分隔符状态。无可售物品时恢复原高度，不保留空白区域。

### 4.3 日终预测

- `EndOfDayUIManager.InitUI` Prefix：使旧预测实例失效；
- `EndOfDayUIManager.RefreshUI` Postfix：读取最新 `WILD_FIXER.isUsing`、等级、案值与声望状态并刷新三行；
- 达到 100 时追加警示并修改新增行颜色；
- 异常：隐藏新增内容，不影响原版日终 UI。

### 4.4 自动电话

- `ModHook.OnGameLoadedLate`：确保服务注册，读取当前 `PlayerStore.ModData()`，创建持久宿主并恢复 pending；
- `ModHook.OnNewGameStarted`：确保新档服务与状态宿主；
- `ModHook.OnWakingUpLate`：在原版新日调查结算后尝试创建当天报价；
- `StorePhoneClient.GetCallDialog` Prefix：仅在展示本服务且目标为 Wild 时返回注入 `Dialogue`；
- `PhoneUIManager.OpenUI` Postfix：若当天报价曾展示但仍 pending，且当前没有电话客户，则重新 arm，使玩家重开电话后可以再次展示；
- `AutoEvidenceCallHost.Update`：累计到 0.75 秒后调用 `TryPresentPending()`，异常安全记录并等待后续重试。

展示前会检查当前客户、地图外出、晨间报告、交易、日终、Wild 网络、现有电话、对话气泡和其他活动纸张 UI。展示失败清除注入引用；若电话由本服务打开，则尝试关闭，避免遗留空界面。

## 5. 只读预测数据流

### 5.1 交易

```text
PlayerStore.secData + 当前客户 + GetSellableItem()
  → EvidenceLevel / 当前案值副本 / 阵营 / 豁免 / 特长
  → 每个物品的协商价、赃物热度、违禁品类型
  → CrimeForecastRules.ForecastTrade()
  → 各罪名增量、当前总案值、新增总案值、成交后总案值
  → TradeForecastUI
```

适配器还遍历可售物品的子物品，并排除带 `ITEM_BEHIND_LOCK_TAG` 的子项。预测路径禁止调用 `CommitCrime()`、`CheckSoldContraband()` 或其他真实写入方法。

### 5.2 夜间

```text
真实 crimeAmount 副本 + 当前 evidenceLevel
  + WILD_FIXER.isUsing / tier
  + SEC_TRUSTED / SEC_PRIORITY / SEC_WATCHLIST / UL_PETITION
  → 收尾人副本 → 每日衰减副本 → 基础增量 → 特长修正
  → NightForecastResult
  → NightForecastUI
```

结果同时保留收尾后和衰减后的逐项列表与总计，便于定位真实日结对账差异。UI 的“预计夜间案值”显示衰减后的总案值。每次刷新都重新复制真实输入，不把上次预测作为下次输入。

## 6. 自动报价状态与持久化

稳定标识：

```text
UpgradeId = NEXT_OPENING_VISITORS_AUTO_EVIDENCE
Source    = nextopeningvisitors
SaveKey   = NextOpeningVisitors.AutoEvidence.State
Schema    = 1|lastOfferDay|hasPendingOffer|offerPrice|offerDay
```

示例：无报价的旧档默认为 `1|-1|0|0|-1`。pending 必须具有正价格和有效报价日；非 pending 时价格字段为 0、报价日为 -1。反序列化拒绝字段数错误、非整数、未知 schema 和不一致组合。

- 当前价格由自定义 `NetworkUpgrade.cost` 持有并跟随游戏升级数据保存；
- 报价状态通过 `PlayerStore.ModData()` 写入当前存档数据；
- 读档只恢复已保存 pending，不根据当前价格重新定价；
- 同一天由 `lastOfferDay` 防止重复生成；
- 新日先清理跨天 pending，再按调查与每日标记判断新报价；
- 状态缺失或损坏时使用安全默认值并记录警告，不修改现金或调查。

## 7. 接受操作与不变量

接受回调先用局部 `choiceResolved` 防止同一对话重复选择，再用 `_acceptInProgress` 防止重入。纯规则重新校验 pending、日期、调查和现金。

成功写入仅限：

```text
playerCash -= offerPrice
secData.evidenceLevel = 0
secData.state = 0
service.cost = Math.Round(offerPrice * 1.2)
state.CompleteAcceptance()
ModData()[SaveKey] = state.Serialize()
```

代码保存修改前的现金、调查、案件状态、服务价格与报价字符串；异常时尝试恢复并记录错误。该回滚是适配层最佳努力，不替代真实游戏内故障验收。

以下状态不属于接受操作写入目标：`crimeAmount`、`crimeAmountTotal`、未来访客队列、当前访客栈、当前治安官、普通盘查、`wildFavor` 和原版 `EVIDENCE_REMOVAL` 价格。

## 8. UI、本地化与错误策略

- 使用 TextMeshPro、原生 Tablet UI 字体／颜色／布局；
- 交易新增高度 54，日终新增高度 86；恢复时使用捕获的原版高度和分隔符状态；
- 新增文本使用固定对象名，重建前销毁，避免重复；
- 11 语文案集中管理，未知语言回退英文；
- 预测异常不影响原版交易或睡觉；自动来电失败不修改现金或罪案；
- 日志记录初始化、报价创建／展示、接受结果与异常，不在每次预测刷新写逐帧日志。

## 9. 构建、测试与产物边界

```text
build.py
  → 便携 .NET + Roslyn csc.dll
  → 引用 Probably Stolen_Data/Managed/*.dll（不由发布包携带）
  → 编译 src/*.cs + Properties/AssemblyInfo.cs
  → build/NextOpeningVisitors.dll（另生成开发用 PDB）
  → 复制 DLL 与 manifest.xml 到 Mods/NextOpeningVisitors/
```

程序集目标为 `.NETStandard,Version=v2.1`。测试宿主单独使用 .NET 8 引用包运行纯规则并调用官方 `ModScanner`，测试宿主框架不等于 Mod 运行时。

- `src/`：Mod 源码事实源；
- `tests/`：规则测试与扫描宿主，不发布；
- `analysis/`：版本适配分析材料，不发布；
- `build/`：编译与本地安装来源，不发布；
- `release/NextOpeningVisitors/`：仅清单与 Mod DLL；
- `local_mod_backup`：不属于本次交付，不修改。
