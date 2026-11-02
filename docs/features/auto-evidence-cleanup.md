# 王哥自动证据清除

- 当前状态：源码已实现；游戏内专项验收未执行
- 目标游戏：《Probably Stolen》`0.46D`

## 行为规格

### 服务与触发

自定义服务 ID 为 `NEXT_OPENING_VISITORS_AUTO_EVIDENCE`，在王哥服务界面的 `Informant`（线人）栏目显示自动服务说明和下一次接受价格，与原版清除证据服务归入同一栏目。服务无额外解锁费、无 14 天冷却，也不提供界面直接购买动作；实际支付只发生在自动来电的接受分支。

在 `OnWakingUpLate` 对应的新日阶段，以及 `OnGameLoadedLate` 读档恢复时，同时满足以下条件时创建报价：

- `evidenceLevel >= 100`；
- 当天尚未生成过报价；
- 当前没有待处理报价；
- 当前服务价格有效。

每个游戏日最多创建一次。报价锁定当日与当时价格。持久宿主每 0.75 秒低频尝试展示，在晨间报告、交易、日终、地图外出、其他电话或其他模态 UI 占用时等待，不覆盖当前交互。

### 来电与分段

来电正文分为三段连续对话，依次说明现状与风险、价格与处理内容、保留项与确认，避免长文本在单行气泡中被截断。价格只在第二段格式化，接受／拒绝选项只出现在第三段。

分段遵循原版 `EmporiumOption.autoNextDialog`：开启时自动继续，关闭时点击继续；不另设计时器，也绝不自动选择接受或拒绝。电话完整说明价格及“只清调查与案件状态，保留案值和已派遣治安官”。

### 接受与价格

接受前重新检查：有待处理报价、报价日期为当天、调查仍达到 100、价格大于 0、`playerCash` 足够。校验通过后：

1. 从 `playerCash` 扣除锁定报价；
2. 将 `evidenceLevel` 与 `secData.state` 设为 0；
3. 完成本次报价，阻止重复接受；
4. 下一价格更新为 `Math.Round(当前价格 × 1.2)`；
5. 保存报价状态并显示下一价格。

首次价格为 1000，序列从 `1000、1200、1440、1728……` 开始。只有成功接受才扣款和涨价；拒绝、余额不足、失效、异常均不涨价。价格溢出或无效时拒绝操作。本服务不消耗 `wildFavor`，与原版 `EVIDENCE_REMOVAL` 的 `+100` 价格曲线独立。

### 拒绝、中断与跨天

- 拒绝后当天不再自动生成，次日满足条件可再次来电，价格不变；
- 余额不足保留待处理报价；
- 未作选择关闭电话也保留报价，当天重新打开电话时可再次展示；
- 读档恢复待处理报价时不重新定价；
- 跨天旧报价失效，再按新日条件决定是否创建；
- 调查已被其他方式清除或日期失效时取消 pending，不扣款、不涨价。

### 明确保留的原版状态

成功接受不得修改：

- `crimeAmount` 与 `crimeAmountTotal`；
- 未来访客队列中的 `securityOfficer`；
- 当前开店队列或当前访客中的治安官；
- 普通盘查安排；
- `wildFavor`；
- 原版手动证据清除服务价格。

不得调用 `ResetCrime()` 或移除对应治安官。若已派遣治安官之后到访，完全交给原版处理；调查降低后可能进入“证据消失”对话，因此本服务不是永久免于判决。

原版 `EVIDENCE_REMOVAL` 清除 `evidenceLevel` 和 `state`，每次价格 `+100`，有自身冷却。自动服务只复用“两项字段清零”的语义，使用独立 ID 和乘法价格，不能调用原版升级 `Unlock()`。

## 实现位置

| 文件 | 职责 |
| --- | --- |
| `src/AutoEvidenceCallState.cs` | 每日报价、pending、序列化、接受校验和价格增长纯规则 |
| `src/AutoEvidenceCallController.cs` | 服务注册、存档状态、电话状态机、支付、回滚与持久宿主 |

主要补丁与事件：

- `ModHook.OnGameLoadedLate`：确保服务注册，读取当前 `PlayerStore.ModData()`，检查并补建当天报价，创建宿主并恢复 pending；
- `ModHook.OnNewGameStarted`：确保新档服务与状态宿主；
- `ModHook.OnWakingUpLate`：在原版新日调查结算后尝试创建当天报价；
- `StorePhoneClient.GetCallDialog` Prefix：仅在展示本服务且目标为 Wild 时返回注入 `Dialogue`；
- `PhoneUIManager.OpenUI` Postfix：若当天报价曾展示但仍 pending，且当前没有电话客户，则重新 arm；
- `AutoEvidenceCallHost.Update`：每 0.75 秒调用 `TryPresentPending()`，异常安全记录并等待后续重试。

`AutoEvidenceCallDialogPatch.InjectedDialog` 只在 `StartPhoneDialog(WILD_CLIENT_NUMBER)` 的同步调用期间设置，`finally` 清理；其他联系人走原版。展示成功后记录 `_presentedOfferDay` 并 disarm，避免同一天反复自动弹出。展示失败清除注入引用；若电话由本服务打开，则尝试关闭，避免遗留空界面。

