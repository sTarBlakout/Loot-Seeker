using System;
using Gameplay.Сharacters;

namespace Gameplay.Interfaces
{
    public interface IDamageable : IInteractable
    {
        void PrepareForDamage(IDamageable attacker, Action onPrepared);
        void Damage(int value, Action<int> onDamageDealt);
        bool IsEnemyFor(PawnController pawn);
    }
}
