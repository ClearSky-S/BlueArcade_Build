using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Doozy.Runtime.UIManager.Containers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace BlueArcade
{
    public enum GameState
    {
        Ready,
        Playing,
        GameOver
    }
    public class StageManager : MonoBehaviour
    {
        public GameState GameState => _gameState;
        public PlayerController Player => _player;
        public Transform EnemySpawnPoint => _enemySpawnPoint;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private WeaponBox _weaponBoxPrefab;
        [SerializeField] private Transform _weaponBoxSpawnPoint;
        [SerializeField] private Transform _enemySpawnPoint;
        [SerializeField] private Enemy _defaultEnemyPrefab;
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private PlayerController _playerPrefab;
        [SerializeField] private CharacterGameData _startCharacterData;
        [SerializeField] private CharacterGameData[] _characterDatas;
        private int maxScore {
            get => PlayerPrefs.GetInt("MaxScore", 0);
            set => PlayerPrefs.SetInt("MaxScore", value);
        }
        private CharacterGameData _currentCharacterData;
        private PlayerController _player;
        private int _score;
        private int _lastBoxIndex = -1;
        private GameState _gameState = GameState.Ready;
        
        private IObjectResolver _resolver;
        private ObjectPooler _objectPooler;
        private UIManager _uiManager;
        
        [Inject]
        private void Construct(IObjectResolver resolver, ObjectPooler objectPooler, UIManager uiManager)
        {
            _resolver = resolver;
            _objectPooler = objectPooler;
            _uiManager = uiManager;
            _uiManager.onOpenMenu += () =>
            {
                Time.timeScale = 0;
                if (_player != null)
                {
                    _player.allowInput = false;
                }
            };
            _uiManager.onCloseMenu += () =>
            {
                Time.timeScale = 1;
                if (_player != null)
                {
                    _player.allowInput = true;
                };
            };
        }

        private void Awake()
        {
            var playerGameObject = _resolver.Instantiate(_playerPrefab.gameObject, _playerSpawnPoint.position, Quaternion.identity);
            _player = playerGameObject.GetComponent<PlayerController>();
            _player.allowInput = false;
            _player.onDeath.AddListener(
                    () =>
                    {
                        _gameState = GameState.GameOver;
                        Time.timeScale = 0;
                        maxScore = Mathf.Max(maxScore, _score);
                        _uiManager.GetSimplePopup().Init($"점수 : {_score}\n최고점수 : {maxScore}", () =>
                        {
                            Time.timeScale = 1;
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                        });
                    }
                );
            SetCharacter(_startCharacterData, false);
            SpawnWeaponBox();
        }
        
        // Start is called before the first frame update
        void Start()
        {
            _uiManager.GetSimplePopup().Init("학생부를 수집해서 점수를 획득하세요!", () =>
            {
                _gameState = GameState.Playing;
                InvokeRepeating(nameof(SpawnEnemy), 1f, 2f);
                _player.allowInput = true;
            });
        }

        private void Update()
        {
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _uiManager.OpenSpecialSkillPopup();
            }
            #endif
        }

        public void SetRandomCharacter()
        {
            // _currentCharacterData 제외하기
            var characterDatas = _characterDatas.Where(x => x != _currentCharacterData).ToArray();
            var randomIndex = Random.Range(0, characterDatas.Length);
            var randomCharacterData = characterDatas[randomIndex];
            SetCharacter(randomCharacterData);
        }
        
        public void SpawnWeaponBox()
        {
            // get random spawn point
            var randomIndex = Random.Range(0, _weaponBoxSpawnPoint.childCount);
            if (randomIndex == _lastBoxIndex)
            {
                randomIndex = (randomIndex + 1) % _weaponBoxSpawnPoint.childCount;
            }
            _lastBoxIndex = randomIndex;
            var spawnPoint = _weaponBoxSpawnPoint.GetChild(randomIndex);
            var weaponBox = Instantiate(_weaponBoxPrefab, spawnPoint.position, Quaternion.identity);
            weaponBox.Init(this);
        }
        
        public void AddScore()
        {
            if(_gameState != GameState.Playing) return;
            _score++;
            RenderScore();
        }
        
        private void SetCharacter(CharacterGameData characterData, bool playFeedback = true)
        {
            _currentCharacterData = characterData;
            _player.SetCharacter(_currentCharacterData, playFeedback);
        }
        void SpawnEnemy()
        {
            var enemy = _objectPooler.GetObject<Enemy>(_defaultEnemyPrefab.gameObject);
            enemy.transform.position = _enemySpawnPoint.position;
            enemy.IsFacingRight = Random.Range(0, 2) == 0;
        }

        void RenderScore()
        {
            _scoreText.text = $"{_score}";
        }
    }
}
