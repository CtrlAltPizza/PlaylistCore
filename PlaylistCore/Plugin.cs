using Harmony;
using IPA;
using IPA.Config;
using IPA.Utilities;
using System.Reflection;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace PlaylistCore
{
    public class Plugin : IBeatSaberPlugin, IDisablablePlugin
    {
        private static HarmonyInstance harmony;
        public void OnEnable()
        {
            if (harmony == null)
                harmony = HarmonyInstance.Create("com.auros.BeatSaber.PlaylistCore");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            PersistentSingleton<PlaylistManager>.instance.Initialize();
        }

        public void Init(IPALogger logger)
        {
            Logger.log = logger;
        }

        public void OnApplicationStart()
        {
            
        }

        public void OnApplicationQuit()
        {
            
        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnDisable()
        {
            if (harmony != null)
                harmony.UnpatchAll();
        }
    }
}
