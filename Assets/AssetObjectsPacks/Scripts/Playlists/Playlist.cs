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
            public Event aoEvent;

            public bool useRandomChoice;
            
            public Channel(CueBehavior cueBehavior) {
                cues = null;
                this.cueBehavior = cueBehavior;
                aoEvent = null;
            }
            public Channel(Event aoEvent) {
                cues = null;
                this.aoEvent = aoEvent;
                cueBehavior = null;
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

        static bool CheckCounts (int channelCount, int playerCount) {
            if (channelCount != playerCount) {
                Debug.LogError("playlist/player coutn mismatch: playlists: " + channelCount + " players: " + playerCount);
                return false;
            }               
            return true;
        }


        public static void InitializePerformance(string debugReason, CueBehavior playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new CueBehavior[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, CueBehavior[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
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

        
        public static void InitializePerformance(string debugReason, Event playlists, EventPlayer players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action onEndPerformanceCallbacks = null) {
            InitializePerformance(debugReason, new Event[] { playlists }, new EventPlayer[] { players }, looped, playerLayer, transforms, forceInterrupt, new Action[] { onEndPerformanceCallbacks });
        }
        public static void InitializePerformance(string debugReason, Event[] playlists, EventPlayer[] players, bool looped, int playerLayer, MiniTransform transforms, bool forceInterrupt = true, Action[] onEndPerformanceCallbacks = null) {
            if (!CheckCounts(playlists.Length, players.Length)) {
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
            if (!CheckCounts(playlists.Length, players.Length)) {
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