using System;
using TMPro;
using UnityEngine;

namespace Gameplay.Сharacters
{
    public class PawnHealthIndicator : MonoBehaviour
    {
        [SerializeField] private TextMeshPro levelText;
        
        private PawnData _pawnData;
        private Transform _cameraTransform;

        public void Init(PawnData pawnData)
        {
            _pawnData = pawnData;
            _cameraTransform = Camera.main.transform;
        }
        
        private void Update()
        {
            levelText.text = _pawnData.Level.ToString(); 
            transform.LookAt(_cameraTransform.position);
        }
    }
}
