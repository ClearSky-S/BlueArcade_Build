using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using Random = UnityEngine.Random;

namespace BlueArcade
{
    public struct WeaponInput
    {
        public bool Pressed;
        public bool PressedThisFrame;
    }
    public enum TriggerType
    {
        SemiAuto, // 키 누를 때마다 한 발씩 발사
        Auto // 키 누르고 있으면 연속 발사
    }
    public class Weapon : MonoBehaviour
    {
        public TriggerType triggerType;
        public float cooltime = 0.25f;
        public int burst = 1;
        public float burstInterval = 0.1f;
        public int bulletCount = 1;
        public float recoil = 3f;
        public float dispersion = 0f;

        public Projectile projectilePrefab;

        [SerializeField] private Transform _firePoint;
        [SerializeField] private MMFeedbacks _attackFeedback;
        
        private float _lastPressedTime = -999f;
        private float _lastFiredTime = -999f;
        
        private PlayerController _player;
        private Animator _animator;
        
        private const string _attackAnimTrigger = "attack";
        
        private IObjectResolver _resolver;
        private ObjectPooler _pooler;

        [Inject]
        private void Construct(IObjectResolver resolver, ObjectPooler pooler)
        {
            _resolver = resolver;
            _pooler = pooler;
        }
        public void Init(PlayerController player)
        {
            _player = player;
            _animator = GetComponentInChildren<Animator>();
        }
        
        public void ProcessInput(WeaponInput input)
        {
            switch (triggerType)
            {
                case TriggerType.SemiAuto:
                    if (input.PressedThisFrame)
                    {
                        _lastPressedTime = Time.time;
                    }
                    if (Time.time - _lastPressedTime < 0.1f)
                    {
                        Attack().Forget();
                    }
                    break;
                case TriggerType.Auto:
                    if (input.Pressed)
                    {
                        Attack().Forget();
                    }
                    break;
            }
        }

        private async UniTaskVoid Attack()
        {
            if (Time.time - _lastFiredTime < cooltime)
            {
                return;
            }
            var cancellationToken = this.GetCancellationTokenOnDestroy();
            _lastFiredTime = Time.time;
            
            float recoilFactor = 1f;
            for(int i = 0; i < burst; i++)
            {

                if (i > 0)
                {
                    await UniTask.Delay((int)(burstInterval * 1000), cancellationToken: cancellationToken);
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                _animator.SetTrigger(_attackAnimTrigger);
                _attackFeedback.PlayFeedbacks();
                
                // repeat for bulletCount
                for (int j = 0; j < bulletCount; j++)
                {
                    var projectile = _pooler.GetObject<Projectile>(projectilePrefab.gameObject);
                    projectile.transform.position = _firePoint.position;
                    Vector2 direction = _player.IsFacingRight ? Vector2.right : Vector2.left;
                    direction = Quaternion.Euler(0, 0, Random.Range(-dispersion, dispersion)) * direction;
                    projectile.Init(direction);
                }

                if (recoil != 0)
                {
                    _player.Rigidbody2D.AddForce(Vector2.right*(_player.IsFacingRight?-1:1) * recoil * recoilFactor, ForceMode2D.Impulse);
                }
                recoilFactor *= 0.7f;
            }
        }

    }
}
