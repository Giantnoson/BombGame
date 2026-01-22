using System;
using System.Collections.Generic;
using config;
using GameSystem.Map;
using JetBrains.Annotations;
using UnityEngine;

namespace GameSystem.Enemy
{
    public class EnemyMoveController : MonoBehaviour
    {
        
        /// <summary>
        /// 依附对象
        /// </summary>
        public EnemyAIController owner;
        
        /// <summary>
        /// 移动状态
        /// </summary>
        [Tooltip("移动状态")]
        public bool isMoving = false;
        /// <summary>
        /// 是否存在目标
        /// </summary>
        [Tooltip("是否存在目标")]
        public bool hasTarget = false;

        /// <summary>
        /// 目标对象
        /// </summary>
        [SerializeField]
        [Tooltip("下一个目标对象")]
        public Vector3 nextTargetPos;

        /// <summary>
        /// 终点
        /// </summary>
        [SerializeField]
        [Tooltip("终点")]
        public Vector3 endPos;

        [Tooltip("预检查步数")]
        public int checkStep = 3;
        
        [Tooltip("是否安全")]
        public bool isSafe = false;
        
        
        /// <summary>
        /// 当前路径
        /// </summary>
        [NotNull]
        private PathInfo _path = new PathInfo();

        private CharacterController _characterController;
        /*private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("EnemyMoveController:在这个对象当中不存在characterController");
            }
        }*/
        
        private void Update()
        {
            if (!hasTarget)//当不存在目标时，则返回
            {
                return;
            }
            if (isMoving)//判断当前是否移动
            {
                if (IsArriveNextPos())//判断是否到达下一个位置
                {
                    if (GetNextPos())//到达下一个位置后，判断是否有后续位置
                    {//有后续位置时，标记存在目标，并标记停止移动
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
                else//当未达到下一个位置时
                {//判断是否安全，如果安全则继续移动，否则停止移动
                    if (!CheckPathIsSafe())
                    {
                        isSafe = false;
                        isMoving = false;
                        StopMoving();
                    }
                }
            }
            else//当停止移动时，判断是否需要移动
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
        /// 确认路径是否安全
        /// </summary>
        /// <returns></returns>
        public bool CheckPathIsSafe()
        {
            if (_path.GetNextPaths(checkStep, out List<Vector2Int> list))
            {
                foreach (var pos in list)
                {
                    if (!owner.MapInfo.IsWalkable(pos) 
                        && owner.bombPos.BombInfo.ContainsKey(
                            owner.ToBombPutPos(owner.MapInfo.GetRealCoord(pos))))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="owner">依附对象</param>
        /// <param name="controller">控制器</param>
        public void Init(EnemyAIController owner, CharacterController controller)
        {
            _characterController = controller;
            this.owner = owner;
        }

        /// <summary>
        /// 获取下一个位置
        /// </summary>
        /// <returns>返回是否有下一个位置</returns>
        private bool GetNextPos()
        {
            if (_path.Next(out var nextPos))
            {
                nextTargetPos = owner.MapInfo.GetRealCoord(nextPos); 
                return true;
            }
            return false;
        }
        

        /// <summary>
        /// 判断是否到达下一个位置
        /// </summary>
        /// <returns>返回判断结果</returns>
        public bool IsArriveNextPos()
        {
            if (Vector3.Distance(owner.ToBombPutPos(owner.transform.position), nextTargetPos) < 0.01f)
            {
                isMoving = false;
                hasTarget = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移动到移动目标
        /// </summary>
        /// <param name="path">目标路径</param>
        public void MoveToTarget(PathInfo path)
        {
            if (path == null)
            {
                Debug.Log("path为空");
                return;
            }

            if (path.NowPath(out var nextPos))
            {
                Debug.LogError("path.NowPath为空");
                return;
            }
            nextTargetPos = owner.MapInfo.GetRealCoord(nextPos);
            _path = path;
            isMoving = true;
            hasTarget = true;
            isSafe = true;
            
        }

        /// <summary>
        /// 移动到下一个位置
        /// </summary>
        public void Moving()
        {
            _characterController.SimpleMove(owner.moveSpeed * (nextTargetPos - owner.transform.position).normalized);
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMoving()
        {
            _characterController.SimpleMove(Vector3.zero);
        }

    }
}