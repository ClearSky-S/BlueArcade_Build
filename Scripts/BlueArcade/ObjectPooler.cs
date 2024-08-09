using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace BlueArcade
{
    public class PoolableObject : MonoBehaviour
    {
        public GameObject Prefab { get; set; }
        public virtual void OnReturnPool()
        {
            
        }
    }
    public class ObjectPooler : MonoBehaviour
    {
        
        private Dictionary<GameObject, Stack<PoolableObject>> _poolMap = new ();
        private IObjectResolver _resolver;
        private Transform _poolParent;
        [Inject]
        private void Construct(IObjectResolver resolver)
        {
            _resolver = resolver;
            _poolParent = new GameObject("PoolParent").transform;
            _poolParent.SetParent(transform);
        }
        
        public T GetObject<T>(GameObject prefab) where T : PoolableObject
        {
            if (!_poolMap.ContainsKey(prefab))
            {
                _poolMap[prefab] = new Stack<PoolableObject>();
            }

            T obj;
            
            if (_poolMap[prefab].Count > 0)
            {
                obj = _poolMap[prefab].Pop() as T;
            }
            else
            {
                obj = _resolver.Instantiate(prefab).GetComponent<T>();
                obj.Prefab = prefab;
            }
            obj.gameObject.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            
            if(obj.gameObject.scene != SceneManager.GetActiveScene())
            {
                SceneManager.MoveGameObjectToScene(obj.gameObject, SceneManager.GetActiveScene());
            }
                
            // GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            // foreach (GameObject rootObject in rootObjects)
            // {
            //     Debug.Log(rootObject.name);
            // }
            return obj;
        }
        
        public void ReturnObject(PoolableObject obj)
        {
            if(!_poolMap.ContainsKey(obj.Prefab))
            {
                _poolMap[obj.Prefab] = new Stack<PoolableObject>();
            }
            obj.OnReturnPool();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_poolParent);
            _poolMap[obj.Prefab].Push(obj);
        }
    }
}
