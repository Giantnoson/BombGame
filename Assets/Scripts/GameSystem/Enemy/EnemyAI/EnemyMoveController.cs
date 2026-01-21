using System;
using System.Collections.Generic;
using GameSystem.Map;
using JetBrains.Annotations;
using UnityEngine;

namespace GameSystem.Enemy
{
    public class EnemyMoveController : MonoBehaviour
    {
        private EnemyAIController Owner;
        
        private bool isMoving = false;

        private bool hasTarget = false;

        private Vector3 targetPos;
        
        [NotNull]
        private List<PathInfo> path = new List<PathInfo>();

        private CharacterController characterController;
        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            if (characterController == null)
            {
                Debug.LogError("EnemyMoveController:在这个对象当中不存在characterController");
            }
        }
        
        private void Update()
        {
            if (isMoving)
            {
                //TODO: Move
            }
        }
        
        public void Init(EnemyAIController owner)
        {
            Owner = owner;
        }
        

        public void IsArrivePos()
        {
            if (Vector3.Distance(Owner.ToBombPutPos(Owner.transform.position), targetPos) < 0.1f)
            {
                isMoving = false;
                hasTarget = false;
            }
        }

        public void MoveTo(Vector3 targetPos, List<PathInfo> path)
        {
            this.targetPos = targetPos;
            this.path = path;
            isMoving = true;
            hasTarget = true;
        }

        public void Moving()
        {
            if (hasTarget)
            {
                
            }
        }



    }
}