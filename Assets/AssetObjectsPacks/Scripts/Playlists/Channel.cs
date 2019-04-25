using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetObjectsPacks {

    namespace Playlists {

            public class PerformanceChannel {
                public bool isActive;
                public EventPlayer player;

                int cueIndex, curCueRepeats;
                Playlist.Channel playlistChannel;

                // like the tape head on a deck, or needle on a record player...
                public PerformanceHead head = new PerformanceHead();


                // protected abstract PerformanceHead GetPerformanceHead();
                    
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
                    //Debug.Log("play cue: " + playlistChannel.childCues[cueIndex].name);
                    head.OnPlay(debugReason, asInterrupter);   
                }

                // public void InitializeChannel (int layer, EventPlayer player, CueBehavior cueBehavior, Transform interestTransform, Performance performance) {
                //     this.player = player;
                //     isActive = true;
                //     head.OnStart(cueBehavior, layer, player, interestTransform);
                // }   

                public void InitializeChannel (bool isTopLevel, int layer, EventPlayer player, Playlist.Channel playlistChannel, MiniTransform suppliedTransform, Transform interestTransform, Performance performance) {
                    this.playlistChannel = playlistChannel;
                    if (playlistChannel == null){
                        Debug.LogError("NULL");
                    }
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
                    // head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
                    head.OnStart(isTopLevel, playlistChannel, cueIndex, layer, player, suppliedTransform, interestTransform);

                }   
                public void UpdateChannel () {
                    if (!isActive) return;   
                    head.UpdateHead();
                }
                bool SkipAheadPastDisabledCues (Performance performance) {

                    int l = playlistChannel.cues.Length;
                    while (cueIndex < l && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {                    
                        cueIndex ++;
                    }
                    if (cueIndex >= l) {
                        //Debug.Log("ending performance");
                        OnPerformanceEnd(performance, false);
                        return true;
                    }
                    return false;
                }

                

                // public void OnCueEnd (Performance performance) {   
                //     OnPerformanceEnd(performance, false);
                // }
                public void OnCueEnd (bool isTopLevel, int layer, Performance performance, MiniTransform suppliedTransform, Transform interestTransform) {
                    
                    if (playlistChannel.cues != null) {

                        curCueRepeats++;
                        Cue cueAtIndex = playlistChannel.cues[cueIndex];
                        
                        if (curCueRepeats < cueAtIndex.repeats) {
                            // head.OnStart(isTopLevel, cueAtIndex, layer, player, interestTransform);
                            head.OnStart(isTopLevel, playlistChannel, cueIndex, layer, player, suppliedTransform, interestTransform);
                            return;
                        }
                        if (playlistChannel.useRandomChoice) {
                            OnPerformanceEnd(performance, false);
                            return;
                        }
                        // int l = playlistChannel.subCount;
                        
                        curCueRepeats = 0;
                        cueIndex++;

                        if (SkipAheadPastDisabledCues(performance)) {
                            return;
                        }

                        //Debug.Log("playing cue " + playlistChannel.childCues[cueIndex].name);
                        // head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
                        head.OnStart(isTopLevel, playlistChannel, cueIndex, layer, player, suppliedTransform, interestTransform);
                    }
                    else {
                        OnPerformanceEnd(performance, false);
                    }
                    


                }
                
                
            }

            // public class PlaylistChannel : PerformanceChannel {
                // int cueIndex, curCueRepeats;
                // Playlist.Channel playlistChannel;
                // public PlaylistHead head = new PlaylistHead();

                // protected override PerformanceHead GetPerformanceHead() { return head; }

                // public void InitializeChannel (bool isTopLevel, int layer, EventPlayer player, Playlist.Channel playlistChannel, Transform interestTransform, Performance performance) {
                //     this.playlistChannel = playlistChannel;
                //     this.player = player;

                //     //only if it has subcues
                //     if (playlistChannel.useRandomChoice) {
                //         List<int> enabledIndicies = new List<int>();
                //         for (int i = 0; i < playlistChannel.cues.Length; i++) {
                //             if (playlistChannel.cues[i].gameObject.activeSelf) {
                //                 enabledIndicies.Add(i);
                //             }
                //             i++;
                //         }
                //         cueIndex = enabledIndicies.RandomChoice();
                //     }
                //     else {
                //         cueIndex = 0;   
                //     }

                //     if (!isTopLevel) {
                //         if (SkipAheadPastDisabledCues(performance)) {
                //             return;
                //         }
                //     }
                //     curCueRepeats = 0;
                //     isActive = true;
                //     head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
                // }   
                // public void UpdateChannel () {
                //     if (!isActive) return;   
                //     head.UpdateHead();
                // }
                // bool SkipAheadPastDisabledCues (Performance performance) {

                //     int l = playlistChannel.cues.Length;
                //     while (cueIndex < l && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {                    
                //         cueIndex ++;
                //     }
                //     if (cueIndex >= l) {
                //         //Debug.Log("ending performance");
                //         OnPerformanceEnd(performance, false);
                //         return true;
                //     }
                //     return false;
                // }
                
                // public void OnCueEnd (bool isTopLevel, int layer, Performance performance, Transform interestTransform) {
                    
                //     curCueRepeats++;

                //     Cue cueAtIndex = playlistChannel.cues[cueIndex];
                    
                //     if (curCueRepeats < cueAtIndex.repeats) {
                //         head.OnStart(isTopLevel, cueAtIndex, layer, player, interestTransform);
                //         return;
                //     }
                    
                //     if (playlistChannel.useRandomChoice) {
                //         OnPerformanceEnd(performance, false);
                //         return;
                //     }

                //     int l = playlistChannel.subCount;
                    
                //     curCueRepeats = 0;
                //     cueIndex++;

                //     if (SkipAheadPastDisabledCues(performance)) {
                //         return;
                //     }

                //     //Debug.Log("playing cue " + playlistChannel.childCues[cueIndex].name);
                //     head.OnStart(isTopLevel, playlistChannel.cues[cueIndex], layer, player, interestTransform);
                // }
            // }


            // public class CueBahaviorChannel : PerformanceChannel {
                // public CueBehaviorHead head = new CueBehaviorHead();
                // protected override PerformanceHead GetPerformanceHead() { return head; }


                // public void InitializeChannel (int layer, EventPlayer player, CueBehavior cueBehavior, Transform interestTransform, Performance performance) {
                //     this.player = player;
                //     isActive = true;
                //     head.OnStart(cueBehavior, layer, player, interestTransform);
                // }   
                // public void UpdateChannel () {
                //     if (!isActive) return;   
                //     head.UpdateHead();
                // }
                // public void OnCueEnd (Performance performance) {   
                //     OnPerformanceEnd(performance, false);
                // }
            // }
            // public class EventChannel : PerformanceChannel {
                // public EventHead head = new EventHead();
                // protected override PerformanceHead GetPerformanceHead() { return head; }

                // public void InitializeChannel (int layer, EventPlayer player, Event aoEvent, Transform interestTransform) {
                //     this.player = player;
                //     isActive = true;
                //     head.OnStart(aoEvent, layer, player, interestTransform);
                // }   
                // public void OnCueEnd (Performance performance) {   
                //     OnPerformanceEnd(performance, false);
                // }
            // }
        // }
    }
}






