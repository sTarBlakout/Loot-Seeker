using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameplay.Pawns;
using Gameplay.Controls;
using Gameplay.Environment;
using UnityEngine;

namespace Gameplay.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance => _instance;
        private static GameManager _instance;
        
        private List<OrderManagerBase> _turnParticipants = new List<OrderManagerBase>();
        private PawnController _player;
        private CameraManager _cameraManager;
        private GameArea _gameArea;
        
        public PawnController PlayerPawn => _player;

        private void Start()
        {
            _instance = this;
            StartCoroutine(InitGame());
        }

        private IEnumerator InitGame()
        {
            _gameArea = FindObjectOfType<GameArea>();
            _cameraManager = FindObjectOfType<CameraManager>();
            yield return new WaitUntil(() => _gameArea.IsInitialized());

            _turnParticipants.Clear();
            foreach (var pawn in _gameArea.pawnsGameObjects) _turnParticipants.Add(pawn.GetComponent<OrderManagerBase>());
            _player = _gameArea.pawnsGameObjects.First(pawn => pawn.gameObject.CompareTag("Player")).GetComponent<PawnController>();
            StartCoroutine(GameCoroutine());
        }

        private IEnumerator GameCoroutine()
        {
            yield return new WaitUntil(_gameArea.IsInitialized);
            
            while (IsGameRunning())
            {
                foreach (var participant in _turnParticipants)
                {
                    if (!participant.CanTakeTurn()) continue;
                    
                    participant.StartTurn();
                    yield return new WaitWhile(() => participant.IsTakingTurn);
                }
            }
        }

        private bool IsGameRunning()
        {
            return _player.IsAlive;
        }

        #region Uitilities

        public void PlayParticle(ParticleSystem particle, bool activate)
        {
            if (activate) particle.Play();
            else particle.Stop();
        }

        #endregion
    }
}
