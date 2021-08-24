using System.Collections;
using System.Collections.Generic;
using Gameplay.Core;
using Gameplay.Environment;
using Gameplay.Interfaces;
using Lean.Touch;
using UnityEngine;

namespace Gameplay.Controls
{
    public class OrderManagerPlayer : OrderManagerBase
    {
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
            if (_order != null || !isTakingTurn || !areAllPathsGenerated) return;
            if (!Physics.Raycast(finger.GetRay(), out var hitInfo, Mathf.Infinity) || finger.IsOverGui) return;

            // Clicked on map, process simple movement
            var tile = hitInfo.collider.transform.parent.GetComponent<GameAreaTile>();
            if (tile != null)
            {
                StartOrderMove(tile);
                return;
            }
            
            // Checking if interactable
            _interactable = hitInfo.collider.transform.parent.GetComponent<IInteractable>();
            
            if (_interactable is IDamageable damageable)
            {
                StartOrderAttack(damageable, false);
                return;
            }
        }

        protected override bool CanDoActions() { return true; }

        protected override bool CanMove() { return true; }
    }
}
