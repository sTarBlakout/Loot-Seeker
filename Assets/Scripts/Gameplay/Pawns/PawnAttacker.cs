using System;
using Gameplay.Interfaces;
using UnityEngine;

namespace Gameplay.Pawns
{
    public class PawnAttacker : MonoBehaviour
    {
        private IPawn _pawn;
        private PawnAnimator _pawnAnimator;
        
        private IPawn _target;
        private Action _onAttacked;
        
        public void Init(IPawn pawn, PawnAnimator pawnAnimator)
        {
            _pawn = pawn;
            _pawnAnimator = pawnAnimator;
        }

        public void AttackTarget(IPawn target, Action onAttacked)
        {
            _target = target;
            _onAttacked = onAttacked;
            _target.Damageable.PreDamage(_pawn, () => _pawnAnimator.AnimateAttack());
        }

        private void OnDamageDealt(int value)
        {
            _pawn.PawnData.ModifyLevelBy(value);
            _target.Damageable.PostDamage(() => _onAttacked?.Invoke());
        }

        #region Animation Events
       
        public void Hit()
        {
            _target.Damageable.Damage(_pawn.PawnData.DamageValue, OnDamageDealt);
        }
        
        #endregion
    }
}
