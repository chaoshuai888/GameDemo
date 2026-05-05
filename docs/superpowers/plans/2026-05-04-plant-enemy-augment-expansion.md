# 植物、敌人与开局强化扩展任务文档

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**目标：** 在现有 2D 横向草坪塔防原型上扩展植物和敌人种类，并加入每局开局随机三选一的全局强化系统，让同一关卡在不同开局下产生不同打法。

**架构：** 保持当前 `ScriptableObject` 配置驱动、`MonoBehaviour` 连接场景对象的结构。新增强化系统独立于植物、敌人和波次系统，由运行时修正服务统一提供数值加成，避免把随机强化逻辑散落到各个组件里。

**技术栈：** 团结引擎 1.8.5、C#、Unity UI/uGUI、ScriptableObject、现有对象池和事件系统；不引入 DOTS/ECS 或大型第三方框架。

---

## 当前基线

当前项目已经具备最小闭环：

- 植物配置：`PlantConfig`，现有 `Sprout Blaster`、`Sunbud`、`Stoneleaf` 三类基础植物。
- 敌人配置：`EnemyConfig`，现有 `Moss Walker`、`Moss Brute` 两类基础敌人。
- 波次配置：`WaveConfig`，当前原型波次共 3 段。
- 关卡配置：`LevelConfig`，当前通过 `AvailablePlants` 控制可选植物。
- 运行系统：`PlantPlacementSystem`、`PlantAttackController`、`SunProducer`、`WaveSystem`、`Projectile`、`GameHudView` 已经存在。

本次扩展应优先在这些已有边界内演进，不重写核心闭环。

## 命名和版权边界

- 可以借鉴“开局随机强化三选一”的玩法结构，但最终游戏内展示名不要使用 `LOL`、`海克斯科技` 等商标化表达。
- 内部代码建议使用通用名称：`Augment`、`AugmentSystem`、`AugmentChoiceView`。
- 游戏内中文展示名建议使用原创词，例如“源能增幅”“灵芽祝福”“战场律令”。
- 不导入任何商业游戏原始美术、音频、图标、商标或角色名。

## 推荐方案

推荐采用“数据驱动强化 + 运行时数值修正服务”的方案。

备选方案：

- **硬编码强化：** 实现最快，但每个强化都要改代码，后续维护会变重。
- **全脚本化效果：** 每个强化一个独立行为类，灵活但第一版过度设计。
- **数据驱动修正服务，推荐：** 先支持一批常见数值修正和开局奖励，够用、可测、对现有系统侵入小。

第一版强化只做“整局生效”的被动效果，不做局中二次选择、升级树、商店刷新或养成系统。

## 新增内容目标

### 植物扩展

在现有 3 个植物基础上，新增 5 个原创植物原型，使卡组从 3 个扩展到 8 个：

- `Mist Sprout`：减速射手。低伤害，命中后降低敌人移动速度。
- `Thorn Pod`：穿透射手。投射物可穿透 2 个敌人，适合处理密集波次。
- `Bloom Battery`：经济强化植物。生产阳光较慢，但单次产量更高。
- `Bark Bastion`：高级阻挡植物。比 `Stoneleaf` 更贵、更厚，承担后期防线。
- `Spore Mine`：一次性爆发植物。敌人接近时造成范围伤害，然后自身消失。

第一版可先使用自制占位图和简单 prefab，不追求最终美术质量。

### 敌人扩展

在现有 2 个敌人基础上，新增 4 个原创敌人原型：

- `Moss Skitter`：快速低血敌人，压迫前期布防。
- `Shellback Shambler`：护甲敌人，受到普通伤害降低固定值。
- `Bloom Carrier`：死亡后奖励额外阳光，鼓励主动击杀。
- `Rot Howler`：小头目敌人，进入战场时短暂提升同 lane 后续敌人速度。

第一版敌人效果优先做少量规则扩展，避免把状态系统一次做得过重。

### 开局强化扩展

每次开始关卡前，从强化池随机抽取 3 个候选，玩家选择 1 个后进入战斗。强化影响整局，第一批建议 10 个：

