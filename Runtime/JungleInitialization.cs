using UnityEngine;

namespace Jungle
{
    public static class JungleInitialization
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoadCallback()
        {
            var jungleRuntimeGameObject = new GameObject("[Jungle Runtime]");
            jungleRuntimeGameObject.AddComponent<JungleRuntime>();
            Object.DontDestroyOnLoad(jungleRuntimeGameObject);
        }
    }
}
