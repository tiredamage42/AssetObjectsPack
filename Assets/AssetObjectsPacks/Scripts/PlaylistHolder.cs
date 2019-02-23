using UnityEngine;
namespace AssetObjectsPacks {
    public class PlaylistHolder : MonoBehaviour {
        public Playlist playlist;
        public void PlayPlaylist (EventPlayer[] players, System.Action onEndPerformance) {
            playlist.InitializePerformance(players, transform.position, transform.rotation, onEndPerformance);
        }
    }
}