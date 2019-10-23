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
                        PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, list.Count, true);
                    }
                    else if (map.Type == "key")
                    {
                        //Maybe have manual cancel button
                        SharedCoroutineStarter.instance.StartCoroutine(FindMap(map.Key, (success, hash) =>
                        {
                            if (success)
                            {
                                PStore val = new PStore
                                {
                                    hash = hash,
                                    key = map.Key,
                                    zip = null
                                };
                                final.Add(val);
                                playlistsLoaded++;
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, list.Count, true);
                            }
                            else
                            {
                                playlistFetchUnsuccessful++;
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, list.Count, true);
                            }

                            if (playlistFetchUnsuccessful + playlistsLoaded == list.Count)
                            {
                                BeatSaverFetch.Invoke(final, playlistsLoaded, playlistFetchUnsuccessful, true);
                                PlaylistStatusProgress.Invoke(playlistsLoaded + playlistFetchUnsuccessful, list.Count, false);
                            }
                        }));
                    }
                }
            }
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
