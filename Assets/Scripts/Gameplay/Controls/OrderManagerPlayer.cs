using Gameplay.Core;
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
            if (_order != Order.None) return;
            if (!Physics.Raycast(finger.GetRay(), out var hitInfo, Mathf.Infinity) || finger.IsOverGui) return;
            
            // Clicked on map, process simple movement
            if (hitInfo.collider.GetComponent<GameArea>() != null)
            {
                StartOrderMove(_pawnController.transform.position, hitInfo.point);
                return;
            }
            
            // Checking if interactable
            _interactable = hitInfo.collider.transform.parent.GetComponent<IInteractable>();
            
            if (_interactable is IDamageable damageable)
            {
                StartOrderAttack(damageable);
                return;
            }
        }
    }
}
