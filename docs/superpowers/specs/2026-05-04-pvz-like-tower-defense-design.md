# 植物风格 2D 横向塔防原型设计

日期：2026-05-04

## 目标

本项目第一阶段目标是在团结引擎 1.8.5 中实现一个原创的 2D 横向草坪塔防可运行原型。玩法可以借鉴经典横向格子塔防结构，包括格子放置、阳光经济、植物攻击、敌人推进、波次调度和胜负条件，但不得直接使用、复制或导入《植物大战僵尸》的原始美术、音频、商标、角色名或关卡资源。

第一阶段优先完成最小闭环，而不是追求完整内容量、复杂架构或最终美术质量。

## 技术结论

第一阶段不使用 DOTS，也不引入完整 ECS 框架。推荐使用：

- `MonoBehaviour` 负责场景对象、生命周期和编辑器连接。
- `ScriptableObject` 负责植物、敌人、投射物、关卡和波次数值配置。
- 普通 C# 类负责纯规则、状态计算和可测试逻辑。
- 轻量对象池负责敌人、投射物、阳光和特效复用。
- 简单事件总线或 C# 事件负责 UI 与玩法系统解耦。

可以吸收 ECS 的“数据与系统分离”思想，例如由 `CombatSystem`、`WaveSystem`、`SunSystem` 等模块统一管理规则，但不引入 DOTS 包和实体组件运行时。

选择理由：

- 当前目标是可运行原型，主要风险是玩法闭环和配置迭代，不是大规模单位性能。
- 团结引擎 1.8.5 下 DOTS 兼容性、调试成本和编辑器工作流风险较高。
- 传统 Unity 架构更适合快速搭建预制体、ScriptableObject 和场景验证。
- 后续若出现性能瓶颈，可以对投射物、敌人查询、碰撞判定和对象池做局部优化。

## 目录结构

初始化团结工程后，优先采用以下结构：

```text
Assets/_Project/
  Scripts/
    Core/
    Grid/
    Placement/
    Sun/
    Combat/
    Plants/
    Enemies/
    Waves/
    UI/
  Scenes/
  Prefabs/
    Plants/
    Enemies/
    Projectiles/
    Grid/
    UI/
  Art/
    Placeholders/
  Audio/
  Data/
    Plants/
    Enemies/
    Projectiles/
    Levels/
    Waves/
  UI/
Assets/ThirdParty/
```

第三方素材或插件必须放入 `Assets/ThirdParty/` 并保留许可证说明。第一阶段不主动引入第三方玩法框架。

## 核心玩法模块

### Core

`Core` 放全局入口、事件、对象池和基础服务。

建议类：

- `GameBootstrap`：连接场景中的主要系统，加载关卡配置并启动关卡。
- `GameStateController`：管理 `Preparing`、`Playing`、`Paused`、`Victory`、`Defeat` 等状态。
- `GameEvents`：提供阳光变化、卡牌冷却、波次进度、胜负结果等事件。
- `PoolManager`：轻量对象池，支持按 prefab 获取和回收对象。

### Grid

`GridSystem` 负责横向草坪格子。

职责：

- 维护行列数量、格子尺寸、世界坐标偏移。
- 提供 `GridCoordinate` 与世界坐标互转。
- 记录每个格子的占用状态。
- 为放置系统、敌人推进和攻击查询提供基础数据。

第一阶段建议默认 5 行 x 9 列，但行列数应来自 `LevelConfig`，不写死在系统内部。

### Placement

`PlantPlacementSystem` 负责植物选择和放置。

流程：

1. 玩家点击植物卡牌。
2. 系统进入待放置状态。
3. 鼠标或触控指向格子。
4. 检查格子是否为空、阳光是否足够、卡牌是否冷却完成。
5. 扣除阳光，实例化植物 prefab，绑定 `PlantConfig`，设置格子占用。
6. 启动对应卡牌冷却。

放置失败应给出轻量反馈，例如卡牌闪烁、格子变红或音效，但第一阶段不要求复杂提示系统。

### Sun

`SunSystem` 负责资源经济。

第一阶段包含两种阳光来源：

