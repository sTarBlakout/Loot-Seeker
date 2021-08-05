using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Interfaces;
using Gameplay.Сharacters;
using UnityEngine;

namespace Gameplay.Controls
{
    public abstract class OrderManagerBase : MonoBehaviour
    {
        protected bool isTakingTurn = false;
        
        protected PawnController _pawnController;
        protected GameArea _gameArea;

        protected Order _order;
        protected IInteractable _interactable;
        protected IDamageable _damageable;

        public bool IsTakingTurn => isTakingTurn;
    
        protected virtual void Awake()
        {
            _pawnController = GetComponent<PawnController>();
            _gameArea = FindObjectOfType<GameArea>();

            _pawnController.onDeath += OnDeath;
        }

        public virtual bool CanTakeTurn()
        {
            return _pawnController.IsAlive();
        }

        public virtual void StartTurn()
        {
            isTakingTurn = true;
        }
        
        protected void CompleteTurn()
        {
            isTakingTurn = false;
            _order = Order.None;
        }

        protected virtual void StartOrderMove(Vector3 fromPos, Vector3 toPos)
        {
            if (_gameArea.IsTileBlocked(toPos))
            {
                Debug.Log("Tile is blocked!");
                return;
            }
            _order = Order.Move;
            _gameArea.GeneratePathToPosition(fromPos, toPos, OnPathGenerated);
        }

        protected virtual void StartOrderAttack(IDamageable damageable)
        {
            _damageable = damageable;
            if (!_damageable.IsInteractable() || !_damageable.IsEnemyFor(_pawnController)) return;
            _order = Order.Attack;
            _gameArea.GeneratePathToPosition(_pawnController.transform.position, _damageable.Position, OnPathGenerated);
        }

        private void OnReachedDestination()
        {
            switch (_order)
            {
                case Order.Attack: OrderRotate(_damageable.Position); break;
                default: CompleteTurn(); break;
            }
        }
    
        private void OnRotated()
        {
            switch (_order)
            {
                case Order.Attack: OrderAttack(_damageable); break;
                default: CompleteTurn(); break;
            }
        }
    
        private void OnAttacked()
        {
            Debug.Log("Just Attacked somebody");
            CompleteTurn();
        }
            
        protected void OnPathGenerated(List<Vector3> path)
        {
            switch (_order)
            {
                case Order.Move: OrderSimpleMove(path); break;
                case Order.Attack: OrderMoveForAttacking(path); break;
                default: CompleteTurn(); break;
            }
        }

        private void OnDeath()
        {
            _gameArea.BlockTileAtPos(transform.position, false);
        }
    
        private void OrderRotate(Vector3 position)
        {
            _pawnController.RotateTo(position, OnRotated);
        }
    
        private void OrderSimpleMove(List<Vector3> path)
        {
            _gameArea.BlockTileAtPos(path[0], false);
            _gameArea.BlockTileAtPos(path.Last(), true);
            _pawnController.MovePath(path, OnReachedDestination);
        }
            
        private void OrderMoveForAttacking(List<Vector3> path)
        {
            path.RemoveAt(path.Count - 1);
            _gameArea.BlockTileAtPos(path[0], false);
            _gameArea.BlockTileAtPos(path.Last(), true);
            _pawnController.MovePath(path, OnReachedDestination);
        }
            
        private void OrderAttack(IDamageable _damageable)
        {
            _pawnController.AttackTarget(_damageable, OnAttacked);
        }
    }
}
