using Gameplay.Core;
using UnityEngine;

namespace Gameplay.Interfaces
{
    public interface IPawn
    {
        bool IsAlive();
        PawnRelation RelationTo(IPawn pawn);
        
        IPawnData PawnData { get; }
        IDamageable Damageable { get; }
    }
}
