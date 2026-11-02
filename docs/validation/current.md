# 当前验证记录

- 代码版本：`1.6.0`
- 目标游戏：《Probably Stolen》`0.46D`
- 自动化执行日期：2026-11-02
- 源码状态：当前工作区候选代码
- 真实游戏内验收：仅由用户确认库存香烟鉴定窗口可见；其余场景未完整验收

本文是当前自动化数字和全局验证状态的唯一记录。功能级覆盖范围与游戏内清单见[总索引](../README.md#功能手册)，不要在本文件复制各功能全文。

## 1. 证据分层

| 层级 | 目的 | 能否证明游戏内完整正确 |
| --- | --- | --- |
| 纯规则测试 | 验证调度、冷却、文案、预测公式、报价状态、高亮、鉴定与本地化 | 否 |
| 编译 | 验证源码与当前游戏引用可构建 | 否 |
| 官方扫描 | 检查程序集能力与加载器拒绝项 | 否 |
| 加载检查 | 验证发现、启用、初始化和日志 | 部分 |
| 游戏内验收 | 验证真实 UI、时序、结算、存档与交互 | 是，限实际执行场景 |

任何发布都不能只依赖编译或扫描结果。此前版本的结果不作为当前版本证据。

## 2. 当前自动化结果

在游戏根目录执行：

```bash
python Modding/NextOpeningVisitors/run_tests.py
python Modding/NextOpeningVisitors/build.py
python Modding/NextOpeningVisitors/run_scan.py
```

最终执行结果：

```text
PASS: 1034 assertions; visitor, forecast, automatic offer, highlighting, appraisal and localization rules verified.
Error: <none>
Refused: False
MethodsScanned: 262
Capabilities: <none>
HardFlags: 0
Findings: 0
真实游戏内验收：鉴定窗口仅确认库存香烟可见；动态配件高亮待复验
本地安装：已完成，构建与安装 DLL 及项目与安装清单均逐字节一致
```

最终复验未出现编译警告，DLL 构建、本地安装和官方扫描成功。构建 DLL 与本地安装 DLL 已逐字节核对一致，本地清单也与项目清单一致。`run_scan.py` 调用游戏自己的 `ModScanner.Scan()` 扫描最新构建 DLL；上述结果只代表该次程序集未被扫描器拒绝，不证明算法、UI、电话、存档或兼容性正确。

鉴定详情的旧 `TreeNodeRender` 版本已在日志中确认文本生成并激活，但用户实测画面不可见；随后自行创建 `Image` 与 `TextMeshProUGUI` 的版本只显示黑框，没有文字。按 `AppraisalAssistant 1.0.7` 的界面路径改用游戏原生 `CustomUIManager.Window(..., "overlay")`、`CustomUIBuilder.AddLabel()` 和语言字体后，用户已确认库存香烟的鉴定窗口可见。该结果只覆盖香烟窗口基本显示，其他物品、价值和交互仍待验收。

武器定制高亮已补充 `GUNSMITH` 订单匹配、固定口径检查和机器输出口中可见自有物品的补扫；定制与改造订单还会按必选及奖励属性要求筛选相关配件，覆盖百分比／固定值方向、尺寸和消音器等级。最新候选会用当前订单武器动态跳过已达成条件，同属性存在未完成更高奖励档位时继续显示。当前通过 1034 条纯规则断言及官方扫描并已安装；实际动态撤框待复验。

## 3. 功能级验证入口

| 功能 | 自动化覆盖与游戏内验收 |
| --- | --- |
| 访客调度 | [visitor-scheduling](../features/visitor-scheduling.md#验证与验收) |
| 电话冷却清除 | [phone-cooldown](../features/phone-cooldown.md#验证与验收) |
| 电话本地化 | [phone-localization](../features/phone-localization.md#验证与验收) |
| 交易预测 | [trade-forecast](../features/trade-forecast.md#验证与验收) |
| 夜间预测 | [night-forecast](../features/night-forecast.md#验证与验收) |
| 自动证据清除 | [auto-evidence-cleanup](../features/auto-evidence-cleanup.md#验证与验收) |
| 需求与盘查高亮 | [demand-highlight](../features/demand-highlight.md#验证与验收) |
| 自动鉴定 | [appraisal-assistant](../features/appraisal-assistant.md#验证与验收) |
| 改造人检测 | [aug-detection](../features/aug-detection.md#验证与验收) |

所有功能手册中的游戏内复选项目前均未执行；勾选时必须基于真实运行记录，并注明必要的版本或场景差异。

## 4. 全局游戏内验收

以下项目均为**未测试**，与各功能手册中的专项清单互补。

### 4.1 加载与生命周期

- [ ] 本地 Mod 被发现，ID 为 `nextopeningvisitors`。
- [ ] 启用并按要求重启后出现 `Initialized: Next Opening Visitors v1.6.0`。
- [ ] 没有缺失方法、字段、Harmony 目标或程序集异常。
- [ ] 本地与 Workshop 没有同时加载两份副本。
- [ ] 禁用后不再生成预测、高亮、放大镜提示或自动电话，重启用无残留异常。
- [ ] 重复启用、禁用和加载不会重复创建服务、宿主或 UI。

### 4.2 存档与历史读档

- [ ] 修改预约、冷却和自动服务后保存、退出、重载，状态符合对应存档。
- [ ] 新档、旧档、两个独立存档和历史读档互不污染。
- [ ] 禁用 Mod 不重写或破坏现有存档。
- [ ] 写档失败或异常恢复不会静默改动现金、调查或报价状态。

### 4.3 兼容与渲染

- [ ] 与常用 UI、电话、访客和罪案 Mod 组合检查补丁顺序与预测失配。
- [ ] 测试高亮和鉴定前停用 `ClientDemandHighlight` 与 `AppraisalAssistant` 并重启，确认无双份边框、窗口或输入覆盖。
- [ ] 11 种语言回退正常，常用分辨率和 UI 缩放下字体、换行、按钮及长文本无裁切或遮挡。
- [ ] 游戏版本变化后重新核对全部补丁目标、字段、常量与公式。

## 5. 构建与发布产物检查

发布前应实际核对：

1. 编译器返回成功，且构建时间戳更新；
2. `build/NextOpeningVisitors.dll` 与 `Mods/NextOpeningVisitors/NextOpeningVisitors.dll` 一致；
3. 本地安装目录的 `manifest.xml` 与项目清单一致；
4. 清单、程序集和初始化日志版本一致；
5. `release/NextOpeningVisitors/` 仅包含最新清单和 DLL；
6. 发布目录没有 PDB、游戏程序集、反编译材料、测试、日志或本地设置。

## 6. 发布前记录模板

```text
版本：1.6.0
游戏版本：0.46D
构建提交／源码状态：当前工作区候选代码
规则测试日期与结果：<按第 2 节实际执行结果填写>
官方扫描日期与结果：<按第 2 节实际执行结果填写>
游戏内已测场景：用户确认库存香烟鉴定窗口可见
未测场景：动态配件撤框、其他鉴定物品、各功能专项清单及本文件全局清单
已知问题：当前候选代码仅有上述局部真实运行证据，不能视为完整游戏内验收
兼容 Mod 组合：未测试；测试前须停用两个同功能第三方 Mod
发布目录文件清单：manifest.xml、NextOpeningVisitors.dll
Workshop：1.6.0 尚未上传；既有项目 3829299781 保持 1.5.0 发布版
```

未执行的检查必须写“未测试”，不能根据代码推断写成“通过”。发布新版本时复制本模板到对应发布记录，再按实际证据更新当前文件。
