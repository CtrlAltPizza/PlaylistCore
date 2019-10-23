using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blister.Types;

namespace PlaylistCore
{
    public class PlaylistManager : PersistentSingleton<PlaylistManager>
    {
        public override void OnEnable()
        {
            Loader.PlaylistsLoadedEvent += Loader_PlaylistsLoadedEvent;
        }

        private void Loader_PlaylistsLoadedEvent(Dictionary<string, Playlist> playlistDict)
        {
            
        }

        private void OnDisable()
        {

        }

        public void Initialize()
        {

        }
    }
}
