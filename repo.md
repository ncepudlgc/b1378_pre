# TextRPG

## Repo简介

TextRPG 是一个基于文本的角色扮演游戏（RPG），使用 C# 开发。游戏包含完整的角色系统、战斗系统、地牢探索、任务系统、季节系统等丰富的游戏机制。

### 主要功能

1. **角色系统**
   - 角色属性（生命值、魔法值、攻击力、防御力等）
   - 技能系统
   - 装备系统（武器、护甲）
   - 物品系统（药水、道具）

2. **战斗系统**
   - 回合制战斗
   - 近战和远程攻击
   - 技能和魔法系统
   - 敌人 AI

3. **地牢系统**
   - 多层地牢（3 层）
   - 随机地图生成
   - 敌人生成和分布
   - 楼梯系统

4. **任务系统**
   - 季节性任务生成
   - 任务板系统
   - 常规任务和奖励任务
   - 任务进度跟踪

5. **季节系统**
   - 四季循环（春、夏、秋、冬）
   - 季节性敌人类型
   - 季节性任务
   - 季节变化影响游戏机制

### 技术栈

- **语言**: C#
- **框架**: .NET
- **架构**: 面向对象设计

### 项目结构

```
TextRPG/
├── Character.cs          # 角色类
├── Enemy.cs              # 敌人类
├── Quest.cs              # 基础任务类
├── SeasonalQuest.cs      # 季节性任务类
├── QuestManager.cs       # 任务管理器（集中管理任务逻辑）
├── Dungeon.cs            # 地牢系统
├── TownSquare.cs         # 城镇广场（任务板、商店等）
├── GameCalendar.cs       # 游戏日历（季节系统）
├── BattleSystem.cs       # 战斗系统
├── WorldMap.cs           # 世界地图
└── Program.cs            # 主程序入口
```

### 任务系统架构

**QuestManager** - 集中管理任务逻辑：
- 生成季节性任务
- 生成奖励任务
- 管理任务分布

**任务生成规则**：
- 每个季节开始时，任务板填充每个地牢级别的两个常规任务（针对该季节）
- 同时生成一个带有奖励的季节任务，随机放置在一个地牢级别上
- 玩家通过任务板接受常规任务
- 玩家通过在地牢中踩到奖励任务来接受

**代码分离**：
- QuestManager: 任务生成逻辑
- TownSquare: 任务板 UI 和任务接受
- Dungeon: 任务放置和触发
- GameCalendar: 季节管理和任务模板

## 题目Prompt

Quest generation is not working as intended. The Quest board should be populated at the beginning of each season with two regular quests per dungeon level for that season, along with the non-dungeon quests for that season. At the same time, one seasonal quest with bonus rewards should be generated and placed in a random location on one dungeon level. The player accepts the regular quests through the quest board, and the bonus quest by stepping onto it in the dungeon. Clean up the logic separation while we're making these changes. Quest logic has crept out from the intended classes.

## PR链接

https://github.com/ncepudlgc/b1378_pre/pull/1