- 自然阳光：按关卡配置的间隔从屏幕上方落下。
- 生产植物：按自身配置周期生成阳光。

阳光对象被点击或触控后进入收集流程，增加阳光数值并触发 UI 更新事件。

### Plants

植物 prefab 上建议拆分为小组件：

- `Plant`：植物根组件，绑定配置和当前生命值。
- `PlantHealth`：处理受伤、死亡和格子释放。
- `PlantAttackController`：处理攻击计时和目标检测。
- `SunProducer`：仅生产型植物使用。
- `BlockerPlant`：仅高生命阻挡型植物使用，可先只作为标记或配置差异存在。

第一阶段实现三类原创植物：

- 攻击型：对同一行前方敌人发射投射物。
- 生产型：周期性生成阳光。
- 阻挡型：高生命值，主要拖延敌人。

类型名可以使用通用行为名，例如 `ShooterPlant`、`ProducerPlant`、`BlockerPlant`，具体显示名使用原创命名。

### Enemies

敌人沿固定 lane 从右向左推进，不做复杂寻路。

建议组件：

- `Enemy`：敌人根组件，绑定 `EnemyConfig`。
- `EnemyHealth`：处理伤害、死亡和奖励。
- `EnemyMovement`：控制沿 lane 推进。
- `EnemyAttackController`：遇到阻挡植物时停止移动并攻击。
- `EnemyStatusController`：处理减速、燃烧等状态效果，第一阶段可以只预留接口。

第一阶段实现两类原创敌人：

- 普通型：标准血量、速度和攻击。
- 重装型：更高血量或更慢速度。

### Combat

战斗采用 lane-based 查询。

植物攻击：

- 攻击型植物按 `AttackInterval` 计时。
- 检查同一行、植物前方是否存在敌人。
- 若存在目标，生成投射物或直接造成伤害。

投射物：

- `Projectile` 根据 `ProjectileConfig` 移动。
- 命中敌人后生成 `DamageInfo`。
- 命中后回收到对象池，除非配置为穿透或范围伤害。

伤害数据：

```csharp
public struct DamageInfo
{
    public int Amount;
    public DamageType DamageType;
    public object Source;
}
```

第一阶段只需要普通伤害。减速、范围伤害、穿透、护甲和持续伤害作为后续扩展。

### Waves

`WaveSystem` 根据 `LevelConfig` 和 `WaveConfig` 生成敌人。

职责：

- 读取关卡波次列表。
- 按时间生成敌人。
- 选择生成 lane。
- 统计当前存活敌人和剩余波次。
- 所有波次生成完且场上无敌人时触发胜利。
- 任一敌人到达左侧防线时触发失败。

第一阶段的 lane 选择支持：

- 指定 lane。
- 随机 lane。
- 从可用 lane 列表中随机。

## 配置资产

### PlantConfig

字段建议：

- `Id`
- `DisplayName`
- `Icon`
- `Prefab`
- `SunCost`
- `Cooldown`
- `MaxHealth`
- `PlantRole`
- `AttackInterval`
- `AttackRange`
- `ProjectileConfig`
- `SunProduceInterval`
- `SunProduceAmount`

未使用字段可以在对应植物类型中忽略，但配置资产应保持清晰，避免在 MonoBehaviour 中硬编码大量数值。

### EnemyConfig

字段建议：

- `Id`
- `DisplayName`
- `Prefab`
- `MaxHealth`
- `MoveSpeed`
- `AttackDamage`
- `AttackInterval`
- `RewardSun`
- `EnemyTags`

### ProjectileConfig

字段建议：

- `Id`
- `Prefab`
- `Speed`
- `Damage`
- `HitRadius`
- `CanPierce`
- `MaxPierceCount`
- `HitEffectPrefab`

第一阶段 `CanPierce` 可以默认为 false。

### LevelConfig

字段建议：

- `Id`
- `DisplayName`
- `Rows`
- `Columns`
- `InitialSun`
- `NaturalSunInterval`
- `NaturalSunAmount`
- `AvailablePlants`
- `WaveConfig`

### WaveConfig

字段建议：

- `Entries`

