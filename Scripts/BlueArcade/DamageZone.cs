using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueArcade
{
    public class DamageZone : MonoBehaviour
    {
        private BoxCollider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            BodyAttack();
        }

        void BodyAttack()
        {
            // box cast same as boxcollider 2D
            RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, Vector2.right, 0.1f, LayerManager.PlayerMask);
            if (hit.collider != null)
            {
                hit.collider.GetComponent<PlayerController>().Kill();
            }
        }

    }
}
