using Blister.Types;
using SiaUtil.External;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace PlaylistCore
{
    public class BeatSaverAPI
    {
        public static Action<int, int, bool> PlaylistStatusProgress;
        public static Action<List<PStore>, int, int, bool> BeatSaverFetch;
        public static void lolPants(List<Playlist> list)
        {
            int playlistsLoaded = 0;
            int playlistFetchUnsuccessful = 0;
            List<PStore> final = new List<PStore>();

            int finalCount = 0;
            foreach (var li in list)
            {
                foreach (var st in li.Maps)
                    finalCount++;
            }

            foreach (Playlist playlist in list)
            {
                foreach (Beatmap map in playlist.Maps)
                {
                    if (map.Type == "hash")
                    {
                        PStore val = new PStore
                        {
                            hash = map.Hash,
                            key = null,
                            zip = null
                        };
                        final.Add(val);
                        playlistsLoaded++;
                        PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, finalCount, true);
                    }
                    else if (map.Type == "key")
                    {
                        string theKey;
                        if (map.Key.Contains("-"))
                            theKey = ConvertOldHashToNew(map.Key);
                        else
                            theKey = map.Key;
                        //Maybe have manual cancel button
                        SharedCoroutineStarter.instance.StartCoroutine(FindMap(theKey, (success, hash) =>
                        {
                            if (success)
                            {
                                PStore val = new PStore
                                {
                                    hash = hash,
                                    key = theKey,
                                    zip = null
                                };
                                final.Add(val);
                                playlistsLoaded++;
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, finalCount, true);
                            }
                            else
                            {
                                playlistFetchUnsuccessful++;
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, finalCount, true);
                            }

                            if (playlistFetchUnsuccessful + playlistsLoaded == finalCount)
                            {
                                BeatSaverFetch.Invoke(final, playlistsLoaded, playlistFetchUnsuccessful, true);
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, finalCount, false);
                            }
                        }));
                    }
                }
            }
        }

        private static string ConvertOldHashToNew(string oldKey)
        {
            string lastNumber = oldKey.Substring(oldKey.LastIndexOf('-') + 1);
            return int.Parse(lastNumber).ToString("x");
        }

        public static IEnumerator FindMap(string key, Action<bool, string> done)
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

    public struct PStore
    {
        public string key;
        public string hash;
        public byte[] zip;
    }
}
