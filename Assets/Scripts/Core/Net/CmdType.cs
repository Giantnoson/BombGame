namespace Core.Net
{
    public class CmdType
    {
        public static readonly int Login = 0x0101;
        public static readonly int Heartbeat = 0x0102;
        public static readonly int EnterGame = 0x0103;
        public static readonly int EnterScene = 0x0201;
    }
}