# NextOpeningVisitors 技术手册

- 当前实现：`1.5.0`
- 基线版本：`1.4.5`
- 目标游戏：《Probably Stolen》`0.46D`
- 运行环境：Unity `2021.3.45f2`、Mono、`.NET Standard 2.1`
- 状态：功能源码已实现；纯规则、构建、官方扫描已通过；游戏内验收未执行

相关文档：[README](../README.md)、[现有行为规格](behavior-spec.md)、[现有架构](architecture.md)、[1.5.0 更新说明](v1.5.0-update.md)、[测试说明](testing.md)。

## 1. 工程约定

### 1.1 事实源优先级

1. 当前游戏版本的公开 Mod API 与可观察行为；
2. 当前项目 `src/`；
3. 当前游戏 `Assembly-CSharp.dll` 的版本适配分析；
4. 本手册中的规则整理；
5. 玩家描述用于提出需求，不代替代码或运行核验。

游戏更新后必须重新核对方法签名、字段、常量与结算顺序。不得把 `analysis/` 中的反编译代码复制进 Mod 源码或发布包。

### 1.2 分层与线程

- **纯规则层**：案值与调查计算、报价状态、价格和本地化，不访问 Unity 场景；
- **游戏适配层**：读取 `SecData`、物品、客户、服务和特长，并在明确分支写入真实状态；
- **UI／平台层**：Harmony、`IMod`、电话、UI、日志、注册与 `PlayerStore.ModData()`。

游戏对象访问和 UI 修改保持在游戏主线程。预测只读真实对象并写入临时模型。持久宿主仅做低频主线程重试，不引入后台线程或跨线程游戏对象访问。

### 1.3 版本一致性

当前已同步：

- `manifest.xml`：`ModVersion = 1.5.0`；
- `AssemblyInfo.cs`：程序集 `1.5.0.0`、信息版本 `1.5.0`；
- 初始化日志：`v1.5.0`；
- README、行为、架构、测试、Workshop 文案和本手册。

## 2. 当前实现地图

| 领域 | 目标／入口 | 当前实现 |
| --- | --- | --- |
| 访客队列 | `PlayerStore.HandleFutureClientQueue` | Prefix 去重与归一化，Postfix 稳定置顶 |
| 电话冷却按钮 | `PhoneUIManager.OpenUI` | 创建按钮并控制可见性 |
| 电话文案 | `LocHelper.GetLocalizedDialoguePhone` | 替换目标 key |
| 交易预测 | `NegociationUIManager.Rebuild / RefreshNow` | 失效清理与只读刷新 |
| 日终预测 | `EndOfDayUIManager.InitUI / RefreshUI` | 失效清理与动态刷新 |
| 自动报价对话 | `StorePhoneClient.GetCallDialog` | 仅对当前 Wild 服务展示注入对话 |
|  pending 重开恢复 | `PhoneUIManager.OpenUI` | 当天未处理报价重新 arm |
| 新日与读档 | `OnWakingUpLate / OnGameLoadedLate / OnNewGameStarted` | 注册、状态恢复与报价创建 |
| 低频展示 | `AutoEvidenceCallHost.Update` | 每 0.75 秒安全重试 |

当前源码文件职责和完整补丁流见[架构](architecture.md)。

## 3. 原版罪案机制依据

本章依据当前游戏 `0.46D` 的 `SecData`、`PlayerStore`、`NegociationUIManager`、`NetworkUpgradeList`、`EndOfDayUIManager` 和 `StoreClientListSecurity` 分析。真实运行一致性仍需游戏内对账。

### 3.1 关键字段

```text
SecData.crimeTypes        罪名 ID 列表
SecData.crimeAmount       各罪名当前案值
SecData.crimeAmountTotal  各罪名历史累计案值
SecData.evidenceLevel     调查进度
SecData.state             判决派遣状态
```

罪名列按相同索引对应。当前预测公式只依赖 `crimeAmount` 的顺序副本与合计；收尾人和每日衰减逐项执行，不能只保存一个无序总数后再还原。

### 3.2 交易案值

`SecData.CheckSoldContraband()` 是售卖赃物／违禁品的核心写入点：

- 客户 `isNoContributeToEvidence` 时直接返回；
- 赃物与违禁品是两个独立判断，同一物品可各贡献一次不同罪名；
- 实际写入通过 `CommitCrime()`，后者应用开局特长。

赃物：

