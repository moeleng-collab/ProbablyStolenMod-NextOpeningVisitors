# 夜间案值与调查预测

- 当前状态：源码已实现；游戏内专项验收未执行
- 目标游戏：《Probably Stolen》`0.46D`

## 行为规格

### 显示与动态切换

日终平板新增三行：

```text
预计夜间案值：1000 → 780
预计调查变化：+6
次日调查进度：80 → 86/100
```

达到 100 时追加“将达到调查上限”警示并标红。切换夜间收尾人后，按最新 `isUsing` 和等级重算。每次预测都从真实案值副本开始，反复切换不累计削减，不扣服务费、不写入案值、不生成夜间日志、不派遣治安官或保存游戏。

### 原版结算与预测顺序

真实流程的关键顺序：

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

预测只计算“收尾人 → 衰减 → 调查增量”的数值，不执行上述真实写入：

```text
当前案值快照
  → 若勾选收尾人：按等级逐项削减
  → 若结算前调查 < 80：每项再衰减 20，最低 0
  → 按剩余总案值求基础调查增量
  → 应用声望／特长修正
  → 加到当前调查并截断到 100
```

### 收尾人

| 收尾人等级 | 百分比部分 | 再固定减去 |
| --- | ---: | ---: |
| 1 | 当前值的 15%，按原版整数转换 | 50 |
| 2 | 当前值的 20%，按原版整数转换 | 150 |
| 3 | 当前值的 25%，按原版整数转换 | 400 |

每项先执行百分比整数转换，再固定削减并截断到 0。不能改写成 `Round(amount × (1 - percent)) - flat`，整数转换位置不同会产生边界差异。原版服务费 25／45／70 不在预测中扣除。

### 每日调查公式

| 收尾与衰减后的总案值 | 基础增量 |
| --- | ---: |
| 0～99 | +0 |
| 100～299 | +2 |
| 300～849 | +6 |
| 850～1199 | +9 |
| 1200～1499 | +12 |
| 1500～2499 | +20 |
| 2500 及以上 | +33 |

修正顺序：`SEC_TRUSTED` 整数除以 2；`SEC_PRIORITY` 乘 1.5，否则 `SEC_WATCHLIST` 乘 1.2；最后 `UL_PETITION` 乘 1.5。乘法采用与 `RNG.MultiplyAndRound()` 一致的 `Math.Round`。

预测仅覆盖本次收尾人与随后 `SecData.OnNewDay()` 的直接罪案结算，不预测随机盘查或其他 Mod 的额外修改。

## 实现位置与数据流

| 文件 | 职责 |
| --- | --- |
| `src/CrimeForecastModels.cs` | 夜间输入、逐项结果与上限标记 |
| `src/CrimeForecastRules.cs` | 收尾人、衰减、调查档位、特长与取整 |
| `src/CrimeForecastAdapter.cs` | 当前案值副本、服务勾选与声望状态只读适配 |
| `src/NightForecastUI.cs` | 三行预测、动态刷新、上限警示与清理 |
| `src/CrimeForecastLocalization.cs` | 预测文本与参数格式化 |

```text
真实 crimeAmount 副本 + 当前 evidenceLevel
  + WILD_FIXER.isUsing / tier
  + SEC_TRUSTED / SEC_PRIORITY / SEC_WATCHLIST / UL_PETITION
  → 收尾人副本 → 每日衰减副本 → 基础增量 → 特长修正
  → NightForecastResult
  → NightForecastUI
```

结果保留收尾后和衰减后的逐项列表与总计，便于和真实日结对账。“预计夜间案值”显示衰减后的总案值。每次刷新都重新复制真实输入，不把上次预测作为下次输入。

补丁与生命周期：

- `EndOfDayUIManager.InitUI` Prefix：使旧预测实例失效；
- `EndOfDayUIManager.RefreshUI` Postfix：读取最新 `WILD_FIXER.isUsing`、等级、案值与声望状态并刷新；
- 新增三行高度合计 86；达到 100 时只修改本 Mod 新增调查行的文本与颜色；
- 异常隐藏新增内容，不影响原版日终 UI。

预测路径不得调用 `SecData.OnFixerUsed()`、`OnNewDay()`、`PlayerStore.EndNight()`、`BeginDay()`、`SaveGame()` 或任何真实队列、现金、日志和声望写入方法。

## 验证与验收

当前全局自动化结果见[验证记录](../validation/current.md)。

### 纯规则覆盖

- 三种收尾人等级按“百分比整数转换后固定削减”计算；
- 调查 `<80` 逐项衰减 20，`80` 不衰减，结果不为负；
- 调查档位边界 `99／100／299／300／849／850／1199／1200／1499／1500／2499／2500`；
- `SEC_TRUSTED`、`SEC_PRIORITY`、`SEC_WATCHLIST`、`UL_PETITION` 的组合、优先级与取整；
- 次日调查截断到 100 并产生上限警示结果；
- 同一真实输入只生成新结果，不执行真实服务或日结。

尚未覆盖所有声望组合的组合爆炸，也未与真实睡觉结果逐项对账。

### 游戏内专项验收

- [ ] 三种收尾人等级与未勾选状态均和真实日结逐项一致。
- [ ] 勾选／取消后案值、增量、次日进度和 100 警示同帧链路更新。
- [ ] 连续切换和重开界面不累计预测削减、不重复扣费。
- [ ] 预测与真实 `EndNight() → BeginDay()` 后案值及调查一致。
- [ ] 预测自身不生成夜间日志、派遣治安官或保存游戏。
- [ ] 常用分辨率、UI 缩放和中英文长文本无裁切或错位。