- `First Light`：初始阳光 +50。
- `Rapid Germination`：所有植物冷却 -15%。
- `Dense Bark`：所有植物最大生命 +20%。
- `Sharp Seeds`：所有投射物伤害 +15%。
- `Golden Drip`：自然阳光间隔 -20%。
- `Frugal Roots`：所有植物费用 -10%，最低减少 5。
- `Focused Rows`：射手攻击间隔 -10%。
- `Prepared Field`：开局随机 2 个空格生成一次性小阳光。
- `Last Stand`：左侧防线被突破前，第一次失败判定延迟 3 秒。
- `Bounty Moss`：敌人死亡奖励阳光 +5。

## 文件结构规划

### 新建脚本

- `Assets/_Project/Scripts/Augments/AugmentConfig.cs`：强化配置 ScriptableObject。
- `Assets/_Project/Scripts/Augments/AugmentEffectType.cs`：强化效果枚举。
- `Assets/_Project/Scripts/Augments/AugmentRarity.cs`：强化稀有度枚举。
- `Assets/_Project/Scripts/Augments/AugmentRuntimeState.cs`：保存本局已选择强化。
- `Assets/_Project/Scripts/Augments/AugmentModifierService.cs`：统一提供费用、冷却、生命、伤害、阳光等数值修正。
- `Assets/_Project/Scripts/Augments/AugmentSystem.cs`：负责开局抽取候选、应用选择、通知开始战斗。
- `Assets/_Project/Scripts/UI/AugmentChoiceView.cs`：三选一 UI 面板。
- `Assets/_Project/Scripts/UI/AugmentCardView.cs`：单个强化卡片 UI。
- `Assets/_Project/Scripts/Enemies/EnemyArmor.cs`：护甲型敌人可选组件。
- `Assets/_Project/Scripts/Enemies/EnemyDeathReward.cs`：死亡奖励扩展组件。
- `Assets/_Project/Scripts/Enemies/EnemyLaneAura.cs`：lane 速度光环或入场增益组件。
- `Assets/_Project/Scripts/Plants/SporeMine.cs`：一次性爆发植物行为。
- `Assets/_Project/Scripts/Combat/EnemyStatusController.cs`：减速等轻量状态效果。

### 修改脚本

- `Assets/_Project/Scripts/Core/GameBootstrap.cs`：开局先进入强化选择，选择后再初始化或启动波次。
- `Assets/_Project/Scripts/Core/GameEvents.cs`：增加强化候选、强化选择、战斗开始事件。
- `Assets/_Project/Scripts/Data/LevelConfig.cs`：增加本关可出现强化池。
- `Assets/_Project/Scripts/Data/PlantConfig.cs`：增加可选标签和特殊行为参数。
- `Assets/_Project/Scripts/Data/EnemyConfig.cs`：增加护甲、死亡奖励、标签和特殊行为参数。
- `Assets/_Project/Scripts/Data/ProjectileConfig.cs`：复用 `CanPierce`、`MaxPierceCount`，并增加减速参数。
- `Assets/_Project/Scripts/Placement/PlantCardState.cs`：冷却读取强化修正。
- `Assets/_Project/Scripts/Placement/PlantPlacementSystem.cs`：费用读取强化修正。
- `Assets/_Project/Scripts/Plants/Plant.cs` / `PlantHealth.cs`：最大生命读取强化修正。
- `Assets/_Project/Scripts/Plants/PlantAttackController.cs`：攻击间隔读取强化修正。
- `Assets/_Project/Scripts/Combat/Projectile.cs`：伤害、穿透、减速读取强化和配置。
- `Assets/_Project/Scripts/Sun/SunSystem.cs`：自然阳光间隔、初始阳光、开局奖励读取强化修正。
- `Assets/_Project/Scripts/Waves/WaveSystem.cs`：敌人死亡奖励、首次失败延迟等效果接入。

### 新建资源

- `Assets/_Project/Data/Augments/`：强化配置资产。
- `Assets/_Project/Data/Plants/Expanded/`：新增植物配置资产。
- `Assets/_Project/Data/Enemies/Expanded/`：新增敌人配置资产。
- `Assets/_Project/Data/Waves/Expanded/`：扩展波次配置。
- `Assets/_Project/Prefabs/Plants/Expanded/`：新增植物 prefab。
- `Assets/_Project/Prefabs/Enemies/Expanded/`：新增敌人 prefab。
- `Assets/_Project/Prefabs/UI/Augments/`：强化选择 UI prefab。
- `Assets/_Project/Art/Placeholders/Expanded/`：新增占位图。