```text
基础值 = 当前协商成交价
热度折算 = Round(基础值 × STOLEN_VALUE_INT / 100)
阵营修正 = ApplyCrimeModifier(clientFaction, 热度折算)
特长修正 = Intimidating / Blabbermouth 顺序
罪名 = FENCING
```

违禁品使用完整协商成交价，先阵营后特长，罪名按 `SUBSTANCE / WEAPON / ACCESS_CARD / 其他违禁品` 映射。

| 阵营条件 | 修正 |
| --- | --- |
| `FACTION_BLACK_MARKET` | 整数除以 2 |
| `FACTION_REVOLUTION` | 整数除以 3 |
| `FACTION_LOWER_LEVEL` 且有 `LLTIGHTLIPS` | 整数除以 2 |
| `FACTION_CARTEL` | 0 |
| 其他 | 不变 |

| 开局特长 | 修正 |
| --- | --- |
| `StartingPerk.IsIntimidating()` | 乘约 0.67 并按原版取整 |
| `StartingPerk.IsBlabbermouth()` | 乘约 1.33 并按原版取整 |

`CrimeForecastRules.MultiplyAndRound()` 使用 `(int)Math.Round(value * multiplier)`，对应默认 `RNG.MultiplyAndRound()` 行为。浮点常量保留原版单精度提升后的数值，以减少边界差异。

### 3.3 夜间真实结算顺序

```text
PlayerStore.EndNight()
  → 收取已选夜间服务费用
  → HandleOtherServices()
       → 若收尾人 isUsing：SecData.OnFixerUsed()
PlayerStore.BeginDay()
  → StoreClientManager.OnNewDay()
  → SecData.OnNewDay()
       → 每日案值衰减
       → 基础调查增量
       → 特长修正
       → evidenceLevel 截断到 100
       → 必要时 SendOfficer()
```

预测必须按“收尾人 → 衰减 → 调查增量”计算，不能先按原案值求调查再减收尾效果。

### 3.4 收尾人

`NetworkUpgrade.GetWildFixerTier()` 返回 1～3。当前规则对每项：

```csharp
if (useFixer)
{
    if (amount > 0) amount -= (int)(amount * percent);
    else amount = 0;
    amount -= flat;
}
amount = Math.Max(0, amount);
```

数组：

```text
percent = [0.15, 0.20, 0.25]
flat    = [50, 150, 400]
fee     = [25, 45, 70]  // 仅原版真实结算收取，预测不扣
```

不能改写成 `Round(amount × (1 - percent)) - flat`，整数转换位置不同会产生边界差异。

### 3.5 每日调查公式

若真实当前 `evidenceLevel < 80`，每项案值减 20 并截断到 0。按衰减后总案值取得基础增量：

```text
[0, 100)       → 0
[100, 300)     → 2
[300, 850)     → 6
[850, 1200)    → 9
[1200, 1500)   → 12
[1500, 2500)   → 20
[2500, +∞)     → 33
```

修正顺序：

```text
SEC_TRUSTED  → delta /= 2（整数除法）
SEC_PRIORITY → Round(delta × 1.5)
否则 SEC_WATCHLIST → Round(delta × 1.2)
UL_PETITION  → Round(delta × 1.5)
evidenceLevel = Min(100, evidenceLevel + delta)
```

预测不生成原版夜间日志，不调用 `SendOfficer()`。

### 3.6 派遣、判决与原版清除

`SecData.SendOfficer()` 在 `state == 0` 且总案值大于 0 时设置 `state = 1` 并排队 `securityOfficer`。原版治安官对话按当时状态决定证据消失、罚款、监禁或处决；本 Mod 不重写这些分支。

原版 `EVIDENCE_REMOVAL` 清除 `evidenceLevel` 和 `state`，每次价格 `+100`，有自身冷却。自动服务只复用“两项字段清零”的语义，使用独立 ID 和乘法价格，不能调用原版升级 `Unlock()`。

普通盘查由 `ContrabandHelper.StartInspection()` 等机制处理，与判决上门不同。预测不预测随机盘查，自动电话也不撤销盘查。

## 4. 只读预测实现

### 4.1 输入与输出

交易输入包含当前调查、案值副本、阵营、证据豁免、`LLTIGHTLIPS`、两个开局特长，以及物品的协商价、赃物标记／热度和违禁品类型。

夜间输入包含当前调查、案值副本、收尾人勾选与等级，以及 `SEC_TRUSTED / SEC_PRIORITY / SEC_WATCHLIST / UL_PETITION`。

