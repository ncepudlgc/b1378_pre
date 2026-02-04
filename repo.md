# b1378_pre

## Repo简介

DnDnC (Dungeons and Dragons like game) 是一个 C# 实现的地牢爬行游戏，旨在模仿龙与地下城的游戏机制。

**主要功能：**
- 终端可导航菜单
- 可升级角色系统
- 随机生成的地牢
- 角色命名功能
- 游戏状态保存
- 季节性任务系统
- 任务板系统
- 地牢探索和战斗
- 商店系统（药水商店、铁匠铺）
- 图书馆系统
- 技能管理系统

**技术栈：**
- C# / .NET
- 控制台应用程序
- 面向对象设计

**项目结构：**
- `TownSquare.cs` - 城镇广场，包含商店、任务板、图书馆等
- `Dungeon.cs` - 地牢系统，管理多层地牢、敌人生成、任务放置
- `QuestManager.cs` - 任务生成逻辑，集中管理任务创建和分发
- `Character.cs` - 角色类，管理玩家属性、装备、技能、任务日志
- `SeasonalQuest.cs` - 季节性任务类
- `Enemy.cs` - 敌人类
- `BattleSystem.cs` - 战斗系统
- `GameCalendar.cs` - 游戏日历，管理季节变化
- `Display.cs` - 显示输出管理
- `Item.cs`, `Weapon.cs`, `Armor.cs` - 物品系统
- `Skill.cs` - 技能系统

**核心组件：**
- QuestManager - 集中管理任务生成逻辑，包括常规任务和奖励任务
- TownSquare - 城镇交互，任务板刷新，商店系统
- Dungeon - 地牢生成、敌人放置、任务标记、战斗处理
- Character - 玩家角色管理，属性、装备、技能、任务日志
- GameCalendar - 季节管理和变化事件

**游戏机制：**
- 季节性敌人类型和倍数
- 多层地牢系统（3 层）
- 任务类型：常规任务（通过任务板接受）和奖励任务（在地牢中发现）
- 战斗系统：回合制战斗
- 装备系统：武器和护甲
- 技能系统：可升级技能

## 题目Prompt

Quest generation is not working as intended. The Quest board should be populated at the beginning of each season with two regular quests per dungeon level for that season, along with the non-dungeon quests for that season. At the same time, one seasonal quest with bonus rewards should be generated and placed in a random location on one dungeon level. The player accepts the regular quests through the quest board, and the bonus quest by stepping onto it in the dungeon. Clean up the logic separation while we're making these changes. Quest logic has crept out from the intended classes.

## PR链接

https://github.com/ncepudlgc/b1378_pre/pull/3
