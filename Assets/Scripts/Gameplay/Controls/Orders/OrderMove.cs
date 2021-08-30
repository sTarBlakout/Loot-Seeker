using System;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Core;
using Gameplay.Сharacters;
using UnityEngine;

namespace Gameplay.Controls.Orders
{
    public class OrderMove : OrderBase
    {
        protected OrderArgsMove args;
        protected CompleteOrderArgsMove completeArgs;

        public OrderMove(OrderArgsBase args) : base(args) { this.args = (OrderArgsMove) args; }

        private bool _isFullRoute = true;

        public override void StartOrder()
        {
            completeArgs = new CompleteOrderArgsMove();
            
            if (args.ToTile != null)
            {
                if (args.GameArea.IsTileBlocked(args.ToTile.NavPosition))
                {
                    completeArgs.Result = OrderResult.Fail;
                    completeArgs.FailReason = OrderFailReason.BlockedArea;
                    args.OnCompleted?.Invoke(completeArgs);
                    return;
                }

                var pathTiles = args.PathsToTiles.FirstOrDefault(pair => pair.Key == args.ToTile).Value;
                if (pathTiles == null)
                {
                    completeArgs.Result = OrderResult.Fail;
                    completeArgs.FailReason = OrderFailReason.TooFar;
                    args.OnCompleted?.Invoke(completeArgs);
                    return;
                }
                
                TryTraversePath(pathTiles.Select(pathTile => pathTile.Item1).ToList());
            }
            else if (args.ToPawn != null)
            {
                var pathToPawn = args.PathsToPawns.First(pawn => pawn.Key == args.ToPawn).Value;
                pathToPawn.RemoveAt(pathToPawn.Count - 1);
                TryTraversePath(pathToPawn.Select(pathTile => pathTile.Item1).ToList());
            }
            else
            {
                if (args.GameArea.IsTileBlocked(args.To))
                {
                    completeArgs.Result = OrderResult.Fail;
                    completeArgs.FailReason = OrderFailReason.BlockedArea;
                    args.OnCompleted?.Invoke(completeArgs);
                    return;
                }

                args.GameArea.GeneratePathToPosition(args.PawnController.transform.position, args.To, OnPathGenerated);
            }
        }

        protected void TryTraversePath(List<Vector3> path)
        {
            if (path.Count - 1 > args.MaxSteps)
            {
                if (args.MoveAsFarAsCan)
                {
                    _isFullRoute = false;
                    path = path.Take(args.MaxSteps + 1).ToList();
                }
                else
                {
                    completeArgs.Result = OrderResult.Fail;
                    completeArgs.FailReason = OrderFailReason.TooFar;
                    args.OnCompleted?.Invoke(completeArgs);
                    return;
                }
            }
            
            TraversePath(path);
        }

        private void TraversePath(List<Vector3> path)
        {
            args.OnUsedMovePointsCallback?.Invoke(path.Count - 1);
            args.GameArea.BlockTileAtPos(path[0], false);
            args.GameArea.BlockTileAtPos(path.Last(), true);
            args.PawnController.MovePath(path, OnReachedDestination);
        }
        
        protected void OnPathGenerated(List<Vector3> path)
        {
            if (path.Count - 1 > args.MaxSteps)
            {
                completeArgs.Result = OrderResult.Fail;
                completeArgs.FailReason = OrderFailReason.TooFar;
                args.OnCompleted?.Invoke(completeArgs);
                return;
            }
            TryTraversePath(path);
        }
        
        private void OnReachedDestination()
        {
            completeArgs.Result = _isFullRoute ? OrderResult.Succes : OrderResult.HalfSucces;
            args.OnCompleted?.Invoke(completeArgs);
        }
    }
}
