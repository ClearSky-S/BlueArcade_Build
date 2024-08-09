using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BlueArcade
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private UIManager _uiManager;
        protected override void Configure(IContainerBuilder builder)
        {
            // DontDestroyOnLoad(gameObject);
            var pooler = gameObject.AddComponent<ObjectPooler>();
            builder.RegisterComponent(pooler);
            builder.RegisterComponentInHierarchy<StageManager>();
            builder.RegisterComponent(_uiManager);
        }
    }
}
