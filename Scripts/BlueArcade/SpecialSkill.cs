using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace BlueArcade
{
    public class SpecialSkill : MonoBehaviour
    {
        public float CoolTimeRatio => 1 - Mathf.Clamp01((Time.time - _lastUsedTime) / _coolTime);
        [SerializeField] private InputAction _specialSkillAction;
        [SerializeField] private GameObject _explosionPrefab;
        private float _coolTime = 25f;
        private float _lastUsedTime = -999f;
        private UIManager _uiManager;
        private StageManager _stageManager;
        private List<Transform> _targets;
        
        [Inject]
        private void Construct(UIManager uiManager)
        {
            _uiManager = uiManager;
            // all child objects of this object
            _targets = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                _targets.Add(transform.GetChild(i));
            }
        }

        public async UniTask TryUseSpecialSkill()
        {
            if (CoolTimeRatio > 0)
            {
                return;
            }
            _uiManager.OpenSpecialSkillPopup();
            _lastUsedTime = Time.time;
            await UniTask.Delay(300, cancellationToken: destroyCancellationToken);
            foreach (var target in _targets)
            {
                var explosion = Instantiate(_explosionPrefab, target.transform.position, Quaternion.identity);
                // circle cast
                var colliders = Physics2D.OverlapCircleAll(target.position, 1f);
                foreach (var collider in colliders)
                {
                    if (collider.TryGetComponent(out Enemy enemy))
                    {
                        enemy.Damage(100);
                    }
                }
                await UniTask.Delay(400, cancellationToken: destroyCancellationToken);
            }
        }
    }
}
