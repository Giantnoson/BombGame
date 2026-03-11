using System;

namespace Core.Net
{
    /// <summary>
    /// 命令类型枚举类，用于定义各种网络通信命令类型
    /// </summary>
    public class CmdType
    {

        // 登录相关命令
        public const int Login = 0x0101;          // 登录命令
        public const int Heartbeat = 0x0102;      // 心跳命令，用于保持连接
        public const int Logout = 0x0103;         // 登出命令
        
        // 系统相关命令
        /// <summary>
        /// 系统异常命令
        /// </summary>
        public const int Exception = 0x01FF;      // 异常命令
        /// <summary>
        /// 系统警告命令
        /// </summary>
        public const int Alert = 0x01FE;          // 警告命令
        /// <summary>
        /// 系统消息命令
        /// </summary>
        public const int Info = 0x01FD;

        // 场景相关命令
        public const int EnterScene = 0x0201;      // 进入场景命令
        public const int ExitScene = 0x0202;       // 退出场景命令
        
        // 移动相关命令
        public const int Move = 0x0301;           // 移动命令



        // 基础游戏相关命令
        /// <summary>
        /// 开始匹配命令
        /// </summary>
        public const int BaseGameStartMatch = 0x0401;      // 开始匹配命令
        /// <summary>
        /// 取消匹配命令
        /// </summary>
        public const int BaseGameCancelMatch = 0x0402;     // 取消匹配命令
        /// <summary>
        /// 匹配成功命令
        /// </summary>
        public const int EnterBaseGame = 0x0403;   // 匹配成功命令
        /// <summary>
        /// 创建房间命令
        /// </summary>
        public const int BaseGameCreateRoom = 0x0404;      // 创建房间命令
        /// <summary>
        /// 加入房间命令
        /// </summary>
        public const int BaseGameJoinRoom = 0x0405;        // 加入房间命令
        /// <summary>
        /// 离开房间命令
        /// </summary>
        public const int BaseGameLeaveRoom = 0x0406;       // 离开房间命令
        /// <summary>
        /// 当前房间变化命令
        /// </summary>
        public const int BaseGameCurrentRoomChange = 0x0407; // 当前房间变化命令
        /// <summary>
        /// 请求房间命令
        /// </summary>
        public const int BaseGameReqRoomInfo = 0x0408;     // 请求房间信息命令
        /// <summary>
        /// 踢出玩家命令
        /// </summary>
        public const int BaseGameKickPlayer = 0x0409;      // 踢出玩家命令
        /// <summary>
        /// 移除房间命令
        /// </summary>
        public const int BaseGameRemoveRoom = 0x040A;      // 移除房间命令
        /// <summary>
        /// 房主变更命令
        /// </summary>
        public const int BaseGameLeaderChange = 0x040B;   // 房主变更命令
        /// <summary>
        /// 准备命令
        /// </summary>
        public const int BaseGameReady = 0x040C;          // 准备命令
        /// <summary>
        /// 更换职业命令
        /// </summary>
        public const int BaseGameChangeCareer = 0x040D;    // 更换职业命令
        /// <summary>
        /// 更改地图命令
        /// </summary>
        public const int BaseGameMapChange = 0x040E;        // 更改地图命令
        /// <summary>
        /// 玩家发送消息命令
        /// </summary>
        public const int BaseGamePlayerSendMessage = 0x040F; // 玩家发送消息
        /// <summary>
        /// 开始游戏命令
        /// </summary>
        public const int BaseGameStartGame = 0x0410;        // 开始游戏命令


        
        // 炸弹相关命令
        /// <summary>
        /// 放置炸弹命令
        /// </summary>
        public const int PutBomb = 0x0501;        // 放置炸弹命令
        /// <summary>
        /// 炸弹爆炸命令
        /// </summary>
        public const int BombExplode = 0x0502;    // 炸弹爆炸命令
        

        
        // 状态相关命令
        /// <summary>
        /// 状态变更命令
        /// </summary>
        public const int StatusChange = 0x0601;   // 状态变更命令
        

        
        // 障碍物相关命令
        /// <summary>
        /// 创建障碍物命令
        /// </summary>
        public const int ObstacleCreate = 0x0701; // 创建障碍物命令
        /// <summary>
        /// 障碍物变更命令
        /// </summary>
        public const int ObstacleChange = 0x0702; // 障碍物变更命令

        /// <summary>
        /// 根据命令类型代码获取对应的命令名称
        /// </summary>
        /// <param name="cmd">命令类型代码</param>
        /// <returns>命令名称字符串</returns>
        /// <exception cref="ArgumentOutOfRangeException">当传入未知的命令类型代码时抛出异常</exception>
        public static string TryToGetType(int cmd)
        {
            switch (cmd)
            {
                case Login:
                    return nameof(Login);
                case Heartbeat:
                    return nameof(Heartbeat);
                case Logout:
                    return nameof(Logout);
                case Exception:
                    return nameof(Exception);
                case Alert:
                    return nameof(Alert);
                case Move:
                    return nameof(Move);
                case BaseGameStartMatch:
                    return nameof(BaseGameStartMatch);
                case BaseGameCancelMatch:
                    return nameof(BaseGameCancelMatch);
                case EnterBaseGame:
                    return nameof(EnterBaseGame);
                case BaseGameCreateRoom:
                    return nameof(BaseGameCreateRoom);
                case BaseGameJoinRoom:
                    return nameof(BaseGameJoinRoom);
                case BaseGameLeaveRoom:
                    return nameof(BaseGameLeaveRoom);
                case BaseGameCurrentRoomChange:
                    return nameof(BaseGameCurrentRoomChange);
                case BaseGameReqRoomInfo:
                    return nameof(BaseGameReqRoomInfo);
                case BaseGameKickPlayer:
                    return nameof(BaseGameKickPlayer);
                case BaseGameRemoveRoom:
                    return nameof(BaseGameRemoveRoom);
                case BaseGameLeaderChange:
                    return nameof(BaseGameLeaderChange);
                case BaseGameReady:
                    return nameof(BaseGameReady);
                case BaseGameChangeCareer:
                    return nameof(BaseGameChangeCareer);
                case PutBomb:
                    return nameof(PutBomb);
                case BombExplode:
                    return nameof(BombExplode);
                case StatusChange:
                    return nameof(StatusChange);
                case ObstacleCreate:
                    return nameof(ObstacleCreate);
                case ObstacleChange:
                    return nameof(ObstacleChange);
                case BaseGamePlayerSendMessage:
                    return nameof(BaseGamePlayerSendMessage);
                case BaseGameStartGame:
                    return nameof(BaseGameStartGame);
                case BaseGameMapChange:
                    return nameof(BaseGameMapChange);
                case EnterScene:
                    return nameof(EnterScene);
                case ExitScene:
                    return nameof(ExitScene);
                default:
                    throw new ArgumentOutOfRangeException(nameof(cmd), cmd, "未知的cmd");
            }
        }
        
    }
}