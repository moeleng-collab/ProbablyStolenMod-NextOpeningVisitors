# 访客调度：下一次开店、去重与稳定置顶

- 当前状态：源码已实现；游戏内专项验收未执行
- 目标游戏：《Probably Stolen》`0.46D`

## 行为规格

### 支持的联系人与 ID

| 联系人 | 普通预约 ID | 免费／首次预约 ID |
| --- | --- | --- |
| Red Imp 供货商 | `courrierRedImp` | `courrierRedImpFree` |
| Chem 供货商 | `courrierChem` | `courrierChemFree` |
| Mining 供货商 | `courrierMining` | `courrierMiningFree` |
| Pharma 供货商 | `courrierPharma` | `courrierPharmaFree` |
| 奥丁 | `wanted4Normal` | 无 |

`wanted4` 是剧情变体，不属于奥丁电话订单，不能被加速、去重或置顶。字符串匹配使用精确、区分大小写的 ID 比较。

### 队列处理

游戏执行 `PlayerStore.HandleFutureClientQueue()` 时，Mod 在原版方法之前：

1. 按当前队列顺序扫描目标预约；
2. 普通与免费预约映射到同一联系人键，保留第一次出现的项目；
3. 同步删除后续重复项及其相同索引的延迟；
4. 将目标预约延迟统一为 `NextOpeningDelay = 1`。

原版随后继续处理预约队列。Mod 不直接创建新预约，非目标预约保持不变。两个平行队列长度不一致时拒绝猜测对应关系，由外层记录警告并保留原版流程。

原版方法结束后，若 `storeClientManager.clientStack` 可用，目标访客执行稳定置顶：目标之间及非目标之间均保持原相对顺序，不复制访客对象。

## 实现位置

| 文件 | 职责 |
| --- | --- |
| `src/VisitorRules.cs` | 目标 ID、预约键、去重、延迟归一化与稳定置顶 |
| `src/NextOpeningVisitorsMod.cs` | `HandleFutureClientQueue` Prefix／Postfix、警告与原版回退 |

- Prefix：去重并归一化延迟；
- Postfix：在原版处理完成后稳定置顶开店访客栈。

异常不得阻断原版调度。预约与访客修改使用原版字段，不新增独立存档状态；卸载 Mod 不会自动撤销已经写入的队列。

## 验证与验收

当前全局自动化结果见[验证记录](../validation/current.md)。

### 纯规则覆盖

- 九种目标 ID 可识别，`wanted4`、空 ID 和非目标 ID 不受影响；
- 延迟归一化幂等，多目标稳定置顶，空队列安全；
- 平行队列长度不一致时拒绝处理；
- 普通／免费／奥丁重复预约保留最早项，延迟队列保持对齐。

这些断言不证明真实游戏队列时序或 UI 到访顺序已经验收。

### 游戏内专项验收

- [ ] 四种供货商和奥丁在开店前、营业中、关店后联系均于下一次开店到访。
- [ ] 普通与免费预约同时存在时只到访一次。
- [ ] 多目标同时预约时稳定置顶，非目标相对顺序不变。
- [ ] `wanted4` 与普通访客未被误改。
- [ ] 保存、退出和重载后预约不重复，实际到访时机符合预期。
