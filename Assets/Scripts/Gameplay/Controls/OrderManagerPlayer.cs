using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Environment;
using Gameplay.Interfaces;
using Lean.Touch;
using UnityEngine;

namespace Gameplay.Controls
{
    public class OrderManagerPlayer : OrderManagerBase
    {
        public IPawn Player => _pawnController;
        public GameAreaTile selectedTile;

        public Action<GameAreaTile> OnTileClicked;
        public Action<IPawn> OnPawnClicked;

        #region Finger Handling

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
            if (_order != null || !isTakingTurn || !areAllPathsGenerated || selectedTile != null) return;
            if (!Physics.Raycast(finger.GetRay(), out var hitInfo, Mathf.Infinity) || finger.IsOverGui) return;
            
            var tile = hitInfo.collider.transform.parent.GetComponent<GameAreaTile>();
            if (tile != null)
            {
                // Clicked on empty tile, proceed with movement
                if (tile.CanWalkIn(_pawnController) && pathsToTiles.ContainsKey(tile))
                {
                    selectedTile = tile;
                    DrawWay(true, OrderType.Move);
                    OnTileClicked?.Invoke(tile);
                    return;
                }

                // Clicked on tile with enemy pawn, proceed with attack
                _targetPawn = tile.HasEnemyForPawn(_pawnController);
                if (_targetPawn != null && _targetPawn.Damageable != null && IsPawnReachable(_targetPawn) && _targetPawn.IsAlive)
                {
                    var pathToPawn = pathsToPawns[_targetPawn];
                    selectedTile = pathToPawn[pathToPawn.Count - 2];
                    DrawWay(true, OrderType.Attack);
                    OnPawnClicked?.Invoke(_targetPawn);
                    return;
                }
            }
        }
        
        #endregion

        #region OrderManagment

        public void StartOrder(OrderType order)
        {
            HighlightReachableTiles(false);
            HighlightEnemyTiles(false);
            HighlightInteractableTiles(false);
            
            switch (order)
            {
                case OrderType.Attack: StartOrderAttack(_targetPawn, false); break;
                case OrderType.Move: StartOrderMove(selectedTile); break;
            }
        }
        

        public void ResetOrder()
        {
            DrawWay(false);
            selectedTile = null;
            _targetPawn = null;
        }
        
        #endregion


        #region Visuals Managment
        
        private void DrawWay(bool draw, OrderType order = OrderType.None)
        {
            if (draw)
            {
                if (order == OrderType.None) return;

                _way = _gameArea.CreateWay().SetFollowPawn(_pawnController.transform).SetOrder(order);
                
                if (selectedTile != currLocationTile)
                {
                    _way.SetWayLine(_pawnController.Data.WayMoveLinePrefab)
                        .BuildWay(_gameArea.OptimizePathForPawn(pathsToTiles[selectedTile], _pawnController.transform));
                }

                if (order == OrderType.Attack)
                {
                    _way.SetAttackLine(_pawnController.Data.WayAttackeLinePrefab)
                        .BuildAttack(_targetPawn);
                }
            }
            else
            {
                if (_way == null) return;
                _way.DestroyWay();
                _way = null;
            }
        }

        private void HighlightReachableTiles(bool highlight)
        {
            var tilesList = new List<GameAreaTile>(pathsToTiles.Keys);
            foreach (var tile in tilesList) tile.ActivateParticle(TileParticleType.ReachableTile, highlight);
        }
        
        private void HighlightEnemyTiles(bool highlight)
        {
            var pathsToEnemies = pathsToPawns.Where(pawnPath => 
                pawnPath.Key.RelationTo(_pawnController) == PawnRelation.Enemy 
                && IsPawnReachable(pawnPath.Key)
                && pawnPath.Key.IsAlive);
            var tilesList = pathsToEnemies.Select(pathToEnemy => pathToEnemy.Value[pathToEnemy.Value.Count - 1]).ToList();
            foreach (var tile in tilesList) tile.ActivateParticle(TileParticleType.ReachableEnemy, highlight);
        }

        private void HighlightInteractableTiles(bool highlight)
        {
            var pathsToInteractables = pathsToPawns.Where(pawnPath => 
                pawnPath.Key.RelationTo(_pawnController) == PawnRelation.Interactable
                && IsPawnReachable(pawnPath.Key)
                && pawnPath.Key.IsAlive);

            var tilesList = pathsToInteractables.Select(pathsToInteractable => pathsToInteractable.Value[pathsToInteractable.Value.Count - 1]).ToList();
            foreach (var tile in tilesList) tile.ActivateParticle(TileParticleType.ReachableInteractable, highlight);
        }
        
        #endregion

        #region Overrides

        protected override void OnAllPathsGenerated()
        {
            base.OnAllPathsGenerated();
            HighlightReachableTiles(true);
            HighlightEnemyTiles(true);
            HighlightInteractableTiles(true);
        }

        protected override void ProcessPostOrder()
        {
            base.ProcessPostOrder();
            ResetOrder();
        }

        public override void CompleteTurn()
        {
            HighlightReachableTiles(false);
            HighlightEnemyTiles(false);
            HighlightInteractableTiles(false);
            ResetOrder();
            base.CompleteTurn();
        }

        protected override bool CanDoActions() { return true; }

        protected override bool CanMove() { return true; }
        
        #endregion
    }
}
