# Eggs - Game Scope

## Theme

Regrowth

## One Sentence

玩家围绕爱巢分配仅存的小角色去采集、繁殖，并留下 Idle 个体自动防守，在敌人攻击爱巢的压力下孵化新的小角色，让濒危种群重新恢复。

## Theme Expression

Regrowth 必须直接发生在核心玩法中。

玩家开局只有很少的个体。

通过采集资源与繁殖：

- Egg 出现
- Egg 孵化
- Population 增长

增长后的种群又必须承受新的生存压力。

游戏的核心不是单纯让数字越来越大，而是：

恢复种群
→ 承受威胁
→ 重新分配有限个体
→ 继续恢复

玩家应该能在画面上直接看到：
一个接近灭绝的种群逐渐重新增长。

---

## Core Player Decision

有限 Population 必须在 Gathering、Breeding 与剩余 Idle 防守能力之间权衡：

### Gathering
生产 Food。

### Breeding
暂时占用个体，消耗 Food，产生 Egg，并最终增加 Population。

### Remaining Idle Defense Capacity
Idle 小角色是 M2 自动防守人口；不需要分配到 Defense Zone 或手动切换防守工作。
Gathering 与 Breeding 小角色不参与自动防守。

核心冲突：

“现在活下来”
vs
“投资未来的种群恢复”。

如果把太多小角色用于繁殖：
当前防守变弱。

如果保留太多 Idle 小角色用于防守：
Food 与 Population 增长变慢。

---

## Core Loop

拖拽小角色到当前有资源的 Food Rings

→ Gathering 产生 Food

→ 将两个小角色带回中心 Love Nest / Nest Core

→ 消耗 Food 开始繁殖

→ 产生 Egg

→ Egg 孵化新生小角色

→ Population 增长

→ M2 敌人从四面八方接近

→ 剩余 Idle 小角色自动防守

→ 守住 Love Nest

→ 再继续采集与繁殖

---

## World Structure

只做一个固定单屏场景。

场景包含：

### 中心：Love Nest / Nest Core
- 位于世界中心附近
- 圆盘区域，半径默认 0–1.1 world units
- WorkZone TargetState = Breeding
- 是繁殖区域
- Egg 出现在这里
- 后续也是必须保护的核心

### 第一圈：Guard / Standby Ring
- 默认半径 1.1–2.5
- 真实 WorkZone，TargetState = Idle；有成员归属，但不是职业或防守工作状态
- 初始小角色、繁殖结束的小角色和新生小角色均放在此圈
- M1B 不攻击敌人；M2 才让 Idle 小角色自动攻击

### 外圈：Food Rings
- Food Ring A 默认半径 2.5–3.7；Food Ring B 默认半径 3.7–4.9
- 两者 TargetState = Gathering，产粮按当前有效、有资源区域的成员总数计算
- 半径可在 WorkZone Inspector / Builder 中调整；必须保持相邻边界一致且区域不重叠
- 共用边界只归属外侧区域，最外侧边界归属 Food Ring B，避免重叠或缝隙

### 未来外围：Enemy Approach Area
- M2 才启用，从 Nest 周围 360° 的外围圆或屏幕边缘附近进入
- 直线向 Love Nest 中心移动，不使用 NavMesh / pathfinding

拖到所有有效区域之外：CurrentZone = null、CurrentState = Idle。
这类 Idle 小角色在 M2 同样具备自动防守资格。

### Food Ring 资源刷新方向

后续部分 Food Ring 会出现或失去可采集资源，玩家需要按当前有资源的圆环重新分配。
每个 Food Ring 可独立标记 resource available / unavailable，不改变 Unit 唯一区域归属或 GameState 的 Food 所有权。
当前 M1B 中 A/B 均先设为有资源；unavailable 只停止该圈产粮，不自动移动成员或改写工作状态。
本轮不实现随机刷新、耗尽、容量、重生计时或拾取动画。

不做地图探索。

不做多关卡。

不做多场景。

---

# Milestone 1 - Regrowth Prototype

## Goal

证明：

“有限个体重新分配 → 资源 → 繁殖 → Egg → 新生个体”

本身能够形成清晰可玩的 Regrowth。

## Required Features

初始：

- 4 个小角色，初始均匀分布在 Guard Ring，Idle 且归属 Guard Ring
- 一定初始 Food

每个小角色支持：

- 鼠标拖拽
- Idle
- Gathering
- Breeding

