# Eggs - Game Scope

## Theme

Regrowth

## One Sentence

玩家拖拽仅存的小狗去采集、繁殖和防守，在敌人不断攻击爱巢的压力下孵化新的小狗，让濒危种群重新恢复。

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

同一批有限的小狗必须在三个需求之间重新分配：

### Gathering
生产 Food。

### Breeding
暂时占用个体，消耗 Food，产生 Egg，并最终增加 Population。

### Defending
放弃当前采集 / 繁殖效率，保护 Love Nest。

核心冲突：

“现在活下来”
vs
“投资未来的种群恢复”。

如果把太多狗用于繁殖：
当前防守变弱。

如果把太多狗用于防守：
Food 与 Population 增长变慢。

---

## Core Loop

拖拽分配小狗

→ Gathering 产生 Food

→ 两只狗进入 Love Nest

→ 消耗 Food 开始繁殖

→ 产生 Egg

→ Egg 孵化新狗

→ Population 增长

→ 敌人来袭

→ 玩家把狗重新调到 Defense

→ 守住 Love Nest

→ 再继续采集与繁殖

---

## World Structure

只做一个固定单屏场景。

场景包含：

### Love Nest
- 位于场景中心或视觉核心位置
- 是繁殖区域
- Egg 出现在这里
- 后续也是必须保护的核心

### Food Zone
- 狗进入后成为 Gathering
- 持续生产 Food

### Defense Zone
- 狗进入后成为 Defending
- M1 只需要完成状态分配
- M2 才真正攻击敌人

### Enemy Entry
- M2 才启用
- 敌人从屏幕边缘进入
- 只向 Love Nest 移动

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

- 4 只普通狗
- 一定初始 Food

每只狗支持：

- 鼠标拖拽
- Idle
- Gathering
- Breeding
- Defending

M1 中 Defending 只需要正确保存工作状态，
不需要战斗。

Food Zone：

- Gathering 狗持续产生 Food
- 拖走后立即停止产生 Food

Love Nest：

- 至少两只狗才能开始繁殖
- 必须有足够 Food
- 一次繁殖只消耗一次 Food
- 参与繁殖的两只狗暂时不能承担其他工作
- 繁殖经过时间后产生一个 Egg

Egg：

- Egg 经过 Hatch Duration
- 孵化出一只新的普通狗
- Population +1
- 新生狗和初始狗使用同一套核心逻辑
- 新生狗也可继续拖拽和重新分配

Minimum HUD：

- Population
- Food

## M1 Success Flow

玩家必须能够通过真实鼠标操作亲自完成：

4 dogs
→ assign Gathering
→ gain Food
→ move 2 dogs into Love Nest
→ Breed
→ Egg appears
→ Egg hatches
→ Population becomes 5

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

- 从屏幕边缘生成
- 只向 Love Nest 移动
- 到达 Nest 后攻击 Nest
- 可以被 Defending 狗杀死

Defending Dog:

- 自动攻击进入防守范围的敌人
- 不需要玩家逐个指定敌人

## Hard Simplifications

狗没有 HP。

敌人不攻击狗。

不做：

- aggro
- threat
- dog damage
- dog combat death
- NavMesh
- pathfinding
- multiple enemy behaviours

Defense 的决策核心仍然是：

“我要分多少只狗回来保护 Love Nest？”

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
- 狗 HP
- 狗被攻击
- 狗战斗死亡
- 敌人攻击狗
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
