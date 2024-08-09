using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueArcade
{
    public class WeaponBox : MonoBehaviour
    {
        private StageManager _stageManager;
        public void Init(StageManager stageManager)
        {
            _stageManager = stageManager;
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerManager.Player)
            {
                _stageManager.AddScore();
                _stageManager.SpawnWeaponBox();
                _stageManager.SetRandomCharacter();
                Destroy(gameObject);
            }
        }
    }
}