输出保留各罪名交易增量、交易前后总案值、收尾后／衰减后逐项列表和总计、基础与修正后调查增量、次日调查和上限标记，便于测试和对账。

### 4.2 当前物品集合

`CrimeForecastAdapter.ForecastCurrentTrade()` 使用当前 `NegociationUIManager.GetSellableItem()`，而不是扫描整个后仓。对每个返回物品调用 `AddItem()`，再遍历 `FindAllChildItems()`，排除带 `ITEM_BEHIND_LOCK_TAG` 的子项。

这一集合与真实成交的一致性属于游戏内验收重点；纯规则测试无法证明原版后续检查、预算流程或其他 Mod 不会更改最终成交集合。

### 4.3 禁止调用

预测路径不得调用：

- `SecData.CommitCrime()`、`CheckSoldContraband()`；
- `SecData.OnFixerUsed()`、`OnNewDay()`；
- `PlayerStore.EndNight()`、`BeginDay()`、`SaveGame()`；
- 任何队列、现金、日志或声望写入方法。

当前预测适配器只构造输入并调用纯规则；UI 仅写新增文本、颜色和布局。

## 5. 预测 UI 生命周期

### 5.1 交易

`Rebuild` 前销毁新增行并清空 frame 引用，`RefreshNow` 后重新读取当前 frame。frame 改变时先恢复旧布局；没有 frame、infoHost、客户或可售物品时不显示预测。

新增两行高度合计 54，使用 `TabletUI.NewText()`、`TabletUI.AutoSize()` 和 `TabletUI.LayoutSection()`。保存原始信息高度及分隔符状态，隐藏或禁用时恢复。

### 5.2 日终

`InitUI` 前清理旧行，`RefreshUI` 后更新三行，新增高度 86。切换收尾人由原版先更新 `isUsing` 再触发 `RefreshUI`，因此后置补丁读取的是新状态。

达到 100 时只修改本 Mod 新增调查行的文本与颜色。预测器不修改 `isUsing`，不重复调用服务费逻辑。

### 5.3 禁用安全

两个 UI 都有 `_enabled`。`OnEnable()` 设为 true；`OnDisable()` 先设为 false 并执行清理。之后即使 Harmony Postfix 仍被调用，也只执行 `Hide()`／恢复布局，不会重新创建文本。

## 6. 自动服务注册与价格

稳定 ID：

```text
NEXT_OPENING_VISITORS_AUTO_EVIDENCE
source = nextopeningvisitors
```

定义使用 `cost = 1000`、`state = 2`、`isRepeatable = true`、零冷却，并复用原版证据清除图标。标题、说明和已启用状态通过 provider 本地化，状态文本读取当前运行时 `cost`。

`NetworkUpgrade.RegisterModUpgrade(definition, null)` 用于注册；没有 `UnlockActions`，所以服务界面不会直接收费。真实支付只由电话接受回调执行。禁用时 `RemoveModUpgrades(Source)` 清理运行时定义，不主动重写罪案字段。

下一价格：

```text
Math.Round(offerPrice × 1.2)
```

初始序列 `1000、1200、1440、1728……`。非正价格抛出参数异常，超过 `int.MaxValue` 抛出溢出异常；接受失败不得应用涨价。

## 7. 报价状态机与持久化

### 7.1 实际 schema

```text
SaveKey = NextOpeningVisitors.AutoEvidence.State
Schema  = 1|lastOfferDay|hasPendingOffer|offerPrice|offerDay
```

| 字段 | 默认 | 含义 |
| --- | ---: | --- |
| schema | 1 | 当前状态格式版本 |
| `lastOfferDay` | -1 | 最近创建报价的游戏日 |
| `hasPendingOffer` | 0 | 是否存在未处理报价 |
| `offerPrice` | 0 | pending 锁定价格；非 pending 为 0 |
| `offerDay` | -1 | pending 所属日；非 pending 为 -1 |

`TryDeserialize()` 要求五个字段、已知 schema、不变文化整数和一致 pending 组合。缺失键返回默认状态；损坏数据回退默认并记录警告。

### 7.2 状态迁移

