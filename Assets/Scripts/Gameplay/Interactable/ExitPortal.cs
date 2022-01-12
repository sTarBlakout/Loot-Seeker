using System;
using Gameplay.Core;
using Gameplay.Interfaces;
using UnityEngine;

namespace Gameplay.Interactable
{
    public class ExitPortal : MonoBehaviour, IPawnInteractable, IPawnInteractableData
    {
        #region IPawn Implementation
        
        public PawnRelation RelationTo(IPawn pawn)
        {
            return PawnRelation.Interactable;
        }
        
        public bool IsBlockingTile => true;
        public Vector3 WorldPosition => transform.position;
        public Action<GameObject> OnDestroyed { get; set; }
        public IPawnInteractableData PawnData => this;
        IPawnData IPawn.PawnData => PawnData;


        #endregion

        #region IPawnInteractable Implementation

        private IPawnNormal _interactor;
        
        public void PreInteract(IPawnNormal interactor, Action onPreInteract)
        {
            _interactor = interactor;
            onPreInteract?.Invoke();
        }

        public void Interact(Action onInteract)
        {
            onInteract?.Invoke();
        }

        public void PostInteract(Action onPostInteract)
        {
            onPostInteract?.Invoke();
        }

        #endregion
    }
}