## 任务拆解

### Task 1: 强化数据模型

- [ ] 创建 `Augments` 脚本目录。
- [ ] 创建 `AugmentEffectType`，覆盖初始阳光、植物费用、植物冷却、植物生命、投射物伤害、自然阳光间隔、击杀奖励、首次失败延迟等效果。
- [ ] 创建 `AugmentRarity`，第一版包含 `Common`、`Rare`、`Epic`。
- [ ] 创建 `AugmentConfig`，字段包含 `Id`、`DisplayName`、`Description`、`Icon`、`Rarity`、`EffectType`、`FlatValue`、`PercentValue`。
- [ ] 创建 10 个强化配置资产，全部使用原创展示名和占位图标。
- [ ] 验证 Inspector 中可以创建和编辑强化配置资产。

### Task 2: 强化运行时选择流程

- [ ] 创建 `AugmentRuntimeState`，记录本局已选择强化。
- [ ] 创建 `AugmentModifierService`，提供费用、冷却、生命、伤害、阳光、奖励等查询方法。
- [ ] 创建 `AugmentSystem`，从 `LevelConfig` 的强化池中随机抽取 3 个候选。
- [ ] 修改 `LevelConfig`，增加 `AugmentConfig[] AvailableAugments`。
- [ ] 修改 `GameBootstrap`，开局显示强化选择，玩家选择后再启动波次。
- [ ] 增加强化选择相关 `GameEvents`，让 UI 与玩法解耦。
- [ ] 验证每次进入 Play Mode 时能看到 3 个随机强化，选择后战斗开始。

### Task 3: 强化选择 UI

- [ ] 创建 `AugmentChoiceView`，订阅候选事件并控制面板显示。
- [ ] 创建 `AugmentCardView`，显示图标、名称、描述和稀有度。
- [ ] 点击任意卡片后调用 `AugmentSystem.SelectAugment(config)`。
- [ ] 选择后隐藏面板，恢复卡牌放置和波次推进。
- [ ] UI 使用现有 uGUI，不引入 UI Toolkit 或第三方 UI。
- [ ] 验证三张卡片在 16:9 和常见窗口尺寸下文本不溢出。

### Task 4: 接入通用数值修正

- [ ] 修改 `PlantPlacementSystem`，植物费用使用 `AugmentModifierService.GetPlantCost(config)`。
- [ ] 修改 `PlantCardState`，冷却使用 `GetPlantCooldown(config)`。
- [ ] 修改 `PlantHealth`，最大生命使用 `GetPlantMaxHealth(config)`。
- [ ] 修改 `PlantAttackController`，攻击间隔使用 `GetPlantAttackInterval(config)`。
- [ ] 修改 `Projectile`，伤害使用 `GetProjectileDamage(config)`。
- [ ] 修改 `SunSystem`，初始阳光、自然阳光间隔使用强化修正。
- [ ] 修改 `WaveSystem` 或敌人死亡流程，支持击杀奖励阳光修正。
- [ ] 验证 `First Light`、`Rapid Germination`、`Sharp Seeds`、`Golden Drip` 至少 4 个强化有可见效果。

### Task 5: 轻量状态与特殊战斗规则

- [ ] 创建 `EnemyStatusController`，支持减速状态的持续时间和倍率。
- [ ] 修改 `EnemyMovement`，移动速度读取当前状态倍率。
- [ ] 修改 `ProjectileConfig`，增加 `SlowPercent`、`SlowDuration`。
- [ ] 修改 `Projectile`，命中后可对敌人施加减速。
- [ ] 创建 `EnemyArmor`，支持固定减伤，最低伤害不低于 1。
- [ ] 创建 `SporeMine`，检测近距离敌人后造成范围伤害并移除自身。
- [ ] 验证减速、护甲、一次性爆发三个新规则互不影响基础闭环。

### Task 6: 新植物内容落地

