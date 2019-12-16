using Blister.Types;
using SiaUtil.External;
using System;
using System.Collections;
using UnityEngine.Networking;
using IPA.Utilities;
using UnityEngine;

namespace PlaylistCore
{
    public class BeatSaverInfo
    {
        /* Baron Pants made me do this. If the key-hash valie isn't stored it'll check BeatSaver. */
        public static void TransformPlaylistKeysToHash(Playlist playlist)
        {
            for (int i = 0; i < playlist.Maps.Count; i++)
            {
                Beatmap map = playlist.Maps[i];
                if (map.Type == BeatmapType.Key)
                {
                    if (!Loader.KeyToHashDB.ContainsKey(map.Key.ToString()))
                    {
                        SharedCoroutineStarter.instance.StartCoroutine(FindMapHash(map.Key.ToString(), (success, hash) =>
                        {
                            Logger.log.Info(map.Key + " ::: " + hash);
                            if (success)
                                Loader.KeyToHashDB.Add(map.Key.ToString(), hash);
                            
                        }));
                    }
                }
            }
        }

        /* Put in key, get string back (maybe) */
        public static IEnumerator FindMapHash(string key, Action<bool, string> done)
        {
            using (UnityWebRequest www = UnityWebRequest.Get($"https://beatsaver.com/api/maps/detail/{key}"))
            {
                yield return www.SendWebRequest();

                JSONNode response = JSON.Parse(www.downloadHandler.text);
                if (response["hash"] != null)
                    done?.Invoke(true, response["hash"]);
                else
                    done?.Invoke(false, null);
            }
        }
    }
}