`WaveEntry` 字段建议：

- `EnemyConfig`
- `SpawnTime`
- `Count`
- `Interval`
- `LaneMode`
- `AllowedLanes`
- `IsMajorWave`

## UI 方案

第一阶段使用 Unity UI / uGUI，不引入第三方 UI 框架，也不混用 UI Toolkit。

主要界面：

- `GameHudView`：阳光数量、植物卡牌栏、波次进度。
- `PlantCardView`：图标、阳光消耗、冷却遮罩、可用状态。
- `PauseMenuView`：暂停、继续、重开、退出。
- `ResultView`：胜利、失败、重开。

UI 更新方式使用事件驱动：

- `SunChanged`
- `PlantCardCooldownChanged`
- `WaveProgressChanged`
- `GameStateChanged`
- `LevelEnded`

UI 不应每帧主动轮询玩法系统。

## 相机方案

第一阶段使用固定正交相机，不引入第三方相机框架。

`CameraFitController` 根据关卡行列数、格子尺寸和 UI 安全区域设置相机位置与 `orthographicSize`，保证草坪区域可见。

后续如果团结 1.8.5 对 Cinemachine 支持稳定，可以考虑引入 Cinemachine 做开场平移、轻微镜头震动或屏幕适配增强。但核心玩法不依赖 Cinemachine。

## 对象池方案

第一阶段自建轻量对象池，不引入大型池框架。

池化对象：

- 敌人。
- 投射物。
- 阳光。
- 命中特效。

接口建议：

```csharp
public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}
```

对象池按 prefab 管理实例，系统通过 `PoolManager.Spawn(prefab, position, rotation)` 和 `PoolManager.Despawn(instance)` 获取与回收对象。

## 第一阶段验收标准

第一阶段完成后，应能在编辑器中验证一个完整闭环：

1. 打开主场景并开始关卡。
2. 初始阳光显示正确。
3. 玩家可以选择植物卡牌并放置到空格子。
4. 阳光不足或卡牌冷却中时不能放置。
5. 自然阳光会生成并可收集。
6. 生产型植物会周期性生成阳光。
7. 敌人会按波次从右侧出现并沿 lane 推进。
8. 攻击型植物会攻击同一行前方敌人。
9. 投射物命中后造成伤害，敌人生命归零后死亡。
10. 敌人遇到植物会停止并攻击，植物死亡后释放格子。
11. 敌人到达左侧防线时失败。
12. 所有波次结束且场上无敌人时胜利。
13. Console 无新增脚本编译错误、缺失引用或运行时错误。

## 暂不做的内容

以下内容不进入第一阶段：

- DOTS/ECS 改造。
- 第三方战斗框架。
- 第三方 UI 框架。
- 复杂寻路。
- 大量植物和敌人内容。
- 商业级美术、音频和动效。
- 存档、选关、养成、商店和商业化系统。
- 多平台适配。

这些内容应在可运行原型稳定后，根据实际需求分阶段设计。

## 风险与应对

- 团结引擎 1.8.5 包兼容风险：第一阶段减少第三方依赖，优先使用内置能力。
- IP 风险：所有显示名、资源和音频使用原创内容，代码中只保留通用行为命名。
- 后期扩展风险：数值和 prefab 绑定通过 ScriptableObject 管理，避免硬编码。
- 性能风险：第一阶段使用对象池和 lane 查询，避免频繁全局查找和大量运行时分配。
- 场景引用风险：核心对象通过 prefab 和配置连接，移动资源时优先使用编辑器保持 `.meta` 引用稳定。

## 推荐实现顺序

1. 初始化团结工程目录与主场景。
2. 建立 `LevelConfig`、`PlantConfig`、`EnemyConfig`、`ProjectileConfig`。
3. 实现 `GridSystem` 和基础格子显示。
4. 实现阳光数值、HUD 和卡牌 UI。
5. 实现植物选择与放置。
6. 实现敌人生成和 lane 推进。
7. 实现植物攻击、投射物和伤害。
8. 实现敌人攻击植物和死亡清理。
9. 实现胜负条件。
10. 补齐对象池、占位资源和最小测试关卡。
