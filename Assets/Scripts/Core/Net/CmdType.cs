namespace Core.Net
{
    public class CmdType
    {
        public static readonly int Login = 0x0101;
        public static readonly int Heartbeat = 0x0102;
        public static readonly int EnterGame = 0x0103;
        public static readonly int Exception = 0x01FF;
        
        public static readonly int Move = 0x0301;

        public static readonly int BaseGameStartMatch = 0x0401;
        public static readonly int BaseGameCancelMatch = 0x0402;
        public static readonly int BaseGameMatchSuccess = 0x0403;

        public static readonly int PutBomb = 0x0501;
        public static readonly int BombExplode = 0x0502;
        
        public static readonly int StatusChange = 0x0601;
        
        public static readonly int ObstacleCreate = 0x0701;
        public static readonly int ObstacleChange = 0x0702;
    }
}