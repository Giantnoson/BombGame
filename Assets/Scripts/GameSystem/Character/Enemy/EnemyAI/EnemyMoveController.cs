using GameSystem.EventSystem;
using GameSystem.Map;
using JetBrains.Annotations;
using UnityEngine;

namespace GameSystem.Character.Enemy
{
    public class EnemyMoveController : MonoBehaviour
    {
        /// <summary>
        ///     依附对象
        /// </summary>
        public EnemyAIController owner;

        /// <summary>
        ///     移动状态
        /// </summary>
        [Tooltip("移动状态")] public bool isMoving;

        /// <summary>
        ///     是否存在目标
        /// </summary>
        [Tooltip("是否存在目标")] public bool hasTarget;

        /// <summary>
        ///     目标对象
        /// </summary>
        [SerializeField] [Tooltip("下一个目标对象")] public Vector3 nextTargetPos;

        /// <summary>
        ///     终点
        /// </summary>
        [SerializeField] [Tooltip("终点")] public Vector3 endPos;

        [Tooltip("预检查步数")] public int checkStep = 3;

        [Tooltip("是否安全")] public bool isSafe;

        public Vector3 moveDirection;
        public Vector3 beforeMoveDirection = Vector3.zero;

        public float stopRange = 0.1f;

        public float reScanTime = 0.1f;
        public float reScanTimer;

        private CharacterController _characterController;

        /// <summary>
        ///     当前路径
        /// </summary>
        [NotNull] private PathInfo _path = new();

        private bool isEnterSafeMode = true;

        private void Update()
        {
            if (!hasTarget || owner.isDead) //当不存在目标时，则返回
                return;

            if (reScanTimer > 0) //当重扫计时器大于0时，则减少计时器
            {
                reScanTimer -= Time.deltaTime;
                reScanTimer = Mathf.Clamp(reScanTimer, 0, reScanTime);
            }

            if (reScanTimer == 0)
            {
                Moving();
                reScanTimer = reScanTime;
            }


            if (isMoving) //判断当前是否移动
            {
                if (IsArriveNextPos()) //判断是否到达下一个位置
                {
                    if (GetNextPos()) //到达下一个位置后，判断是否有后续位置
                    {
                        //有后续位置时，标记存在目标，并标记停止移动
                        hasTarget = true;
                        isMoving = true;
                        Moving();
                        Debug.Log("已经到达下一个地点");
                    }
                    else
                    {
                        hasTarget = false;
                        isMoving = false;
                        StopMoving();
                    }
                }
                else //当未达到下一个位置时
                {
                    //判断是否安全，如果安全则继续移动，否则停止移动
                    if (!CheckPathIsSafe())
                    {
                        isSafe = false;
                        isMoving = false;
                        StopMoving();
                    }
                }
            }
            else //当停止移动时，判断是否需要移动
            {
                if (CheckPathIsSafe())
                {
                    isSafe = true;
                    isMoving = true;
                    Moving();
                }
            }
        }

        /// <summary>
        ///     确认路径是否安全
        /// </summary>
        /// <returns></returns>
        public bool CheckPathIsSafe()
        {
            if (!isEnterSafeMode) return true;
            if (_path.GetNextPaths(checkStep, out var list))
                foreach (var pos in list)
                    if (!owner.MapInfo.IsWalkable(pos)
                        && owner.bombPos.BombInfo.ContainsKey(
                            owner.ToBombPutPos(owner.MapInfo.GetRealCoord(pos))))
                    {
                        print("路径上存在爆炸威胁");
                        return false;
                    }

            return true;
        }

        /// <summary>
        ///     初始化
        /// </summary>
        /// <param name="owner">依附对象</param>
        /// <param name="controller">控制器</param>
        public void Init(EnemyAIController owner, CharacterController controller)
        {
            _characterController = controller;
            this.owner = owner;
        }

        /// <summary>
        ///     获取下一个位置
        /// </summary>
        /// <returns>返回是否有下一个位置</returns>
        private bool GetNextPos()
        {
            if (_path.Next(out var nextPos))
            {
                nextTargetPos = owner.MapInfo.GetRealCoord(nextPos);
                Debug.Log("Next Path = " + nextPos);
                return true;
            }

            return false;
        }


        /// <summary>
        ///     判断是否到达下一个位置
        /// </summary>
        /// <returns>返回判断结果</returns>
        public bool IsArriveNextPos()
        {
            if (Vector3.Distance(owner.ToSearchPos(owner.transform.position), nextTargetPos) < stopRange)
            {
                isMoving = false;
                hasTarget = false;
                return true;
            }

            return false;
        }

        public bool IsArriveEndPos()
        {
            if (Vector3.Distance(owner.ToSearchPos(owner.transform.position), endPos) < 0.01f) return true;
            return false;
        }

        /// <summary>
        ///     移动到移动目标
        /// </summary>
        /// <param name="path">目标路径</param>
        public bool MoveToTarget(PathInfo path, bool isEnterSafeMode)
        {
            this.isEnterSafeMode = isEnterSafeMode;
            StopMoving();
            if (path == null)
            {
                Debug.Log("path为空");
                return false;
            }

            if (!path.NowPath(out var nextPos))
            {
                Debug.LogError("path.NowPath为空");
                return false;
            }

            print($"此次起点为：{path.NowPath()}，终点为：{path.GetEndPos()}途径点如下：");
            foreach (var vector2Int in path.Path) print("path = " + vector2Int);
            nextTargetPos = owner.MapInfo.GetRealCoord(nextPos);
            _path = path;
            hasTarget = true;
            if (CheckPathIsSafe())
            {
                isMoving = true;
                isSafe = true;
                Moving();
            }
            else
            {
                isMoving = true;
                isSafe = true;
                StopMoving();
            }

            return true;
        }

        /// <summary>
        ///     移动到下一个位置
        /// </summary>
        public void Moving()
        {
            // 将移动向量从角色本地坐标系转换到世界坐标系
            moveDirection =
                transform.TransformDirection((nextTargetPos - owner.ToSearchPos(owner.transform.position)).normalized);
            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateMoveDirectionEvent(owner.id, moveDirection));
        }

        /// <summary>
        ///     停止移动
        /// </summary>
        public void StopMoving()
        {
            moveDirection = Vector3.zero;
            GameEventSystem.Broadcast(new CharacterMoveEvent.UpdateMoveDirectionEvent(owner.id, moveDirection));
        }
    }
}