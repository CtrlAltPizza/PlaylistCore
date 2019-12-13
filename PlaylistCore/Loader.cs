using Blister;
using Blister.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaylistCore
{
    public class Loader
    {
        public static Action<Dictionary<string, Playlist>> PlaylistsLoaded;
        public static Dictionary<string, Playlist> AllPlaylists = new Dictionary<string, Playlist>();

        public static Dictionary<string, Playlist> LoadAllPlaylistsFromFolder(string path)
        {
            Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();
            Directory.CreateDirectory(path);
            string[] filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);

            foreach (var fPath in filePaths)
            {
                try
                {
                    Playlist plist = PlaylistLib.Deserialize(File.ReadAllBytes(fPath));
                    playlists.Add(fPath, plist);
                    AllPlaylists.Add(fPath, plist);
                }
                catch
                {
                    Logger.log.Error("Error loading playlist at: " + fPath);
                }
            }
            PlaylistsLoaded?.Invoke(playlists);
            return playlists;
        }
    }
}
