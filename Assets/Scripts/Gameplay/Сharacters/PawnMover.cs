using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Сharacters
{
    public class PawnMover : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float movementSpeed;

        [Header("Components")] 
        [SerializeField] private GameObject charGraphics;

        private PawnAnimator _pawnAnimator;
        private List<(Vector3 pos, bool rot)> _vectorPath;
        private Vector3 _rotateToPos;

        private Action _onReachedDestination;
        private Action _onRotated;

        private void Update()
        {
            ProcessMovement();
            ProcessRotation();
        }

        public void Init(PawnAnimator pawnAnimator)
        {
            _pawnAnimator = pawnAnimator;
        }
        
        public void RotateTo(Vector3 position, Action onRotated)
        {
            _rotateToPos = position;
            _onRotated = onRotated;
        }
        
        public void MovePath(List<Vector3> path, Action onReachedDestination)
        {
            _onReachedDestination = onReachedDestination;
            _vectorPath = TraversePath(path);
            TargetNextPoint();
        }

        private List<(Vector3 pos, bool rot)> TraversePath(List<Vector3> path)
        {
            var rez = new List<(Vector3 pos, bool rot)>();

            var traverser = new GameObject("Traverser");
            traverser.transform.position = transform.position;
            traverser.transform.rotation = transform.rotation;
            
            foreach (var point in path)
            {
                (Vector3 pos, bool rot) posRot = (point, false);
                if (Vector3.Angle(traverser.transform.forward, point - traverser.transform.position) > 5f)
                {
                    rez[rez.Count - 1] = (rez[rez.Count - 1].pos, true);
                    traverser.transform.LookAt(point);
                }
            
                traverser.transform.position = point;
                rez.Add(posRot);
            }
            
            rez[rez.Count - 1] = (rez[rez.Count - 1].pos, true);
            rez.RemoveAll(posRot => !posRot.rot);
            Destroy(traverser);
            
            return rez;
        }

        private void ProcessRotation()
        {
            if (_onRotated == null) return;
            if (Rotate(_rotateToPos)) return;
            _onRotated?.Invoke();
            _onRotated = null;
        }

        private void ProcessMovement()
        {
            if (_vectorPath == null) return;

            if (_vectorPath.Count != 0)
            {
                if (Rotate(_vectorPath[0].pos)) return;
                if (Move(_vectorPath[0].pos)) return;
                TargetNextPoint();
            }
            else
            {
                _vectorPath = null;
            }
        }

        private void TargetNextPoint()
        {
            _vectorPath.RemoveAt(0);
            if (_vectorPath.Count == 0) _onReachedDestination?.Invoke();
        }

        private bool Rotate(Vector3 position)
        {
            var direction = position - transform.position;
            var angle = Vector3.Angle(charGraphics.transform.forward, direction);

            if (angle > 10f)
            {
                if (Vector3.Dot(direction, charGraphics.transform.right) > 0)
                {
                    _pawnAnimator.AnimateRotationRight(true);
                }
                else
                {
                    _pawnAnimator.AnimateRotationLeft(true);
                }
            }
            else
            {
                _pawnAnimator.AnimateRotationLeft(false);
                _pawnAnimator.AnimateRotationRight(false);
            }

            if (angle > 1f) 
            {
                var lookRotation = Quaternion.LookRotation(direction);
                charGraphics.transform.rotation = Quaternion.RotateTowards(charGraphics.transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                return true;
            }
            
            charGraphics.transform.LookAt(position);
            return false;
        }

        private bool Move(Vector3 postion)
        {
            transform.position = Vector3.MoveTowards(transform.position, postion, movementSpeed * Time.deltaTime);
            _pawnAnimator.AnimateWalk(postion != transform.position);
            return postion != transform.position;
        }
    }
}
