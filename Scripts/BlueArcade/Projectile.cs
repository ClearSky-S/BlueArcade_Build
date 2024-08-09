using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using UnityEngine;
using VContainer;

namespace BlueArcade
{
    public class Projectile : PoolableObject
    {
        public float lifetime = 1f;
        public float speed = 10f;
        public bool destroyOnCollision = true;
        public int damage = 100;
        public float knockback = 2f;
        public LayerMask collisionMask = LayerManager.Enemy | LayerManager.Platform;
        [SerializeField] private MMFeedbacks _hitFeedback;

        
        private float _spawnTime;
        private Vector2 _direction;
        
        private ObjectPooler _pooler;
        
        [Inject]
        private void Construct(ObjectPooler pooler)
        {
            _pooler = pooler;
        }
        
        public void Init(Vector2 direction)
        {
            _spawnTime = Time.time;
            _direction = direction;
            transform.right = direction;
        }
        
        private void Update()
        {
            if (Time.time - _spawnTime > lifetime)
            {
                _pooler.ReturnObject(this);
                return;
            }
            
            // circle cast
            RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 0.1f,
                _direction, speed * Time.deltaTime, collisionMask);
            
            if(hits.Length == 0)
            {
                transform.position += (Vector3)_direction * speed * Time.deltaTime;
            }
            else
            {
                
                if(destroyOnCollision)
                {
                    hits = new RaycastHit2D[] { hits.First() };
                    transform.position = hits.First().point;
                }

                foreach (var hit in hits)
                {
                    var enemy = hit.collider.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        _hitFeedback.PlayFeedbacks(hit.point);
                        enemy.Damage(damage);
                        if (knockback != 0)
                        {
                            enemy.Rigidbody2D.AddForce(Vector2.right*(_direction.x>0?1:-1) * knockback, ForceMode2D.Impulse);
                        }
                    }
                    else
                    {
                        _pooler.ReturnObject(this);
                        return;
                    }
                }
                if(destroyOnCollision)
                {
                    _pooler.ReturnObject(this);
                }
            }
            
        }
    }
}
