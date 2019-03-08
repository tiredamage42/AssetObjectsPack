using UnityEngine;
namespace AssetObjectsPacks {
    public class PlaylistHolder : MonoBehaviour {
        public Playlist playlist;
        public void PlayPlaylist (EventPlayer[] players, bool looped, System.Action onEndPerformance) {
            playlist.InitializePerformance(players, transform.position, transform.rotation, looped, onEndPerformance);
        }

        //Cue[] playlistCues;
        void OnDrawGizmos () {

            if (playlist) {
                playlist.OnDrawGizmos(transform);
            }
            

        }
    }
}