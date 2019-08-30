using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;



namespace AssetObjectsPacks {
    public static class Playlist {

        
        //using this top level system enables messaging on the root cue
        //but now syncing only works for the root cue, not just first layer or others

        [System.Serializable] public class Channel {
            public Cue[] cues;
            public CueBehavior cueBehavior;
            public Event[] aoEvents;

            public bool useRandomChoice;
            
            public Channel(Event[] aoEvents) {
                cues = null;
                this.aoEvents = aoEvents;
                cueBehavior = null;
            }
            
            public Channel(CueBehavior cueBehavior) {
                cues = null;
                this.cueBehavior = cueBehavior;
                aoEvents = null;
            }

            public Channel(Cue parentCue, bool topLevel) {
                if (topLevel) {
                    cues = new Cue[] { parentCue };
                    return;
                }
                
                this.useRandomChoice = parentCue.useRandomPlaylist;
                Transform t = parentCue.transform;
                int c = t.childCount;
                if (c != 0) { 
                    cues = c.Generate( i => t.GetChild(i).GetComponent<Cue>() ).Where( cue => cue != null).ToArray();
                }
                if (c == 0 || cues.Length == 0) {
                    cues = new Cue[] { parentCue };
                }
            }
        }

        static bool CheckChannelCounts (int channelCount, int playerCount) {
            if (channelCount != playerCount) {
                Debug.LogError("playlist/player coutn mismatch: playlists: " + channelCount + " players: " + playerCount);
                return false;
            }               
            return true;
        }


        //event plays cant be multi channel...
        public static void InitializePerformance(string debugReason, Event playEvent, EventPlayer player, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Event[] { playEvent }, player, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Event[] events, EventPlayer player, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
        
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                debugReason,
                playerLayer,
                new Channel[] { new Channel( events ) },
                new EventPlayer[] { player },
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

        public static void InitializePerformance(string debugReason, CueBehavior playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new CueBehavior[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, CueBehavior[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckChannelCounts(playlists.Length, players.Length)) {
                return;
            }
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                
                debugReason,
                playerLayer,
                playlists.Generate( p => new Channel(p)).ToArray(),
                players, 
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

        

        public static void InitializePerformance(string debugReason, Cue playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Cue[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Cue[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckChannelCounts(playlists.Length, players.Length)) {
                return;
            }
            Playlists.Performance.playlistPerformances.GetNewPerformance().InitializePerformance (
                true, 
                debugReason,
                playerLayer, 
                playlists.Generate( p => new Channel(p, true)).ToArray(), 
                players, 
                transforms,
                looped, 
                forceInterrupt, 
                onEndPerformanceCallbacks
            );
        }

    }
}