当前正式玩法不使用 Defending。`Defending = 3` 仅为已验证的历史 M1A
Scene / Builder 保留；M1A 原三矩形布局不代表正式 M1B/M2 空间设计。

Food Rings：

- Gathering 小角色持续产生 Food
- 拖走后立即停止产生 Food
- 每次 tick 合计所有有效、有资源 Food Ring 的当前成员数，重复区域引用不得重复计数

Love Nest：

- 至少两个小角色才能开始繁殖
- 必须有足够 Food
- 一次繁殖只消耗一次 Food
- 参与繁殖的两个小角色暂时不能承担其他工作
- 繁殖经过时间后产生一个 Egg
- M1B 默认消耗 5 Food、持续 5 秒；一次仅一个繁殖周期或未孵化 Egg
- 完成后两位参与者解锁、退出 Nest 并瞬移回 Guard Ring，状态 Idle
- 必须再次拖入 Nest 才能参与下一轮

Egg：

- Egg 经过 Hatch Duration
- 孵化出一个新的小角色
- Population +1
- 新生小角色和初始小角色使用同一套核心逻辑
- 新生小角色也可继续拖拽和重新分配
- M1B 默认孵化 4 秒，新生小角色出现在 Guard Ring，Idle、未锁定且归属 Guard Ring

Minimum HUD：

- Population
- Food

## M1 Success Flow

玩家必须能够通过真实鼠标操作亲自完成：

4 units
→ assign Gathering
→ gain Food
→ move 2 units into Love Nest
→ Breed
→ Egg appears
→ Egg hatches
→ Population becomes 5
→ newborn Unit starts Idle in Guard Ring and can be reassigned

## M1 Gate

在上述流程真实 Unity Play 验证通过之前：

禁止实现：

- Enemy
- Wave
- Nest HP
- Attack
- Game Over
- Victory

---

# Milestone 2 - Nest Defense

只有 M1 通过后开发。

## Added Features

Love Nest:

- Nest HP
- HP <= 0 → Game Over

Enemy:

- 从 Nest 周围 360° 任意方向的外围圆 / 屏幕边缘附近生成
- 直线向 Love Nest 中心移动
- 到达 Nest 后攻击 Nest
- 不攻击小角色；可以被 Idle 小角色击败

Automatic Defense:

- 不存在 Defense Zone，也不让玩家手动指定 Defending 工作
- 所有 CurrentState == Idle 的小角色自动参与防守，包括 Guard Ring 成员和区域外 Idle
- 自动攻击进入攻击范围的最近 Enemy
- Gathering 与 Breeding 小角色不攻击
- 不需要玩家逐个指定敌人

以上为已确认的 M2 设计。本轮仅更新文档，不实现 Enemy、攻击或战斗。

## Hard Simplifications

小角色没有 HP。

敌人不攻击小角色。

不做：

- aggro
- threat
- unit damage
- unit combat death
- NavMesh
- pathfinding
- multiple enemy behaviours

Defense 的决策核心仍然是：

“我要保留多少 Idle 小角色保护 Love Nest，又投入多少小角色采集和繁殖？”

---

# Milestone 3 - Complete Demo

只有 M1 与 M2 稳定后开发。

加入：

- 3 个简单敌人波次
- 来袭预警
- Population 目标
- Win
- Lose
- 正式角色美术
- Love Nest / Egg / Enemy 美术
- 基础动画
- 必要 UI
- 必要音频
- 数值平衡
- Windows Build

目标单局长度：

约 2–4 分钟。

无需为了增加时长增加系统。

---

# Lose Condition

Love Nest HP <= 0

→ Game Over

---

# Win Condition

玩家：

1. 撑过最终波次；
2. Love Nest 仍存活；
3. Population 达到最终目标数量。

具体目标数属于可调参数。

---

# Explicitly Out of Scope

除非用户明确修改 GAME_SCOPE.md，否则禁止实现：

- 职业系统
- 训练系统
- 采集者职业
- 守卫职业升级
- 照料者
- 小角色 HP
- 小角色被攻击
- 小角色战斗死亡
- 敌人攻击小角色
- 多种敌人
- Boss
- NavMesh
- 寻路
- 塔建造
- 防御塔
- 技能树
- 武器
- 装备
- 商店
- 科技树
- 遗传
- 基因
- 性别
- 多种蛋
- 幼崽成长阶段
- 疾病
- 多资源经济
- 天气系统
- 随机地图
- Roguelike
- 永久成长
- 多场景
- 存档
- 长剧情
- 复杂教程系统
