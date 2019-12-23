using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IPA;
using IPA.Config;
using IPA.Utilities;
using Harmony;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

namespace PlaylistCore
{
    public class Plugin : IBeatSaberPlugin, IDisablablePlugin
    {
        public const string HarmonyId = "com.auros.PlaylistCore";
        internal static HarmonyInstance harmony;
        internal static string Name => "PlaylistCore";
        internal static Ref<PluginConfig> config;
        internal static IConfigProvider configProvider;

        public void Init(IPALogger logger, [Config.Prefer("json")] IConfigProvider cfgProvider)
        {
            Logger.log = logger;
            configProvider = cfgProvider;
            config = configProvider.MakeLink<PluginConfig>((p, v) =>
            {
                if (v.Value == null || v.Value.RegenerateConfig)
                {
                    Logger.log.Debug("Regenerating PluginConfig");
                    p.Store(v.Value = new PluginConfig()
                    {
                        // Set your default settings here.
                        RegenerateConfig = false,
                        KeyToHashDB = new Dictionary<string, string>()
                    }); ;
                }
                config = v;
            });
            harmony = HarmonyInstance.Create(HarmonyId);
        }

        public void OnEnable()
        {
            PlaylistCore.instance.LoadBlisters();
            ApplyHarmonyPatches();
        }

        public void OnDisable()
        {
            RemoveHarmonyPatches();
        }

        public static void ApplyHarmonyPatches()
        {
            try
            {
                Logger.log.Debug("Applying Harmony patches.");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error applying Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        public static void RemoveHarmonyPatches()
        {
            try
            {
                // Removes all patches with this HarmonyId
                harmony.UnpatchAll(HarmonyId);
            }
            catch (Exception ex)
            {
                Logger.log.Critical("Error removing Harmony patches: " + ex.Message);
                Logger.log.Debug(ex);
            }
        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }


        public void OnApplicationQuit()
        {
            config.Value.KeyToHashDB = Loader.KeyToHashDB;
            configProvider.Store(config.Value);
            Logger.log.Debug("OnApplicationQuit");

        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.L))
                Loader.ReloadAllPlaylists();
            if (Input.GetKeyDown(KeyCode.Y))
                Loader.UnregisterPlaylist(PlaylistCore.instance.LoadedPlaylistSO.First());
            
        }


        public void OnSceneUnloaded(Scene scene)
        {

        }

        public void OnApplicationStart()
        { }
    }
}