```text
新日
  → 跨天 pending 失效
  → 调查 < 100：清理 pending，不创建
  → 已有 pending / 当天已创建 / 价格无效：不重复创建
  → 否则锁定 day 与当前 cost，设置 pending 和 lastOfferDay

pending
  ├─ UI 忙：等待低频重试
  ├─ 调查已被其他方式清除或日期失效：取消，不扣款、不涨价
  ├─ 拒绝：清 pending，保留 lastOfferDay
  ├─ 余额不足：保留 pending，不扣款、不涨价
  ├─ 未选择关闭：保留 pending；当天重开电话可重新 arm
  └─ 接受且校验通过：扣款、清调查、涨价、清 pending
```

读档只恢复 pending 并 arm；`_presentedOfferDay` 重置，不在 `LoadState()` 内生成或重新定价。`PhoneUIManager.OpenUI` 后置处理只针对“当天曾展示但仍 pending 且当前无电话客户”的状态重新 arm，避免宿主在电话关闭时反复自动弹出。

### 7.3 存档所有权

当前价格存放在自定义 `NetworkUpgrade.cost`，报价状态存入当前 `PlayerStore.ModData()`。没有使用跨存档全局文本文件，也没有直接 ES3 文件路径写入，因此清单不需要文件能力。

这依赖游戏自身对自定义升级和 `ModData()` 的槽位／历史存档保存语义。两个存档隔离、历史读档回滚和写档失败恢复仍必须真实验收，不能仅凭 API 名称宣称通过。

## 8. 自动电话适配与接受幂等

展示时构造原生 `Dialogue` 与两个选择。`AutoEvidenceCallDialogPatch.InjectedDialog` 只在 `StartPhoneDialog(WILD_CLIENT_NUMBER)` 的同步调用期间设置，`finally` 清理；Prefix 仅拦截 Wild，其他联系人走原版。

`TryPresentPending()` 在 UI 忙时等待，不覆盖其他电话或模态界面。展示成功后记录 `_presentedOfferDay` 并 disarm，避免同一天反复自动弹出。失败不修改现金、调查或价格。

接受有局部 `choiceResolved` 和静态 `_acceptInProgress` 两层防重入。写入前重新校验：

```text
HasPendingOffer == true
OfferDay == 当前日
evidenceLevel >= 100
OfferPrice > 0
playerCash >= OfferPrice
```

成功写入：

```csharp
store.playerCash -= state.OfferPrice;
store.secData.evidenceLevel = 0;
store.secData.state = 0;
service.cost = increasedPrice;
state.CompleteAcceptance();
SaveState();
```

适配器保存旧现金、调查、案件状态、服务价格与报价字符串，在异常时尝试恢复。代码没有 `ResetCrime()` 或治安官队列移除调用。不得把“调查清零”解释为取消已派遣访客或永久免于判决。

## 9. 本地化、日志与失败策略

`CrimeForecastLocalization` 含 19 类文本，复用 `ModLocalization.NormalizeLocale()`，支持：

```text
en, zh, ja, de, ru, fr, es, pl, ptBR, ko, it
```

价格、案值和进度使用参数格式化。长文本、电话气泡宽度、自动换行、字体回退和按钮遮挡必须实际渲染检查。

日志覆盖初始化、报价创建／展示、接受结果、拒绝和异常。预测刷新不逐次写日志。失败原则：

- 预测失败不影响原版交易或睡觉；
- 注册失败不影响原版王哥界面；
- 来电失败不修改现金或罪案；
- 接受异常尝试回滚并记录错误；
- 状态损坏使用安全默认值，不静默改动调查或现金；
- 任何异常都不通过删除治安官“修复”状态。

## 10. 验证证据与剩余门槛

当前可复现命令：

```bash
python Modding/NextOpeningVisitors/run_tests.py
python Modding/NextOpeningVisitors/build.py
python Modding/NextOpeningVisitors/run_scan.py
```

当前结果：818 条断言通过；编译及本地安装成功；官方扫描 160 个方法，`Capabilities: <none>`、`HardFlags: 0`、`Findings: 0`、`Refused: False`。

这些结果只证明纯规则、引用可构建性和当次扫描结果。以下仍未测试：

1. 三个功能的真实 UI、字体、布局与交互；
2. 交易集合和夜间公式与真实结算逐项对账；
3. 电话展示、重开、拒绝、余额不足和重复点击；
4. 新档、旧档、槽位隔离、历史读档与禁用／重启用；
5. 中英文渲染及其他 Mod 兼容组合。

发布包只应包含 `manifest.xml` 与 `NextOpeningVisitors.dll`，不得包含 PDB、游戏 DLL、`analysis/`、测试、日志或本地设置。本版本未上传 Workshop。
