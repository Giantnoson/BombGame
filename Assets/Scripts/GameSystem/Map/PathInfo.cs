using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace GameSystem.GameScene.MainMenu.Map
{
    /// <summary>
    ///     路径信息
    /// </summary>
    public class PathInfo
    {
        //移动相关
        /// <summary>
        ///     总步数
        /// </summary>
        public readonly int Count;

        public readonly List<Vector2Int> Path;

        /// <summary>
        ///     当前步数
        /// </summary>
        public int CurrentStep;

        //基本信息
        public Vector2Int StartPos;
        public TargetInfo TargetInfo;


        public PathInfo(Vector2Int startPos, [NotNull] TargetInfo targetInfo, [NotNull] List<Vector2Int> path)
        {
            StartPos = startPos;
            TargetInfo = targetInfo ?? throw new ArgumentNullException(nameof(targetInfo));
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Count = path.Count;
            CurrentStep = 0;
        }

        public PathInfo()
        {
        }

        public bool NowPath(out Vector2Int nowPos)
        {
            if (CurrentStep < Count)
            {
                nowPos = Path[CurrentStep];
                return true;
            }

            nowPos = Vector2Int.down;
            return false;
        }

        public Vector2Int NowPath()
        {
            if (CurrentStep < Count) return Path[CurrentStep];

            return Vector2Int.down;
        }

        public bool Next(out Vector2Int nextPos)
        {
            if (CurrentStep < Count - 1)
            {
                nextPos = Path[++CurrentStep];
                return true;
            }

            nextPos = Vector2Int.down;
            return false;
        }

        public Vector2Int Next()
        {
            if (CurrentStep < Count - 1) return Path[++CurrentStep];
            return Vector2Int.down;
        }

        public bool GetNextPaths(int step, out List<Vector2Int> path)
        {
            if (CurrentStep >= Count)
            {
                path = null;
                return false;
            }

            if (CurrentStep + step < Count)
                path = Path.GetRange(CurrentStep, step);
            else
                path = Path.GetRange(CurrentStep, Count - CurrentStep);
            return true;
        }

        public Vector2Int GetEndPos()
        {
            return Path[Count - 1];
        }

        public Vector2Int GetStartPos()
        {
            return Path[0];
        }

        public bool IsEnd()
        {
            return CurrentStep == Count - 1;
        }
    }
}