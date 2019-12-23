using Blister;
using Blister.Conversion;
using Blister.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PlaylistCore
{
    public class Loader
    {
        internal static bool isDirty = false;

        public static Action<Dictionary<string, Playlist>> PlaylistsLoaded;
        public static Dictionary<string, Playlist> AllPlaylists = new Dictionary<string, Playlist>();
        public static Dictionary<string, string> KeyToHashDB = new Dictionary<string, string>();

        public static async Task LoadAllPlaylistsFromFolder(string path)
        {
            await LoadAllPlaylistsFromFolders(new string[] { path });
        }

        public static async Task<Playlist> AddPlaylist(string file)
        {
            if (!file.EndsWith("blist"))
                return null;
            byte[] result;
            using (FileStream p = File.Open(file, FileMode.Open))
            {
                result = new byte[p.Length];
                await p.ReadAsync(result, 0, (int)p.Length);
            }
            Playlist plist = PlaylistLib.Deserialize(result);
            await AddPlaylistToLC(plist);
            return plist;
        }

        public static async Task<Playlist> AddLegacyPlaylist(string file)
        {
            byte[] result;
            using (FileStream p = File.Open(file, FileMode.Open))
            {
                result = new byte[p.Length];
                await p.ReadAsync(result, 0, (int)p.Length);
            }
            var leg = PlaylistConverter.DeserializeLegacyPlaylist(result);
            var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);
            await AddPlaylistToLC(converted);
            return converted;
        }

        private static bool isReLoading = false;
        public static async Task<Dictionary<string, Playlist>> ReloadAllPlaylists()
        {
            if (!isReLoading)
            {
                isReLoading = true;
                Logger.log.Info("LOADER ::: Reloading all active playlists.");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var store = new Dictionary<string, Playlist>();
                foreach (var p in AllPlaylists)
                {
                    if (p.Key.EndsWith("blist"))
                        store.Add(p.Key, await AddPlaylist(p.Key));
                    else
                        store.Add(p.Key, await AddLegacyPlaylist(p.Key));
                }
                PlaylistCore.instance.LoadedPlaylistSO.Clear();
                foreach (var pl in store)
                {
                    var n = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                    n.playlist = pl.Value;
                    await n.SetupCover();
                    PlaylistCore.instance.LoadedPlaylistSO.Add(n);
                }
                isDirty = true;
                AllPlaylists.Clear();

                AllPlaylists = store;
                PlaylistsLoaded?.Invoke(store);

                sw.Stop();
                new FancyRadialMessage().SetupAndDisplayThenHide("Loaded " + AllPlaylists.Count + " playlists.", new Vector3(0f, 3.5f, 2.7f), Quaternion.Euler(-15f, 0f, 0f));
                PlaylistCore.instance.Loader_OnLevelPacksRefreshed();
                Logger.log.Info("LOADER ::: Finished reloading " + store.Count + " playlists. Took: " + sw.Elapsed.Seconds + "." + sw.Elapsed.Milliseconds / 10 + " seconds.");
                isReLoading = false;
                return AllPlaylists;
            }
            else return null;
        }

        private static bool isLoading = false;
        public static async Task<Dictionary<string, Playlist>> LoadAllPlaylistsFromFolders(string[] paths)
        {
            if (!isLoading)
            {
                isLoading = true;
                Logger.log.Info("LOADER ::: Loading playlists.");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dictionary<string, Playlist> playlists = new Dictionary<string, Playlist>();
                foreach (var path in paths)
                {
                    Directory.CreateDirectory(path);
                    string[] filePaths = Directory.GetFiles(path, "*.blist", SearchOption.AllDirectories);

                    foreach (var fPath in filePaths)
                    {
                        try
                        {
                            byte[] result;
                            using (FileStream p = File.Open(fPath, FileMode.Open))
                            {
                                result = new byte[p.Length];
                                await p.ReadAsync(result, 0, (int)p.Length);
                            }
                            Playlist plist = PlaylistLib.Deserialize(result);
                            playlists.Add(fPath, plist);
                            if (!AllPlaylists.ContainsKey(fPath))
                                AllPlaylists.Add(fPath, plist);
                        }
                        catch
                        {
                            Logger.log.Error("Error loading playlist at: " + fPath);
                        }
                    }

                    //Legacy Conversion (bplist)
                    var legacyPathsBP = Directory.GetFiles(path, "*.bplist", SearchOption.AllDirectories);
                    foreach (var filePath in legacyPathsBP)
                    {
                        try
                        {
                            byte[] result;
                            using (FileStream p = File.Open(filePath, FileMode.Open))
                            {
                                result = new byte[p.Length];
                                await p.ReadAsync(result, 0, (int)p.Length);
                            }
                            var leg = PlaylistConverter.DeserializeLegacyPlaylist(result);
                            var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);
                            /*
                            var fileName = Path.GetFileNameWithoutExtension(filePath);
                            var newFilePath = path + fileName + "_CFB.blist";
                            OverwritePlaylist(filePath, converted);
                            playlists.Add(newFilePath, converted);
                            if (!AllPlaylists.ContainsKey(newFilePath))
                                AllPlaylists.Add(newFilePath, converted);
                            File.Move(filePath, newFilePath);
                            */
                            if (!AllPlaylists.ContainsKey(filePath))
                                AllPlaylists.Add(filePath, converted);
                            playlists.Add(filePath, converted);
                            Logger.log.Debug("Converted Playlist " + converted.Title);
                        }
                        catch (Exception e)
                        {
                            string st = "";
                            if (e.GetType() == typeof(InvalidBase64Exception))
                                st += "Invalid Image: " + e.Message;
                            else if (e.GetType() == typeof(InvalidMapHashException))
                                st += "Invalid Hash <<<" + (e as InvalidMapHashException).Hash + ">>>";
                            else if (e.GetType() == typeof(InvalidMapKeyException))
                                st += "Invalid Key <<<" + (e as InvalidMapKeyException).Key + ">>>";
                            else
                                st += "Other: " + e.Message;
                            Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath) + ". Why? " + st);
                        }

                    }

                    //Legacy Conversion (json)
                    var legacyPathsJSON = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);
                    foreach (var filePath in legacyPathsJSON)
                    {
                        if (!filePath.Contains("SongBrowserPluginFavorites") && !filePath.Contains("favorites.json"))
                        {
                            try
                            {
                                byte[] result;
                                using (FileStream p = File.Open(filePath, FileMode.Open))
                                {
                                    result = new byte[p.Length];
                                    await p.ReadAsync(result, 0, (int)p.Length);
                                }
                                var leg = PlaylistConverter.DeserializeLegacyPlaylist(result);
                                var converted = PlaylistConverter.ConvertLegacyPlaylist(leg);
                                /*
                                var fileName = Path.GetFileNameWithoutExtension(filePath);
                                var newFilePath = path + fileName + "_CFJ.blist";
                                OverwritePlaylist(filePath, converted);
                                playlists.Add(newFilePath, converted);
                                if (!AllPlaylists.ContainsKey(newFilePath))
                                    AllPlaylists.Add(newFilePath, converted);
                                File.Move(filePath, newFilePath);
                                */
                                if (!AllPlaylists.ContainsKey(filePath))
                                    AllPlaylists.Add(filePath, converted);
                                playlists.Add(filePath, converted);
                                Logger.log.Debug("Converted Playlist " + converted.Title);
                            }
                            catch (Exception e)
                            {
                                string st = "";
                                if (e.GetType() == typeof(InvalidBase64Exception))
                                    st += "Invalid Image: " + e.Message;
                                else if (e.GetType() == typeof(InvalidMapHashException))
                                    st += "Invalid Hash <<<" + (e as InvalidMapHashException).Hash + ">>>";
                                else if (e.GetType() == typeof(InvalidMapKeyException))
                                    st += "Invalid Key <<<" + (e as InvalidMapKeyException).Key + ">>>";
                                else
                                    st += "Other: " + e.Message;
                                Logger.log.Error("Error converting playlist: " + Path.GetFileName(filePath) + ". Why? " + st);
                            }
                        }
                    }
                }

                foreach (var pl in playlists)
                {
                    var n = ScriptableObject.CreateInstance<CustomPlaylistSO>();
                    n.playlist = pl.Value;
                    await n.SetupCover();
                    PlaylistCore.instance.LoadedPlaylistSO.Add(n);
                }
                sw.Stop();

                PlaylistsLoaded?.Invoke(playlists);
                Logger.log.Info("LOADER ::: Finished loading " + playlists.Count + " playlists. Took: " + sw.Elapsed.Seconds + "." + sw.Elapsed.Milliseconds / 10 + " seconds.");
                isLoading = false;
                return playlists;
            }
            else return null;
        }

        private static async Task AddPlaylistToLC(Playlist list)
        {
            var a = ScriptableObject.CreateInstance<CustomPlaylistSO>();
            a.playlist = list;
            await a.SetupCover();
            PlaylistCore.instance.LoadedPlaylistSO.Add(a);
        }

        public static void UnregisterPlaylist(CustomPlaylistSO list)
        {
            var so = PlaylistCore.instance.LoadedPlaylistSO.Find(x => x = list);
            if (so != null)
            {
                var playlist = AllPlaylists.Where(x => x.Value == so.playlist);
                if (playlist.Count() > 0)
                    AllPlaylists.Remove(playlist.First().Key);
                PlaylistCore.instance.LoadedPlaylistSO.Remove(so);
            }
            isDirty = true;
        }

        public static void UnregisterPlaylist(Playlist list)
        {
            var so = PlaylistCore.instance.LoadedPlaylistSO.Find(x => x.playlist == list);
            if (so != null)
            {
                var playlist = AllPlaylists.Where(x => x.Value == so.playlist);
                if (playlist.Count() > 0)
                    AllPlaylists.Remove(playlist.First().Key);
                PlaylistCore.instance.LoadedPlaylistSO.Remove(so);
            }
            isDirty = true;
        }

        public static void UnregisterPlaylist(string fileLocation)
        {
            if (AllPlaylists.TryGetValue(fileLocation, out Playlist f))
                UnregisterPlaylist(f);
        }

        /* Overwrites a legacy playlist with Blister playlist. Does not change file name. */
        public static void OverwritePlaylist(string path, Playlist playlist)
        {
            var bytes = PlaylistLib.Serialize(playlist);
            File.WriteAllBytes(path, bytes);
        }
    }
}
