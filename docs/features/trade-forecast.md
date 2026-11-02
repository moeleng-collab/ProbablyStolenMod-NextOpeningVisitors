# 交易案值与调查预测

- 当前状态：源码已实现；游戏内专项验收未执行
- 目标游戏：《Probably Stolen》`0.46D`

## 行为规格

### 显示与刷新

在玩家向当前客户出售物品的交易平板中新增两行：

```text
调查进度：80/100
案值：450 → 650（+200）
```

没有当前客户、罪案数据或可出售物品时隐藏新增区域，不保留额外高度。合法物品仍可显示 `+0`。预测覆盖当前 `GetSellableItem()` 返回的物品，以及其中未带 `ITEM_BEHIND_LOCK_TAG` 的子物品。物品价格、客户阵营、证据豁免、赃物／违禁品双标签与开局特长均从当前快照读取。

原版信息区高度为 0 时会隐藏容器但保留旧 `sizeDelta`。Mod 此时必须按 0 捕获基础高度，确保反复放上、取下物品不会累计预测区域高度或移动文案。

### 计算顺序

若客户 `isNoContributeToEvidence`，全部新增案值为 0。否则每个物品分别计算：

1. **赃物**：协商成交价 × `STOLEN_VALUE_INT / 100`，按原版 `Math.Round` 顺序折算，再应用阵营和开局特长，计入 `FENCING`；
2. **违禁品**：协商成交价依次应用阵营和开局特长，按类型计入毒品、武器、门禁卡或普通违禁品案值；
3. 同一物品同时具有两种属性时，两部分独立累加；
4. 汇总各罪名增量，显示当前总案值与成交后总案值。

阵营修正：

| 阵营条件 | 修正 |
| --- | --- |
| `FACTION_BLACK_MARKET` | 整数除以 2 |
| `FACTION_REVOLUTION` | 整数除以 3 |
| `FACTION_LOWER_LEVEL` 且有 `LLTIGHTLIPS` | 整数除以 2 |
| `FACTION_CARTEL` | 0 |
| 其他 | 不变 |

开局特长按 Intimidating 后 Blabbermouth 的顺序及原版取整规则处理。`MultiplyAndRound()` 使用 `(int)Math.Round(value * multiplier)`，对应默认 `RNG.MultiplyAndRound()` 行为；浮点常量保留原版单精度提升后的数值，以减少边界差异。

调查行只显示真实 `evidenceLevel`，不根据本次交易预先增加调查。

## 实现位置与数据流

| 文件 | 职责 |
| --- | --- |
| `src/CrimeForecastModels.cs` | 交易物品、输入与输出模型 |
| `src/CrimeForecastRules.cs` | 纯案值公式、阵营与特长修正 |
| `src/CrimeForecastAdapter.cs` | 从 `PlayerStore`、当前客户和可售物品构造只读输入 |
| `src/TradeForecastUI.cs` | 两行预测、原生布局、禁用与异常恢复 |
| `src/CrimeForecastLocalization.cs` | 预测文本与参数格式化 |

```text
PlayerStore.secData + 当前客户 + GetSellableItem()
  → EvidenceLevel / 当前案值副本 / 阵营 / 豁免 / 特长
  → 每个物品的协商价、赃物热度、违禁品类型
  → CrimeForecastRules.ForecastTrade()
  → 各罪名增量、当前总案值、新增总案值、成交后总案值
  → TradeForecastUI
```

补丁与生命周期：

- `NegociationUIManager.Rebuild` Prefix：调用 `TradeForecastUI.Invalidate()`，清理将被原版重建的旧文本；
- `NegociationUIManager.RefreshNow` Postfix：读取当前客户和 `GetSellableItem()`，计算并刷新两行；
- frame 改变时先恢复旧布局；没有 frame、infoHost、客户或可售物品时隐藏预测；
- 新增两行高度合计 54，保存原始信息高度及分隔符状态，隐藏或禁用时恢复；
- 异常记录警告并调用 `Hide()`，不阻断原版交易。

预测路径禁止调用 `SecData.CommitCrime()`、`CheckSoldContraband()`，或任何队列、现金、日志、声望及存档写入方法。预测不保证重新鉴定、交易锁定、预算检查或其他 Mod 检查后一定成交。

## 验证与验收

当前全局自动化结果见[验证记录](../validation/current.md)。

### 纯规则覆盖

- 黑市、革命、下层／`LLTIGHTLIPS`、卡特尔和其他阵营修正；
- 武器、毒品、门禁卡和普通违禁品罪名映射；
- 赃物与违禁品双标签分别贡献并汇总；
- Intimidating 后 Blabbermouth 的顺序与 `Math.Round` 边界；
- 合法物品为 `+0`，证据豁免客户全部贡献为 0；
- 输入模型在预测后保持不变。

这些断言不验证真实 `GetSellableItem()` 集合、子物品交易行为或实际 UI 刷新。

### 游戏内专项验收

- [ ] 单件、批量和预算不足确认后的预测集合与真实成交集合一致。
- [ ] 子物品和锁定子物品处理与原版一致。
- [ ] 赃物、违禁品、双标签、阵营、证据豁免与特长结果逐项等于真实成交。
- [ ] 切换物品、协商价、交易模式、成交和预算确认后 UI 立即刷新。
- [ ] 无可售物品时不保留额外空白，合法物品显示 `+0`。
- [ ] 交租日商人页面反复放上、取下物品后，预测文案位置固定，信息区高度不累计。
- [ ] 仅打开或刷新界面不改变案值、调查、现金、队列或存档。
- [ ] 常用分辨率、UI 缩放和中英文长文本无裁切或错位。
