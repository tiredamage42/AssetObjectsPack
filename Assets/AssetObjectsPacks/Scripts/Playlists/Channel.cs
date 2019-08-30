using System.Collections.Generic;
using UnityEngine;

namespace AssetObjectsPacks.Playlists {

    public class PerformanceChannel {
        public EventPlayer player;
        Playlist.Channel playlistChannel;

        public bool isActive;
        int cueIndex, curCueRepeats;

        // like the tape head on a deck, or needle on a record player...
        public PerformanceHead head = new PerformanceHead();


        Cue GetCurrentCue () {
            Cue cue = null;
            // playing physical cue
            if (playlistChannel.cues != null) {
                cue = playlistChannel.cues[cueIndex];
            }
            return cue;
        }
        CueBehavior GetCurrentCueBehavior () {
            CueBehavior cueBehavior = null;
        
            // playing physical cue
            if (playlistChannel.cues != null) {
                cueBehavior = playlistChannel.cues[cueIndex].behavior;
            }
            // just playing cue behavior
            else if (playlistChannel.cueBehavior != null) {
                cueBehavior = playlistChannel.cueBehavior;
            }
            return cueBehavior;
        }

        Event[] GetCurrentPlayingEvents () {
            Event[] aoEvents = null;
            if (playlistChannel.cues == null && playlistChannel.cueBehavior == null) { 
                aoEvents = playlistChannel.aoEvents;
            }
            return aoEvents;

        }

        public void OnPerformanceEnd (Performance performance, bool force) {
            if (!performance.looped || force) {
                if (player) {
                    player.currentPlaylists.Remove(performance);
                    player = null;
                }
            }
            isActive = false;
        }
        public void PlayCue (string debugReason, bool asInterrupter) {
            head.OnPlay(debugReason, asInterrupter, GetCurrentCue(), GetCurrentCueBehavior(), GetCurrentPlayingEvents());   
        }

        public void InitializeChannel (bool isTopLevel, int layer, EventPlayer player, Playlist.Channel playlistChannel, MiniTransform suppliedTransform, Performance performance) {
            this.playlistChannel = playlistChannel;
            this.player = player;

            //only if it has subcues
            if (playlistChannel.useRandomChoice) {
                List<int> enabledIndicies = new List<int>();
                for (int i = 0; i < playlistChannel.cues.Length; i++) {
                    if (playlistChannel.cues[i].gameObject.activeSelf) {
                        enabledIndicies.Add(i);
                    }
                    i++;
                }
                cueIndex = enabledIndicies.RandomChoice();
            }
            else {
                cueIndex = 0;   
            }

            if (!isTopLevel) {
                if (SkipAheadPastDisabledCues(performance)) {
                    return;
                }
            }
            curCueRepeats = 0;
            isActive = true;
            head.OnStart(isTopLevel, playlistChannel, GetCurrentCue(), GetCurrentCueBehavior(), GetCurrentPlayingEvents(), layer, player, suppliedTransform);

        }   
        public void UpdateChannel () {
            if (!isActive) return;   
            head.UpdateHead(GetCurrentCueBehavior());
        }

        // returns true if we're out of cues
        bool SkipAheadPastDisabledCues (Performance performance) {

            int l = playlistChannel.cues.Length;
            while (cueIndex < l && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {                    
                cueIndex ++;
            }
            if (cueIndex >= l) {
                OnPerformanceEnd(performance, false);
                return true;
            }
            return false;
        }

    
        public void OnCueEnd (int channelIndex, bool isTopLevel, int layer, Performance performance, MiniTransform suppliedTransform)
        {
            if (playlistChannel.cues == null) {
                OnPerformanceEnd(performance, false);
                return;
            }

            curCueRepeats++;

            Cue cueAtIndex = playlistChannel.cues[cueIndex];
            
            // repeat again
            if (curCueRepeats < cueAtIndex.repeats) {
                head.OnStart(isTopLevel, playlistChannel, cueAtIndex, cueAtIndex.behavior, null, layer, player, suppliedTransform);
                return;
            }

            //only playing one random one, and we're done repeating it, so end
            if (playlistChannel.useRandomChoice) {
                OnPerformanceEnd(performance, false);
                return;
            }
            
            curCueRepeats = 0;
            cueIndex++;

            if (SkipAheadPastDisabledCues(performance)) {
                return;
            }

            cueAtIndex = playlistChannel.cues[cueIndex];

            head.OnStart(isTopLevel, playlistChannel, cueAtIndex, cueAtIndex.behavior, null, layer, player, suppliedTransform);
            
        }            
    }
}
