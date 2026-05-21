using System;

namespace GameSystem.Timer
{
    /// <summary>
    ///     本地时间辅助类，提供基于计算机本地时间的毫秒级时间戳
    ///     以 Unix 时间戳（毫秒）为基准，long 精度，便于与网络同步等场景对接
    /// </summary>
    public static class LocalTime
    {
        /// <summary>
        ///     Unix 纪元（1970-01-01 本地时间）
        /// </summary>
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);

        /// <summary>
        ///     当前本地毫秒级时间戳（long 精度）
        /// </summary>
        public static long Now => (long)(DateTime.Now - Epoch).TotalMilliseconds;
    }
}
