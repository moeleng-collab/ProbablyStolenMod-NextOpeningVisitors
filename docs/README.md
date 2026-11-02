# NextOpeningVisitors 文档总索引

- 当前代码版本：`1.6.0`
- 目标游戏：《Probably Stolen》`0.46D`
- 文档结构：按功能拆分当前事实，集中记录验证状态，保留历史版本快照

## 查询方式

不要顺序阅读全部文档。先按任务选择最小阅读范围：

| 任务 | 首选文档 | 需要时再读 |
| --- | --- | --- |
| 查询或修改一个功能 | 对应功能手册 | 涉及共享生命周期或构建时再读公共工程说明 |
| 核对当前自动化结果或未测项 | [当前验证记录](validation/current.md) | 对应功能手册的验收清单 |
| 修改架构、生命周期、构建或发布流程 | [公共工程说明](shared-engineering.md) | 受影响的功能手册 |
| 查询某次历史更新 | 历史版本快照 | 当前功能手册用于核对现状，不回写历史文件 |

## 功能手册

| 功能 | 当前行为与实现 | 主要源码 |
| --- | --- | --- |
| 下一次开店到访、预约去重与稳定置顶 | [访客调度](features/visitor-scheduling.md) | `VisitorRules.cs`、`NextOpeningVisitorsMod.cs` |
| 五位联系人冷却清除与预约保护 | [电话冷却清除](features/phone-cooldown.md) | `CooldownResetRules.cs`、`PhoneCooldownResetButton.cs` |
| 供货商与奥丁电话文案及语言回退 | [电话本地化](features/phone-localization.md) | `PhoneDialogueTextRules.cs`、`PhoneDialogueTextPatch.cs` |
| 当前交易案值与调查预测 | [交易预测](features/trade-forecast.md) | `CrimeForecast*`、`TradeForecastUI.cs` |
| 夜间案值、收尾人与调查预测 | [夜间预测](features/night-forecast.md) | `CrimeForecast*`、`NightForecastUI.cs` |
| 王哥调查满值自动来电与付费清除 | [自动证据清除](features/auto-evidence-cleanup.md) | `AutoEvidenceCallState.cs`、`AutoEvidenceCallController.cs` |
| 顾客需求与治安盘查物品高亮 | [需求与盘查高亮](features/demand-highlight.md) | `DemandHighlightRules.cs`、`DemandHighlightUI.cs` |
| 放大镜真实估值、真伪原因、价值拆解与荧光莓品质 | [自动鉴定](features/appraisal-assistant.md) | `AppraisalAssistantRules.cs`、`AppraisalItemDetails.cs`、`AppraisalAssistantUI.cs` |
| 改造人检测与到店身份／通缉提醒 | [改造人检测](features/aug-detection.md) | `AppraisalAssistantRules.cs`、`AppraisalAssistantUI.cs` |

每份功能手册只记录该功能需要的内容，章节可按功能裁剪，不机械保留空模板。自动化结果的全局数字统一放在当前验证记录中，功能手册只列覆盖范围和本功能游戏内验收项。

## 公共文档

- [公共工程说明](shared-engineering.md)：事实源、三层架构、主线程、生命周期、构建、禁用、发布包与兼容约束。
- [当前验证记录](validation/current.md)：当前版本的规则测试、构建、扫描、全局游戏内未测项和发布记录模板。
- [1.6.0 发布记录](validation/v1.6.0.md)：封板提交、发布文件、游戏内证据边界与 Steam 远程核验状态。

## 旧手册入口

旧文件保留为导航入口，不再维护重复全文：

- [现有功能行为规格](behavior-spec.md)
- [现有项目架构](architecture.md)
- [测试说明](testing.md)
- [技术手册](technical-manual.md)

外部已有链接仍可打开这些文件，再从入口跳到对应功能或公共文档。

## 历史版本快照

- [1.5.0 更新说明](v1.5.0-update.md)
- [1.6.0 更新说明](v1.6.0-update.md)

历史文件描述对应版本当时的需求、实现或验证状态，不是当前功能的唯一事实源。后续修复和新功能写入当前功能手册，不回写已经发布的历史快照；确需修正历史记录错误时，应明确标注更正，不改写当时已经发生的验证事实。

## 文档更新规则

1. **单功能变更**：只编辑对应功能手册；涉及共享机制时再编辑公共工程说明。
2. **验证结果变更**：断言数、扫描方法数、构建结果和全局未测状态只编辑 `validation/current.md`，不要在功能手册或根 README 复制另一份当前结果。
3. **功能清单变更**：新增、删除或改名功能时更新本索引和根 README 摘要；普通行为调整不必改索引。
4. **跨功能变更**：主线程、启停、公共本地化、构建、发布和兼容策略写入公共工程说明，功能文件只保留自身特化规则。
5. **验收记录**：游戏内验收按实际场景更新对应功能清单；加载、存档隔离和 Mod 组合等全局场景更新当前验证记录。
6. **历史记录**：版本更新说明保留当时快照，不作为当前状态的唯一来源。
7. **链接**：使用相对 Markdown 链接；移动功能内容时同步检查总索引、旧入口和根 README。

## 证据状态定义

| 状态 | 含义 | 不得推导出的结论 |
| --- | --- | --- |
| 计划／规格 | 目标行为或验收标准已经写明 | 不代表源码已经实现 |
| 源码已实现 | 当前工作区存在对应实现 | 不代表构建、扫描或游戏内行为正确 |
| 自动化通过 | 指明日期、版本和实际执行结果 | 不代表 Unity UI、电话、存档或真实结算通过 |
| 游戏内已验收 | 仅限真实执行并记录的具体场景 | 不得外推到未测语言、分辨率、存档或 Mod 组合 |
| 未测试 | 没有可核验的真实执行记录 | 不得因代码推断而写成“通过” |

计划、实现、构建、扫描、游戏内实测和用户反馈应分开记录。用户反馈可以作为问题线索或实测记录，但仍需注明版本与场景，不能替代未执行的全量回归。
