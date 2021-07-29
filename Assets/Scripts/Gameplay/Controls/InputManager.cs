using System;
using System.Collections;
using System.Collections.Generic;
using Gameplay.Interfaces;
using Gameplay.Сharacters;
using Lean.Touch;
using SimplePF2D;
using UnityEngine;

namespace Gameplay.Controls
{
    public class InputManager : MonoBehaviour
    {
        private PawnController _playerController;
        private Coroutine _waitPathCor;
        private Path _path;

        private Order _order;
        private IInteractable _interactable;
        private IDamageable _damageable;

        private void Awake()
        {
            _playerController = GameObject.FindWithTag("Player").GetComponent<PawnController>();
            _path = new Path(FindObjectOfType<SimplePathFinding2D>());
        }

        private void OnEnable()
        {
            LeanTouch.OnFingerTap += HandleFingerTap;
        }
        
        private void OnDisable()
        {
            LeanTouch.OnFingerTap -= HandleFingerTap;
        }

        private void HandleFingerTap(LeanFinger finger)
        {
            if (!Physics.Raycast(finger.GetRay(), out var hitInfo, Mathf.Infinity) || finger.IsOverGui) return;
            
            // Clicked on map, process simple movement
            if (hitInfo.collider.GetComponent<GameArea>() != null)
            {
                _order = Order.Move;
                GeneratePathToPosition(hitInfo.point, OnPathGenerated);
                return;
            }
            
            // Checking if pawn
            _interactable = hitInfo.collider.transform.parent.GetComponent<IInteractable>();
            if (_interactable != null)
            {
                if (_interactable is IDamageable damageable) _damageable = damageable;

                // Clicked on enemy, try attack
                if (_damageable != null && _damageable.IsInteractable() && _damageable.IsEnemyFor(_playerController))
                {
                    _order = Order.Attack;
                    GeneratePathToPosition(_damageable.Position, OnPathGenerated);
                    return;
                }
            }
        }

        private void OnReachedDestination()
        {
            switch (_order)
            {
                case Order.Attack: OrderRotate(_damageable.Position); break;
                default: _order = Order.None; break;
            }
        }

        private void OnRotated()
        {
            switch (_order)
            {
                case Order.Attack: OrderAttack(_damageable); break;
                default: _order = Order.None; break;
            }
        }

        private void OnAttacked()
        {
            Debug.Log("Just Attacked somebody");
        }
        
        private void OnPathGenerated(List<Vector3> path)
        {
            switch (_order)
            {
                case Order.Move: OrderSimpleMove(path); break;
                case Order.Attack: OrderMoveForAttacking(path); break;
                default: _order = Order.None; break;
            }
        }

        private void OrderRotate(Vector3 position)
        {
            _playerController.RotateTo(position, OnRotated);
        }

        private void OrderSimpleMove(List<Vector3> path)
        {
            _playerController.MovePath(path, OnReachedDestination);
        }
        
        private void OrderMoveForAttacking(List<Vector3> path)
        {
            path.RemoveAt(path.Count - 1);
            _playerController.MovePath(path, OnReachedDestination);
        }
        
        private void OrderAttack(IDamageable _damageable)
        {
            _playerController.AttackTarget(_damageable, OnAttacked);
        }

        private void GeneratePathToPosition(Vector3 position, Action<List<Vector3>> onGeneratedPath)
        {
            _path.CreatePath(_playerController.transform.position, position);
            if (_waitPathCor != null) StopCoroutine(_waitPathCor);
            _waitPathCor = StartCoroutine(WaitGeneratedPath(onGeneratedPath));
        }

        private IEnumerator WaitGeneratedPath(Action<List<Vector3>> onGeneratedPath)
        {
            yield return new WaitUntil(() => _path.IsGenerated());
            var vectorPath = new List<Vector3>();
            for (int i = 0; i < _path.GetPathPointList().Count; i++) vectorPath.Add(_path.GetPathPointWorld(i));
            onGeneratedPath(vectorPath);
        }
    }
}
