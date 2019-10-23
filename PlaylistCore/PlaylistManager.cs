using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Blister.Types;
using IPA.Utilities;
using SongCore.OverrideClasses;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlaylistCore
{
    public class PlaylistManager : PersistentSingleton<PlaylistManager>
    {
        bool playlistsFullyLoaded = false;
        bool greenlight = false;
        bool firstLoad = true;
        public override void OnEnable()
        {
            Loader.PlaylistsLoadedEvent += Loader_PlaylistsLoadedEvent;
            BeatSaverAPI.BeatSaverFetch += BeatSaverAPI_BeatSaverFetch;
            BeatSaverAPI.PlaylistStatusProgress += BeatSaverAPI_PlaylistStatusProgress;
            SongCore.Loader.SongsLoadedEvent += Loader_SongsLoadedEvent;
        }

        private void Loader_SongsLoadedEvent(SongCore.Loader arg1, Dictionary<string, CustomPreviewBeatmapLevel> arg2)
        {
            StartCoroutine(Greenlight());
            StartCoroutine(WaitForLoad());
        }
        
        private IEnumerator WaitForLoad()
        {
            if (greenlight)
            {
                SongCoreBeatmapLevelPackCollectionSO customBeatmapLevelPackCollectionSO = SongCore.Loader.CustomBeatmapLevelPackCollectionSO;
                
                if (firstLoad)
                {
                    foreach (var playlist in Loader.Playlists.Values)
                    {
                        var pso = PlaylistLevelPackSO.CreatePackFromPlaylist(playlist);
                        customBeatmapLevelPackCollectionSO.AddLevelPack(pso);
                    }
                    firstLoad = false;
                }
                else
                {
                    List<CustomBeatmapLevelPack> _customBeatmapLevelPacks = customBeatmapLevelPackCollectionSO.GetPrivateField<List<CustomBeatmapLevelPack>>("_customBeatmapLevelPacks");
                    List<IBeatmapLevelPack> _allBeatmapLevelPacks = customBeatmapLevelPackCollectionSO.GetPrivateField<IBeatmapLevelPack[]>("_allBeatmapLevelPacks").ToList();

                    _customBeatmapLevelPacks.RemoveAll(x => x.packID.StartsWith("Sialist_"));
                    _allBeatmapLevelPacks.RemoveAll(x => x.packID.StartsWith("Sialist_"));

                    customBeatmapLevelPackCollectionSO.SetPrivateField("_customBeatmapLevelPacks", _customBeatmapLevelPacks);
                    customBeatmapLevelPackCollectionSO.SetPrivateField("_allBeatmapLevelPacks", _allBeatmapLevelPacks.ToArray());

                    foreach (var playlist in Loader.Playlists.Values)
                    {
                        var pso = PlaylistLevelPackSO.CreatePackFromPlaylist(playlist);
                        customBeatmapLevelPackCollectionSO.AddLevelPack(pso);
                    }
                }

                StopAllCoroutines();
            }
            else
            {
                yield return new WaitForSecondsRealtime(.5f);
                StartCoroutine(WaitForLoad());
            }
        }

        public void Initialize()
        {
            Loader.LoadAllPlaylistsFromFolder(BeatSaber.InstallPath.Replace('\\', '/') + "/Playlists/");
        }

        private void Loader_PlaylistsLoadedEvent(Dictionary<string, Playlist> playlistDict)
        {
            BeatSaverAPI.lolPants(playlistDict.Values.ToList());
        }

        private void BeatSaverAPI_BeatSaverFetch(List<PStore> maps, int successful, int unsuccessful, bool aborted)
        {
            Logger.log.Info($"Song Hash Data Completed. Summary: {successful} fetched. {unsuccessful} failed.");
            Dictionary<string, Playlist> newPlaylists = new Dictionary<string, Playlist>();
            foreach (var playlist in Loader.Playlists)
            {
                var cplay = playlist.Value.Maps.ToArray();
                for (int i = 0; i < cplay.Length; i++)
                {
                    foreach (var mapy in maps)
                    {
                        if (cplay[i].Key != null && cplay[i].Key == mapy.key)
                        {
                            cplay[i].Key = "";
                            cplay[i].Hash = mapy.hash;
                            cplay[i].Type = "hash";
                        }
                    }
                }
                Playlist newplaylist = new Playlist
                {
                    Author = playlist.Value.Author,
                    Cover = playlist.Value.Cover,
                    Description = playlist.Value.Description,
                    Maps = cplay.ToList(),
                    Title = playlist.Value.Title
                };
                newPlaylists.Add(playlist.Key, newplaylist);
                Loader.OverwritePlaylist(playlist.Key, newplaylist);
            }
            Loader.Playlists = newPlaylists;
            playlistsFullyLoaded = true;
        }

        private void BeatSaverAPI_PlaylistStatusProgress(int soFar, int total, bool downloading)
        {
            //if (downloading)
            //    Logger.log.Info($"Song Hash Data Gathered: {soFar} / {total}.");
        }

        private IEnumerator Greenlight()
        {
            yield return new WaitForSecondsRealtime(1f);
            if (playlistsFullyLoaded && SongCore.Loader.AreSongsLoaded)
                greenlight = true;
            else
                greenlight = false;
        }

        private void OnDisable()
        {
            Loader.PlaylistsLoadedEvent -= Loader_PlaylistsLoadedEvent;
            BeatSaverAPI.BeatSaverFetch -= BeatSaverAPI_BeatSaverFetch;
            BeatSaverAPI.PlaylistStatusProgress -= BeatSaverAPI_PlaylistStatusProgress;
        }
    }
}
