using Blister.Types;
//using PlaylistCore.HarmonyPatches;
using SongCore.OverrideClasses;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class CustomPlaylistSO : ScriptableObject, IPlaylist, IAnnotatedBeatmapLevelCollection
    {
        public bool isDirty = true;
        public Playlist playlist;
        public string collectionName { get; protected internal set; } = "";

        public Sprite coverImage { get; protected internal set; }

        public IBeatmapLevelCollection beatmapLevelCollection { get; internal set; }
        public async Task SetupCover()
        {
            void a()
            {
                HMMainThreadDispatcher.instance.Enqueue(delegate
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(playlist.Cover);
                    coverImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                });
            }
            await Task.Run(a);
        }
        public void SetupFromPlaylist(Playlist playlist, IBeatmapLevelPackCollection coll, BeatmapLevelsModel model = null)
        {
            collectionName = playlist.Title;


            HashSet<string> vs = new HashSet<string>();
            for (int i = 0; i < playlist.Maps.Count; i++)
            {
                var map = playlist.Maps[i];
                if (map.Type == BeatmapType.LevelID && !vs.Contains(map.LevelID))
                    vs.Add(map.LevelID);
                else if (map.Type == BeatmapType.Hash && !vs.Contains("custom_level_" + IPA.Utilities.Utils.ByteArrayToString(map.Hash).ToUpper()))
                    vs.Add("custom_level_" + IPA.Utilities.Utils.ByteArrayToString(map.Hash).ToUpper());
                else if (map.Type == BeatmapType.Key && Loader.KeyToHashDB.TryGetValue(map.Key.ToString(), out string hash) && !vs.Contains("custom_level_" + hash.ToUpper()))
                    vs.Add("custom_level_" + hash.ToUpper());
            }
            BeatmapLevelFilterModel.LevelFilterParams levelFilterParams = BeatmapLevelFilterModel.LevelFilterParams.ByBeatmapLevelIds(vs);
            var filtered = BeatmapLevelFilterModel.FilerBeatmapLevelPackCollection(coll, levelFilterParams).beatmapLevels;
            BeatmapLevelCollection collection = new BeatmapLevelCollection(filtered.ToArray());
            beatmapLevelCollection = collection;

            

            

            isDirty = false;
        }

        public void ReloadPerhaps()
        {
            //if (LevelFilteringNavigationController_InitPlaylists.allSongs != null)
            //    SetupFromPlaylist(playlist, LevelFilteringNavigationController_InitPlaylists.allSongs);
        }
    }
}
