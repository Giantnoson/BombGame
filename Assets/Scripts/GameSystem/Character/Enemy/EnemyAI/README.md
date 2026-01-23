
# 炸弹人对战机器人FSM状态机

## 概述

这是一个基于有限状态机(FSM)的炸弹人对战机器人AI系统，用于控制敌方角色的行为。

## 架构

### 核心类

1. **EnemyAIController** - AI控制器主类
   - 管理AI的所有状态和行为
   - 处理炸弹放置和移动逻辑
   - 提供公共接口供状态使用

2. **EnemyAIBaseState** - 状态基类
   - 所有AI状态的基类
   - 提供公共方法和属性

3. **EnemyAI** - EnemyAIController的别名
   - 保持向后兼容

### 状态列表

1. **IdleState** - 闲置状态
   - 扫描周围环境
   - 检测玩家和可破坏方块
   - 检测爆炸威胁

2. **SearchState** - 搜索状态
   - 寻找可破坏方块
   - 移动到目标位置
   - 检测爆炸威胁

3. **PathWaitState** - 路径等待状态
   - 当路径上有爆炸威胁时暂停
   - 等待爆炸结束
   - 寻找替代路径

4. **ChasePlayerState** - 追击玩家状态
   - 追击并接近玩家
   - 保持安全距离
   - 检测爆炸威胁

5. **PlaceBombState** - 放置炸弹状态
   - 在合适位置放置炸弹
   - 立即进入躲避状态

6. **AvoidExplosionState** - 躲避爆炸状态
   - 躲避爆炸威胁
   - 寻找安全区域
   - 检测安全状态

7. **AttackState** - 攻击状态
   - 对玩家进行攻击
   - 放置炸弹
   - 检测爆炸威胁

## 使用方法

### 基本设置

1. 在敌人预制体上添加`EnemyAI`组件
2. 在Inspector中配置AI参数:
   - Detection Range: 检测范围
   - Chase Range: 追击范围
   - Attack Range: 攻击范围
   - Move Speed: 移动速度
   - Max Bomb Count: 最大炸弹数量
   - Bomb Cooldown: 炸弹冷却时间
   - Bomb Radius: 炸弹爆炸范围
   - Bomb Damage: 炸弹伤害
   - Bomb Fuse Time: 炸弹爆炸时间

### 状态优先级

状态切换遵循以下优先级:
1. 爆炸威胁(最高优先级)
2. 玩家检测
3. 目标完成
4. 路径阻挡
5. 其他条件

## 扩展指南

### 添加新状态

1. 创建新的状态类，继承自`EnemyAIBaseState`
2. 实现必要的方法:
   - `OnEnter` - 进入状态时调用
   - `OnUpdate` - 每帧更新时调用
   - `OnLeave` - 离开状态时调用
3. 在`EnemyAIController`的`InitializeFSM`方法中注册新状态

### 修改现有状态

1. 找到对应的状态类文件
2. 修改状态逻辑
3. 确保状态转换条件正确

## 待实现功能

以下功能已标记为TODO，需要进一步实现:

1. **爆炸范围检测** - `IsInExplosionRange`方法
2. **可破坏方块检测** - `HasDestructibleInRange`方法
3. **路径计算** - `CalculatePath`方法
4. **路径爆炸检测** - `IsPathBlockedByExplosion`方法
5. **安全位置查找** - `FindSafePosition`方法

## 注意事项

1. 确保敌人预制体有`CharacterController`组件
2. 确保玩家对象有"Player"标签
3. 确保可破坏方块有"Destructible"标签
4. 确保墙壁有"Wall"标签
5. 确保炸弹有"Bomb"标签

## 性能优化建议

1. 调整状态检查间隔以减少CPU使用
2. 使用对象池管理炸弹
3. 优化路径计算算法
4. 减少不必要的射线检测

## 版本历史

- v1.0 - 初始版本，实现基本FSM状态机