- [ ] 为 `Mist Sprout` 创建配置、prefab 和占位图，使用减速投射物。
- [ ] 为 `Thorn Pod` 创建配置、prefab 和占位图，使用穿透投射物。
- [ ] 为 `Bloom Battery` 创建配置、prefab 和占位图，使用高产低频阳光生产。
- [ ] 为 `Bark Bastion` 创建配置、prefab 和占位图，使用高血量阻挡。
- [ ] 为 `Spore Mine` 创建配置、prefab 和占位图，使用一次性爆发组件。
- [ ] 将新增植物加入 `PrototypeLevel` 或新建扩展测试关卡的 `AvailablePlants`。
- [ ] 验证 8 个植物卡牌都能选择、放置、扣费、冷却，并表现出差异。

### Task 7: 新敌人内容落地

- [ ] 为 `Moss Skitter` 创建配置、prefab 和占位图，设置低血高移速。
- [ ] 为 `Shellback Shambler` 创建配置、prefab 和占位图，挂载护甲组件。
- [ ] 为 `Bloom Carrier` 创建配置、prefab 和占位图，挂载额外死亡奖励。
- [ ] 为 `Rot Howler` 创建配置、prefab 和占位图，挂载 lane 入场增益。
- [ ] 扩展波次配置，让每种新敌人至少出现一次。
- [ ] 验证新增敌人能生成、推进、攻击、死亡、触发胜负。

### Task 8: 扩展关卡和平衡首轮数值

- [ ] 新建 `ExpandedPrototypeLevel`，包含 8 个植物和 6 个敌人。
- [ ] 新建 `ExpandedPrototypeWave`，总时长控制在 90 秒以内。
- [ ] 设计前期、中期、小头目三个节奏段。
- [ ] 调整植物费用和冷却，避免某个植物明显无用或明显通吃。
- [ ] 调整敌人血量、速度和出场时间，确保基础策略可通关。
- [ ] 至少完成 3 次手动 Play Mode 验证，并记录问题。

### Task 9: 文档和验证清单

- [ ] 更新 `docs/manual-test-checklist.md`，加入强化选择、8 个植物、6 个敌人的检查项。
- [ ] 新增 `docs/content-balance-notes.md`，记录每个植物、敌人、强化的定位和第一版数值。
- [ ] 检查 Console，确保没有新增脚本编译错误、缺失引用或运行时异常。
- [ ] 检查资源命名，确保没有商业游戏角色名、商标名或来源不明素材。
- [ ] 提交前确认 `.meta` 文件随资源一起保留。

## 验收标准

- 开始关卡前出现 3 个随机强化选择。
- 玩家选择 1 个强化后，关卡正常开始。
- 至少 10 个强化配置存在，其中至少 6 个能在一局内明显观察到效果。
- 可用植物达到 8 个，且每个植物有不同定位。
- 敌人种类达到 6 个，且每个敌人至少在扩展测试波次中出现一次。
- 扩展测试关卡能完成最小闭环：选择强化、放置植物、收集阳光、抵御波次、触发胜负。
- Console 没有新增编译错误、缺失引用或运行时异常。
- 所有展示名、美术、图标和文档描述均为原创或通用表达，不使用受保护商业游戏资源。

## 风险和处理

- **强化接入过深：** 第一版只做数值修正和少量开局奖励，复杂触发器留到下一阶段。
- **内容过多导致调参失控：** 新增内容先进入扩展测试关卡，不直接替换主原型关卡。
- **UI 阻塞战斗启动：** `GameBootstrap` 必须明确区分“等待强化选择”和“战斗运行”两个阶段。
- **状态系统扩大范围：** 第一版 `EnemyStatusController` 只支持减速，不提前做中毒、燃烧、眩晕等全套系统。
- **资源引用丢失：** prefab、asset 和 `.meta` 移动尽量通过编辑器完成。

## 建议实施顺序

1. 强化数据模型和配置资产。
2. 强化选择流程和 UI。
3. 强化数值修正接入。
4. 减速、护甲、一次性爆发等少量新规则。
5. 新植物 prefab 和配置。
6. 新敌人 prefab 和配置。
7. 扩展波次和关卡平衡。
8. 手动验证、文档更新和提交。