服务定义使用 `cost = 1000`、`state = 2`、`isRepeatable = true`、零冷却，并复用原版证据清除图标。`NetworkUpgrade.RegisterModUpgrade(definition, null)` 不包含 `UnlockActions`，所以服务界面不会直接收费。禁用时 `RemoveModUpgrades(Source)` 清理运行时定义，不主动重写罪案字段。

## 状态机与持久化

稳定标识：

```text
UpgradeId = NEXT_OPENING_VISITORS_AUTO_EVIDENCE
Source    = nextopeningvisitors
SaveKey   = NextOpeningVisitors.AutoEvidence.State
Schema    = 1|lastOfferDay|hasPendingOffer|offerPrice|offerDay
```

| 字段 | 默认 | 含义 |
| --- | ---: | --- |
| schema | 1 | 当前状态格式版本 |
| `lastOfferDay` | -1 | 最近创建报价的游戏日 |
| `hasPendingOffer` | 0 | 是否存在未处理报价 |
| `offerPrice` | 0 | pending 锁定价格；非 pending 为 0 |
| `offerDay` | -1 | pending 所属日；非 pending 为 -1 |

`TryDeserialize()` 要求五个字段、已知 schema、不变文化整数和一致 pending 组合。缺失键返回默认状态；损坏数据回退默认并记录警告，不主动修改现金或调查。

```text
新日
  → 跨天 pending 失效
  → 调查 < 100：清理 pending，不创建
  → 已有 pending / 当天已创建 / 价格无效：不重复创建
  → 否则锁定 day 与当前 cost，设置 pending 和 lastOfferDay

pending
  ├─ UI 忙：等待低频重试
  ├─ 调查已清或日期失效：取消，不扣款、不涨价
  ├─ 拒绝：清 pending，保留 lastOfferDay
  ├─ 余额不足：保留 pending，不扣款、不涨价
  ├─ 未选择关闭：保留 pending；当天重开电话可重新 arm
  └─ 接受且校验通过：扣款、清调查、涨价、清 pending
```

当前价格存放在自定义 `NetworkUpgrade.cost`，报价状态存入当前 `PlayerStore.ModData()`。没有使用跨存档全局文本文件，也没有直接 ES3 文件路径写入。两个存档隔离、历史读档回滚和写档失败恢复仍必须真实验收，不能仅凭 API 名称宣称通过。

## 接受幂等与回滚

接受回调先用局部 `choiceResolved` 防止同一对话重复选择，再用 `_acceptInProgress` 防止重入。代码保存修改前的现金、调查、案件状态、服务价格与报价字符串；异常时尝试恢复并记录错误。该回滚是适配层最佳努力，不替代真实游戏内故障验收。

成功写入仅限：

```text
playerCash -= offerPrice
secData.evidenceLevel = 0
secData.state = 0
service.cost = Math.Round(offerPrice * 1.2)
state.CompleteAcceptance()
ModData()[SaveKey] = state.Serialize()
```

## 验证与验收

当前全局自动化结果见[验证记录](../validation/current.md)。

### 纯规则与本地化覆盖

- 调查不足 100 不创建，达到 100 创建初始 1000 报价；
- 同日生成幂等，拒绝当天不重建，次日可重建；
- 成功后下一报价使用增长价格，跨天 pending 失效；
- 序列化／反序列化保留报价日、价格和每日标记；
- 读取 pending 本身不重新生成或重新定价，损坏数据安全回退；
- 现金恰好足够可接受，不足时保留报价且不扣款；
- 调查已清、跨天和无 pending 均拒绝接受；
- 重复接受不能再次成功，价格序列 `1000 → 1200 → 1440`；
- 非正价格与整数溢出被拒绝；
- 服务、电话与结果文本在 11 种语言下非空，来电三段互不重复，价格只出现在第二段。

这些测试不直接执行 Unity 电话回调、真实 `playerCash` 写入、UI 分段推进或游戏存档文件回滚。

### 游戏内专项验收

- [ ] 调查不足 100 不来电，达到 100 的新日只创建一次报价。
- [ ] 读取调查已为 100 且当天无报价的存档会补建报价；当天已有、已接受或已拒绝报价时不重复创建。
- [ ] 自动服务与原版清除证据服务同处 `Informant`（线人）栏目。
- [ ] 晨间报告、交易、其他电话和模态 UI 不被覆盖，空闲后能展示。
- [ ] 三段正文按 `autoNextDialog` 设置自动推进或点击推进，只有最后一段出现选择，长文本无截断。
- [ ] 接受、拒绝、余额不足、未选择关闭、当天重开电话和跨天分支符合行为规格。
- [ ] 保存／重载恢复价格、每日标记和 pending，不重复弹出、不重新定价。
- [ ] 成功接受准确扣款一次、涨价一次，调查和 `state` 清零。
- [ ] `crimeAmount`、历史案值、所有治安官队列与当前访客保持不变。
- [ ] 已派遣治安官之后仍执行原版“证据消失”或其他适用分支。
- [ ] 不消耗 Wild Favor，原版手动 `EVIDENCE_REMOVAL` 价格与冷却不受影响。
- [ ] 接受过程中重复点击不会重复扣款，异常回滚按设计工作。
- [ ] 11 种语言文本回退正常，中英文电话气泡和按钮实际渲染无遮挡。
