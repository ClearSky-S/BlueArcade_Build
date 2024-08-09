using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace BlueArcade
{
    public struct CollisionInfo
    {
        public bool Above, Below, Left, Right;
    }
    public class PlayerController : MonoBehaviour
    {
        public bool allowInput = true; // note. InputActions.Disable() API를 이용해도 되지 않을까?
        public UnityEvent onDeath;
        public Rigidbody2D Rigidbody2D => _rb;
        public bool IsFacingRight => _isFacingRight;
        public CollisionInfo CollisionInfo { get; private set; }
        public float DashCoolTimeRatio => 1 - Mathf.Clamp01((Time.time - _lastDashTime) / 1f);
        
        [SerializeField] private InputActionAsset _inputActionAsset;
        [SerializeField] private Animator _modelAnimator;
        [SerializeField] private Transform _weaponParent;
        [SerializeField] private MMFeedbacks _dashFeedback;
        [SerializeField] private MMFeedbacks _setCharacterFeedback;
        [SerializeField] private AudioSource _voiceSource;
        [SerializeField] private GameObject _deathEffectPrefab;
        
        private const float _raycastDistance = 0.1f;
        private const string _moveAnimBool = "move";
        private const string _jumpAnimBool = "jump";
        
        private IObjectResolver _resolver;
        private StageManager _stageManager;
        private SpecialSkill _specialSkill;
        private Rigidbody2D _rb;
        private BoxCollider2D _collider;
        private Weapon _weapon;
        private float _lastJumpTime = -999f;
        private float _lastJumpPressedTime = -999f;
        private bool _isDashing = false;
        private float _lastDashTime = -999f;
        private bool _isAlive = true;
        private bool _isFacingRight = true;
        
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _dashAction;
        private InputAction _attackAction;
        private InputAction _specialSkillAction;
        
        [Inject]
        public void Construct(IObjectResolver resolver, StageManager stageManager)
        {
            _resolver = resolver;
            _stageManager = stageManager;
            _specialSkill = stageManager.GetComponentInChildren<SpecialSkill>();
            
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            
            _inputActionAsset.Enable();
            
            // Note. 공식 문서에서 FindAction써서 찾는 걸로 나와 있긴 한데 C#클래스 생성해서 직접 참조하는게 더 낫지 않을까 함.
            _moveAction = _inputActionAsset.FindAction("Move");
            _jumpAction = _inputActionAsset.FindAction("Jump");
            _dashAction = _inputActionAsset.FindAction("Dash");
            _attackAction = _inputActionAsset.FindAction("Attack");
            _specialSkillAction = _inputActionAsset.FindAction("SpecialSkill");
        }

        // Update is called once per frame
        void Update()
        {
            UpdateCollisionInfo();
            ProcessInput();
            if (Time.time - _lastJumpTime > 0.1f)
            {
                if(CollisionInfo.Below)
                {
                    _modelAnimator.SetBool(_jumpAnimBool, false);
                }
            }
        }
        
        public void SetCharacter(CharacterGameData data, bool playFeedback = true)
        {
            _modelAnimator.runtimeAnimatorController = data.animatorController;
            SetWeapon(data.weapon);

            if (playFeedback)
            {
                _setCharacterFeedback.PlayFeedbacks();
            }
            
            // 캐릭터 음성 넣어야 하나? 교환 자주 일어나서 좀 애매한 듯
            // _voiceSource.clip = data.battleInVoices[UnityEngine.Random.Range(0, data.battleInVoices.Length)];
            // _voiceSource.Play();
        }
        
        public void SetWeapon(Weapon weapon)
        {
            if (_weapon != null)
            {
                Destroy(_weapon.gameObject);
            }
            _weapon = _resolver.Instantiate(weapon, _weaponParent);
            _weapon.Init(this);
        }

        public void Kill()
        {
            if(!_isAlive)
            {
                return;
            }
            _isAlive = false;
            allowInput = false;
            Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
            onDeath.Invoke();
        }

        void UpdateCollisionInfo()
        {
            var info = new CollisionInfo();
            
            // 바닥 충돌 체크
            // boxcollider 양 끝 지점에서 raycast
            Vector2 raycastOrigin;
            RaycastHit2D hit;
            
            raycastOrigin = new Vector2(_collider.bounds.min.x, _collider.bounds.min.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.down, _raycastDistance, LayerManager.PlatformMask);
            info.Below |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.down * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.max.x, _collider.bounds.min.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.down, _raycastDistance, LayerManager.PlatformMask);
            info.Below |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.down * _raycastDistance, Color.red);
            
            // 천장 충돌 체크
            raycastOrigin = new Vector2(_collider.bounds.min.x, _collider.bounds.max.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.up, _raycastDistance, LayerManager.PlatformMask);
            info.Above |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.up * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.max.x, _collider.bounds.max.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.up, _raycastDistance, LayerManager.PlatformMask);
            info.Above |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.up * _raycastDistance, Color.red);
            
            // 왼쪽 충돌 체크
            raycastOrigin = new Vector2(_collider.bounds.min.x, _collider.bounds.min.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.left, _raycastDistance, LayerManager.PlatformMask);
            info.Left |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.left * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.min.x, _collider.bounds.center.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.left, _raycastDistance, LayerManager.PlatformMask);
            info.Left |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.left * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.min.x, _collider.bounds.max.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.left, _raycastDistance, LayerManager.PlatformMask);
            info.Left |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.left * _raycastDistance, Color.red);
            
            // 오른쪽 충돌 체크
            raycastOrigin = new Vector2(_collider.bounds.max.x, _collider.bounds.min.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.right, _raycastDistance, LayerManager.PlatformMask);
            info.Right |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.right * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.max.x, _collider.bounds.center.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.right, _raycastDistance, LayerManager.PlatformMask);
            info.Right |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.right * _raycastDistance, Color.red);
            
            raycastOrigin = new Vector2(_collider.bounds.max.x, _collider.bounds.max.y);
            hit = Physics2D.Raycast(raycastOrigin, Vector2.right, _raycastDistance, LayerManager.PlatformMask);
            info.Right |= hit.collider != null;
            Debug.DrawRay(raycastOrigin, Vector2.right * _raycastDistance, Color.red);
            
            // Debug.Log("Above: " + info.Above + ", Below: " + info.Below + ", Left: " + info.Left + ", Right: " + info.Right);
            CollisionInfo = info;
        }

        void ProcessInput()
        {
            if (!_isDashing)
            {
                float horizontalInput = _moveAction.ReadValue<float>();
                Move(allowInput?horizontalInput:0);
                if (allowInput)
                {
                    if (_jumpAction.triggered)
                    {
                        _lastJumpPressedTime = Time.time;
                    }
                    if (Time.time - _lastJumpPressedTime < 0.1f)
                    {
                        Jump();
                    }
                    if (_dashAction.triggered)
                    {
                        Dash().Forget();
                    }
                }
            }

            if (allowInput)
            {
                _weapon?.ProcessInput(new WeaponInput
                {
                    Pressed = _attackAction.IsPressed(),
                    PressedThisFrame = _attackAction.triggered,
                });
                if (_specialSkillAction.triggered)
                {
                    _specialSkill.TryUseSpecialSkill().Forget();
                }
            }
        }

        void Move(float horizontalInput)
        {
            if(horizontalInput == 0)
            {
                _modelAnimator.SetBool(_moveAnimBool, false);
            }
            else
            {
                _modelAnimator.SetBool(_moveAnimBool, true);
                // _modelAnimator.gameObject.transform.localScale = new Vector3(horizontalInput>0?1:-1, 1, 1);
                _modelAnimator.gameObject.transform.rotation = Quaternion.Euler(0, horizontalInput>0?0:180, 0);
                _isFacingRight = horizontalInput > 0;
            }
            Vector2 move = new Vector2(horizontalInput * 4f, _rb.velocity.y);
            if( (horizontalInput>0 && CollisionInfo.Right) || (horizontalInput<0 && CollisionInfo.Left))
            {
                _modelAnimator.SetBool(_moveAnimBool, false);
                move = new Vector2(0, _rb.velocity.y);
            }

            _rb.velocity = Vector2.MoveTowards(_rb.velocity, move, 30f * Time.deltaTime);
        }
        
        void Jump()
        {
            if (!CollisionInfo.Below)
            {
                return;
            }
            _rb.velocity = new Vector2(_rb.velocity.x, 9.5f);
            _modelAnimator.SetBool(_jumpAnimBool, true);
            _lastJumpTime = Time.time;
            _lastJumpPressedTime = -999f;
        }

        async UniTaskVoid Dash()
        {
            if(IsFacingRight?CollisionInfo.Right:CollisionInfo.Left)
            {
                return;
            }
            if (Time.time - _lastDashTime < 1f)
            {
                return;
            }
            _lastDashTime = Time.time;
            _dashFeedback.PlayFeedbacks();
            var startTime = Time.time;
            _isDashing = true;
            float startSpeed = 20;
            float endSpeed = 12;
            float dashDuration = 0.2f;
            while(Time.time - startTime < dashDuration)
            {
                if(IsFacingRight?CollisionInfo.Right:CollisionInfo.Left)
                {
                    break;
                }
                float speed = Mathf.Lerp(startSpeed, endSpeed, (Time.time - startTime) / dashDuration);
                _modelAnimator.SetBool(_jumpAnimBool, true);
                _rb.velocity = new Vector2(_isFacingRight ? speed : -speed, 0);
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
            _rb.velocity = new Vector2(0, 0);
            _isDashing = false;
        } 
    }
}
