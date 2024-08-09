using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BlueArcade
{
    public class GravityController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private float _defaultGravityScale;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _defaultGravityScale = _rb.gravityScale ;
        }
        
        private void FixedUpdate()
        {
            if(_rb.velocity.y < 0)
            {
                _rb.gravityScale = _defaultGravityScale;
            }
            else
            {
                if (Input.GetKey(KeyCode.C))
                {
                    _rb.gravityScale = _defaultGravityScale* 0.625f;
                }
                else
                {
                    _rb.gravityScale = _defaultGravityScale;
                }
            }
        }
    }
}
