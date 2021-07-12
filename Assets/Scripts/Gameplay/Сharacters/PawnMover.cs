using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Сharacters
{
    public class PawnMover : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float maxMoveSpeed;
        [SerializeField] private float acceleration;
        
        private List<Vector3> _pathToCell;
        private float _currSpeed;
        private float _totalDistToNextPoint;

        private void Update()
        {
            ProcessMovement();
        }
        
        public void MoveToCell(List<Vector3> path)
        {
            _pathToCell = new List<Vector3>(path);
            RecalcTotalDistToNextPoint();
        }

        private void ProcessMovement()
        {
            if (_pathToCell == null) return;

            if (_pathToCell.Count != 0)
            {
                var currTarget = _pathToCell[0];
                if (Rotate(currTarget)) return;
                if (Move(currTarget)) return;
                
                _pathToCell.RemoveAt(0);
                _currSpeed = 0f;
                RecalcTotalDistToNextPoint();
            }
            else
            {
                _pathToCell = null;
            }
        }

        private void RecalcTotalDistToNextPoint()
        {
            if (_pathToCell.Count != 0)
                _totalDistToNextPoint = Vector3.Distance(transform.position, _pathToCell[0]);
        }

        private bool Rotate(Vector3 position)
        {
            var direction = position - transform.position;
            
            if (Vector3.Angle(transform.forward, direction) > 1f) 
            {
                var lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                return true;
            }
            
            transform.LookAt(position);
            return false;
        }

        private bool Move(Vector3 position)
        {
            if (Vector3.Distance(transform.position, position) < _totalDistToNextPoint / 2)
                _currSpeed -= Mathf.Min(acceleration * Time.deltaTime, 1);
            else
                _currSpeed += Mathf.Min(acceleration * Time.deltaTime, 1);

            _currSpeed = Mathf.Clamp(_currSpeed, 0f, 1f);
            transform.position = Vector3.MoveTowards(transform.position, position, maxMoveSpeed * _currSpeed * Time.deltaTime);

            return position != transform.position;
        }
    }
}
