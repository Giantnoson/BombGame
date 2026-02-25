namespace Core.Net
{
    public class CmdType
    {
        public static readonly int Login = 0x0101;
        public static readonly int Heartbeat = 0x0102;
        public static readonly int EnterGame = 0x0103;
        
        public static readonly int Exception = 0x01FF;
        public static readonly int Alert = 0x01FE;
        
        public static readonly int Move = 0x0301;

        public static readonly int BaseGameStartMatch = 0x0401;
        public static readonly int BaseGameCancelMatch = 0x0402;
        public static readonly int BaseGameMatchSuccess = 0x0403;
        public static readonly int BaseGameCreateRoom = 0x0404;
        public static readonly int BaseGameJoinRoom = 0x0405;
        public static readonly int BaseGameLeaveRoom = 0x0406;
        public static readonly int BaseGameCurrentRoomChange = 0x0407;
        public static readonly int BaseGameReqRoomInfo = 0x0408;
        public static readonly int BaseGameKickPlayer = 0x0409;
        public static readonly int BaseGameRemoveRoom = 0x040A;
        public static readonly int BaseGameLeaderChange = 0x040B;
        public static readonly int BaseGameReady = 0x040C;
        public static readonly int BaseGameChangeCareer = 0x040D;
        

        public static readonly int PutBomb = 0x0501;
        public static readonly int BombExplode = 0x0502;
        
        public static readonly int StatusChange = 0x0601;
        
        public static readonly int ObstacleCreate = 0x0701;
        public static readonly int ObstacleChange = 0x0702;
    }
}