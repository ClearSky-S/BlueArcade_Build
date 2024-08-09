using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace BlueArcade
{
    public class Enemy : PoolableObject
    {
        public enum EnemyState
        {
            Normal,
            Angry
        }
        public int startHealth = 100;
        public float defaultSpeed = 3f;
        private float speed => _state == EnemyState.Normal ? defaultSpeed : defaultSpeed * 1.5f;
        [SerializeField] private RuntimeAnimatorController _defaultAnimatorController;
        [SerializeField] private RuntimeAnimatorController _angryAnimatorController;

        public bool IsFacingRight
        {
            get => _isFacingRight;
            set
            {
                if(value != _isFacingRight)
                {
                    _isFacingRight = value;
                    _model.transform.rotation = Quaternion.Euler(0, value ? 180 : 0, 0);
                }
            }
        }
        public Rigidbody2D Rigidbody2D => _rb;
        
        [SerializeField] private GameObject _model;
        private int _currentHealth;
        private IObjectResolver _resolver;
        private ObjectPooler _objectPooler;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private bool _isFacingRight = false;
        private EnemyState _state = EnemyState.Normal;
       
        
        [Inject]
        private void Construct(IObjectResolver resolver, ObjectPooler objectPooler)
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _resolver = resolver;
            _objectPooler = objectPooler;
            _currentHealth = startHealth;
        }

        private void FixedUpdate()
        {
            CheckCollision();
            Move();
        }

        public void Damage(int damage)
        {
            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void SetState(EnemyState state)
        {
            _state = state;
            _model.GetComponent<Animator>().runtimeAnimatorController = state == EnemyState.Normal ? _defaultAnimatorController : _angryAnimatorController;
        }

        public override void OnReturnPool()
        {
            base.OnReturnPool();
            IsFacingRight = false;
            _currentHealth = startHealth;
            SetState(EnemyState.Normal);
        }

        void Die()
        {
            transform.position = new Vector3(-999, -999, 0);
            _objectPooler.ReturnObject(this);
        }

        void CheckCollision()
        {
            Vector2 raycastOrigin = _collider.bounds.center;
            raycastOrigin.x = IsFacingRight ? _collider.bounds.max.x : _collider.bounds.min.x;
            RaycastHit2D hit = Physics2D.Raycast(raycastOrigin,
                IsFacingRight? Vector2.right : Vector2.left,
                0.1f,
                LayerManager.PlatformMask);
            if (hit.collider != null)
            {
                IsFacingRight = !IsFacingRight;
                Vector2 velocity = _rb.velocity;
                velocity.x = speed * (IsFacingRight ? 1 : -1);
                _rb.velocity = velocity;
            }
        }

        void Move()
        {
            Vector2 velocity = _rb.velocity;
            velocity.x = speed * (IsFacingRight ? 1 : -1);
            _rb.velocity = Vector2.MoveTowards(_rb.velocity, velocity, 20f * Time.deltaTime);
        }
    }
}
