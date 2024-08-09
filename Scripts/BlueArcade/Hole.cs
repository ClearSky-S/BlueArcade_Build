using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace BlueArcade
{
    public class Hole : MonoBehaviour
    {
        private BoxCollider2D _collider;
        private StageManager _stageManager;
        
        [Inject]
        public void Construct(StageManager stageManager)
        {
            _stageManager = stageManager;
        }

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            BoxCast();
        }

        void BoxCast()
        {
            // box cast same as boxcollider 2D
            RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, Vector2.right,
                0.1f, LayerManager.PlayerMask | LayerManager.EnemyMask);
            if (hit.collider != null)
            {
                hit.collider.gameObject.transform.position = _stageManager.EnemySpawnPoint.position;
                if (hit.collider.TryGetComponent(out Enemy enemy))
                {
                    enemy.SetState(Enemy.EnemyState.Angry);
                }
            }
        }
    }
}
