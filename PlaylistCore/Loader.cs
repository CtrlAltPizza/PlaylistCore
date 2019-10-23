using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blister;
using Blister.Types;

namespace PlaylistCore
{
    public class Loader
    {
        public static event Action<Dictionary<string, Playlist>> PlaylistsLoadedEvent;
        public static Dictionary<string, Playlist> Playlists = new Dictionary<string, Playlist>();

        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolder(string path)
        {
            Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();
            var filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);
            foreach (var filePath in filePaths)
            {
                var bytes = File.ReadAllBytes(filePath);
                var playlist = PlaylistLib.Deserialize(bytes);
                playlists.Add(filePath, playlist);
            }
            PlaylistsLoadedEvent.Invoke(playlists);
            Playlists = playlists;
            return playlists;
        }
    }
}
