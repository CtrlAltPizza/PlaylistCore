using Blister.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class CustomPlaylistSO : PlaylistSO
    {
        /*public string playlistName { get; private set; } = "";

        public Sprite coverImage { get; private set; }

        public IBeatmapLevelCollection beatmapLevelCollection { get; private set; }
        */
        public void Initialize(Playlist playlist)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(playlist.Cover);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

            _playListLocalizedName = playlist.Title;
            _coverImage = sprite;

            foreach (var song in playlist.Maps)
            {
                var a = SongCore.Loader.CustomLevels.Values.Where(x => x.levelID.Replace("custom_level_", "") == song.Hash).ToList();
                if (a.Count() != 0)
                {
                    var songLevel = a.FirstOrDefault();
                    lvls.Add(songLevel);
                }
            }

            _beatmapLevelCollection;
        }


    }
}
