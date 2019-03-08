using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AssetObjectsPacks {
    public class Playlist : MonoBehaviour {



        Cue[] cues;

        void OnDrawGizmos () {
            OnDrawGizmos(transform);
        }

        public void OnDrawGizmos(Transform baseTransform) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(baseTransform.position, Vector3.one);
            
            //if (cues == null || cues.Length == 0) 
                cues = GetComponentsInChildren<Cue>();

            if (cues.Length == 0) {
                Debug.Log("no cues in " + name);
            }
            
            for (int i = 0; i < cues.Length; i++) {
                Vector3 pos = baseTransform.position + (baseTransform.rotation * cues[i].transform.localPosition);
                Gizmos.color = cues[i].gizmoColor;
                Gizmos.DrawWireSphere(pos, .25f);
            }
        }

        [System.Serializable] public class Channel {
            public Cue[] cues;
            public Channel(Transform t) {
                cues = t.GetComponentsInChildren<Cue>();
            }
        }
        
/*
*/
        Channel[] _channels;
        public Channel[] channels {
            get {
                int c = transform.childCount;
                if (_channels == null || _channels.Length != c) _channels = c.Generate( i => new Channel(transform.GetChild(i)) ).ToArray();
                return _channels;
            }
        }

        // channels play cues at same time, and change cues at same time when ready
        // as opposed to staggered (whenever last cue is done)
        public bool syncChannels; 
        
        public void InitializePerformance (EventPlayer[] players, bool looped, System.Action onEndPerformance) {
            InitializePerformance(players, transform.position, transform.rotation, looped, onEndPerformance);
        }
        
        public void InitializePerformance (EventPlayer[] players, Vector3 position, Quaternion rotation, bool looped, System.Action onEndPerformance) {
            //Channel[] channels = transform.childCount.Generate( i => new Channel(transform.GetChild(i)) ).ToArray();
            
            
            int channelCount = channels.Length;
            int playerCount = players.Length;
            if (channelCount != playerCount) {
                Debug.LogError(name + " requires: " + channelCount + " players, got: " + playerCount);
                return;
            }   
            for (int a = 0; a < playerCount; a++) {
                for (int s = 0; s < players[a].current_playlists.Count; s++) {
                    if (players[a].current_playlists[s].playlist == this) {
                        Debug.LogError(name + " is already an active playlist for: " + players[a].name);
                        return;
                    }
                }   
            }
            
            Performance performance = AssetObjectsManager.GetNewPerformance();
            performance.InitializePerformance (this, channels, players, position, rotation, looped, onEndPerformance);

        }


        //instance of playlists that play out at run time
        public class Performance {

            public class PerformanceCue {
                Vector3 initialPlayerPos;
                Quaternion initialPlayerRot;
                float smooth_l0, smooth_l1, smooth_l0v, smooth_l1v;
                public bool positionReady {
                    get {
                        return posReady || playImmediate;
                    }
                }
                public bool isActive, isPlaying;

                bool playImmediate, posReady;


                CharacterController smoothedCC;
                Transform interestTransform;
                EventPlayer player;
                Cue cue;
                void CheckReadyTransform (EventPlayer player, Transform interestTransform, Cue cue) {
                    if (posReady) 
                        return;
                    
                    smooth_l0 = Mathf.SmoothDamp(smooth_l0, 1.0f, ref smooth_l0v, cue.smoothPositionTime);
                    smooth_l1 = Mathf.SmoothDamp(smooth_l1, 1.0f, ref smooth_l1v, cue.smoothRotationTime);
                    
                    player.transform.position = Vector3.Lerp(initialPlayerPos, interestTransform.position, smooth_l0);
                    player.transform.rotation = Quaternion.Slerp(initialPlayerRot, interestTransform.rotation, smooth_l1);
                    
                    float threshold = .99f;
                    
                    if (smooth_l0 > threshold && smooth_l1 > threshold) {
                    
                        player.transform.position = interestTransform.position;
                        player.transform.rotation = interestTransform.rotation;

                        player.overrideMovement = false;
                        if (smoothedCC) {
                            smoothedCC.enabled = true;
                        }
                        posReady = true;        
                        CustomScripting.ExecuteMessageBlockStep (player.gameObject, cue, interestTransform.position, CustomScripting.onSnap);

                        //Debug.Log("player ready");
                        //Debug.Break();
                    }
                }      
                public void InitializeCue (EventPlayer player, Transform interestTransform, Transform performanceRoot, Cue cue) {
                    this.cue = cue;
                    this.interestTransform = interestTransform;
                    this.player = player;
                    this.playImmediate = cue.playImmediate;

                    
                    isActive = true;
                    posReady = true;
                    isPlaying = false;
                    smoothedCC = null;

                    interestTransform.localPosition = cue.transform.localPosition;
                    //maybe zero out x and z rotation
                    interestTransform.localRotation = cue.transform.localRotation;
                    switch (cue.snapPlayerStyle) {
                        case Cue.SnapPlayerStyle.Snap:
                            player.transform.position = interestTransform.position;
                            player.transform.rotation = interestTransform.rotation;
                            break;
                        case Cue.SnapPlayerStyle.Smooth:
                            posReady = false;

                            player.overrideMovement = true;
                            
                            
                            smoothedCC = player.GetComponent<CharacterController>();
                            if (smoothedCC) {
                                smoothedCC.enabled = false;
                            }
                            initialPlayerPos = player.transform.position;
                            initialPlayerRot = player.transform.rotation;
                            smooth_l0 = smooth_l1 = 0;
                            break;
                    }     
                }
                /* 

                    const string cueTransformParamString = "cuetransform";
                    const string cuePositionParamString = "cueposition";
                    const string cueParamString = "cue";
                    const string nullParamString = "null";

                    
                    object ParamFromString(Cue cue, Transform cueTransform, string paramString) {
                        string lower = paramString.ToLower();
                        if (lower == cuePositionParamString) return cueTransform.position;
                        else if (lower == cueTransformParamString) return cueTransform;
                        else if (lower == cueParamString) return cue;

                        //Debug.Log(paramString);

                        string[] spl = paramString.Split(':');
                        string pType = spl[0];
                        string pVal = spl[1];

                        switch (pType) {
                            case "i": return int.Parse(pVal);
                            case "f": return float.Parse(pVal);
                            case "b": return bool.Parse(pVal);
                            case "s": return spl[1];
                        }

                        return null;
                    }

                    void BroadcastMessage (Cue cue, EventPlayer player, Transform cueTransform, string message) {
                        string msgName = message;
                        if (message.Contains("(")) {
                            string[] split = message.Split('(');

                            msgName = split[0];

                            string paramsS = split[1];

                            int l = paramsS.Length;
                            
                            string parmChecks = paramsS.Substring(0, l - 1);
                            
                            string[] paramStrings = parmChecks.Contains(',') ? parmChecks.Split(',') : new string[] { parmChecks };

                            l = paramStrings.Length;

                            object[] parameters = new object[l];
                            
                            for (int i = 0; i < l; i++) parameters[i] = ParamFromString(cue, cueTransform, paramStrings[i]);
                            
                            player.SendMessage(msgName, parameters, SendMessageOptions.RequireReceiver);
                        }
                        else {
                            player.SendMessage(msgName, SendMessageOptions.RequireReceiver);
                        }
                    }
                    void BroadcastMessageList (Cue cue, EventPlayer player, Transform cueTransform, string messages) {
                        messages = messages.Replace(" ", string.Empty).Trim();
                        if (messages.Contains("/")) {
                            string[] spl = messages.Split('/');
                            for (int i = 0; i < spl.Length; i++) {
                               BroadcastMessage(cue, player, cueTransform, spl[i]);
                            }
                        }
                        else {
                            BroadcastMessage(cue, player, cueTransform, messages);
                        }
                    }

                */

                public bool Play (EventPlayer player, Transform interestTransform, Cue cue) {


                    //Debug.Log("playing cue");
                    //Debug.Break();
                    isPlaying = true;


                    CustomScripting.ExecuteMessageBlockStep (player.gameObject, cue, interestTransform.position, CustomScripting.onPlay);


                    //if (cue.sendMessage != "") {
                    //    BroadcastMessageList(cue, player, interestTransform, cue.sendMessage);
                    //}
                    
                    if (cue.playlist != null) {
                        cue.playlist.InitializePerformance(new EventPlayer[] {player}, interestTransform.position, interestTransform.rotation, false, OnPlaylistEnd);
                        return false;
                    }

                    player.SubscribeToPlayEnd(-1, OnEventEnd);

                    player.PlayEvents_Cue(cue.events, cue.overrideDuration);//, OnEventEnd);
                    return false;
                }

                void OnPlaylistEnd () {
                    Debug.Log("on playlist end "  + cue.name);
                    
                    Deactivate();
                }

                void OnEventEnd (bool success) {
                    Debug.Log("on cue end " + cue.name);
                    Deactivate();
                }

                void Deactivate () {

                    CustomScripting.ExecuteMessageBlockStep (player.gameObject, cue, interestTransform.position, CustomScripting.onEnd);

                    //if (cue.postMessage != "") {
                    //    BroadcastMessageList(cue, player, interestTransform, cue.postMessage);
                    //}


                    cue = null;
                    player = null;
                    interestTransform = null;
                    isActive = false;


                }

                public void UpdateCue (EventPlayer player, Transform interestTransform, Cue cue) {
                    if (!isActive) 
                        return;
                    CheckReadyTransform(player, interestTransform, cue);
                    //if (!playerReady)
                    //    return;
                    
                }
            }

            [System.Serializable] public class PerformanceChannel {
                public EventPlayer player;
                public bool isActive;
                int cueIndex, curCueRepeats;
                Playlist.Channel playlistChannel;
                public PerformanceCue currentCue = new PerformanceCue();

                public void InitializeChannel (EventPlayer player, Playlist.Channel playlistChannel, Transform performanceRoot, Transform interestTransform) {
                    this.playlistChannel = playlistChannel;
                    this.player = player;
                    cueIndex = 0;
                    curCueRepeats = 0;
                    isActive = true;
                    currentCue.InitializeCue(player, interestTransform, performanceRoot, playlistChannel.cues[cueIndex]);
                }   
                public void PlayCue (Transform interestTransform) {
                    currentCue.Play(player, interestTransform, playlistChannel.cues[cueIndex]);   
                }
                public void UpdateChannel (Transform interestTransform) {
                    if (!isActive) return;   
                    currentCue.UpdateCue(player, interestTransform, playlistChannel.cues[cueIndex]);
                }
                public void OnCueEnd (Performance performance, Transform performanceRoot, Transform interestTransform) {
                    curCueRepeats++;
                    Debug.Log("on cue end, cueIndex: " + cueIndex + ", playlistChannel.cues.Length: " + playlistChannel.cues.Length + performance.playlist.name);
                    
                    if (curCueRepeats < playlistChannel.cues[cueIndex].repeats) {
                        Debug.Log("repeating" + performance.playlist.name);
                    
                        currentCue.InitializeCue(player, interestTransform, performanceRoot, playlistChannel.cues[cueIndex]);
                        return;
                    }




                    
                    curCueRepeats = 0;
                    cueIndex++;
                    Debug.Log("going up cue, " + cueIndex + performance.playlist.name);
                    if (cueIndex< playlistChannel.cues.Length) {
                    Debug.Log("new cue, " + playlistChannel.cues[cueIndex].name);
                        
                    }

                    while (cueIndex < playlistChannel.cues.Length && !playlistChannel.cues[cueIndex].gameObject.activeSelf) {
                    
                        cueIndex ++;
                        Debug.Log("skipping inactive, " + cueIndex + performance.playlist.name);
                    }

                    if (cueIndex >= playlistChannel.cues.Length) {
                        Debug.Log("on channel performance end " + performance.playlist.name);
                        OnPerformanceEnd(performance);
                        return;
                    }


                    currentCue.InitializeCue(player, interestTransform, performanceRoot, playlistChannel.cues[cueIndex]);
                }
                public void OnPerformanceEnd (Performance performance) {
                    if (player) {
                        player.current_playlists.Remove(performance);
                        player = null;
                    }
                    isActive = false;
                }
            }

            public Playlist playlist;
            int performance_key;
            public void SetPerformanceKey (int key) {
                this.performance_key = key;
            }
            Transform performance_root_transform;
            List<Transform> interestTransforms = new List<Transform>();
            System.Action on_performance_done;
            PerformanceChannel[] channels = new PerformanceChannel[0];
            EventPlayer[] orig_players;
            System.Action orig_performance_done_callback;
            bool looped;

            public void InterruptPerformance () {
                for (int i = 0; i < channels.Length; i++) {   
                    channels[i].OnPerformanceEnd(this);
                }
                on_performance_done = null;
                AssetObjectsManager.ReturnPerformanceToPool(performance_key);
            }

            //public void ClearPerformance () {
            //    this.on_performance_done = null;
            //    this.parent_scene = null;
            //}
            public void InitializePerformance (Playlist playlist, Channel[] playlistChannels, EventPlayer[] players, Vector3 position, Quaternion rotation, bool looped, System.Action on_performance_done) {
                this.on_performance_done = on_performance_done;
                this.playlist = playlist;
                this.looped = looped;

                if (looped) {
                    orig_players = players;
                    orig_performance_done_callback = on_performance_done;
                }

                if (!performance_root_transform) {
                    performance_root_transform = new GameObject("performance_root_transform").transform;
                }
                performance_root_transform.position = position;
                performance_root_transform.rotation = rotation;
                
                int channel_count = playlistChannels.Length;// playlist.channels.Length;
                if (channels.Length != channel_count) {
                    channels = channel_count.Generate(i => new PerformanceChannel()).ToArray();
                }

                if (interestTransforms.Count != channel_count) {
                    if (interestTransforms.Count < channel_count) {
                        int c = channel_count - interestTransforms.Count;
                        for (int i = 0; i < c; i++) {
                            Transform new_trans = new GameObject("ChannelInterestTransform").transform;
                            new_trans.SetParent(performance_root_transform);
                            interestTransforms.Add(new_trans);
                        }
                    }
                }

                for (int i = 0; i < channel_count; i++) {
                    players[i].current_playlists.Add(this);
                    channels[i].InitializeChannel (players[i], playlistChannels[i], performance_root_transform, interestTransforms[i]);
                
                }
            }
            public void UpdatePerformance () {
                bool cuesReadySynced = true;
                bool cuesDoneSynced = true;
                bool allDone = true;
                int c = channels.Length;
                for (int i = 0; i < c; i++) {

                    PerformanceChannel playerChannel = channels[i];
                    if (!playerChannel.currentCue.positionReady) {
                        cuesReadySynced = false;
                    }
                    if (playerChannel.currentCue.isActive) {
                        cuesDoneSynced = false;
                    }
                    if (playerChannel.isActive) {
                        allDone = false;
                    }
                }

                if (playlist.syncChannels) {
                    for (int i = 0; i < c; i++) {
                        PerformanceChannel playerChannel = channels[i];
                        if (!playerChannel.currentCue.isPlaying && cuesReadySynced)
                            playerChannel.PlayCue (interestTransforms[i]);

                        if (cuesDoneSynced) 
                            playerChannel.OnCueEnd (this, performance_root_transform, interestTransforms[i]);
                        
                        //if (allDone) 
                        //    channels[i].OnPerformanceEnd(this);
                    }

                }
                else {

                    for (int i = 0; i < c; i++) {
                        PerformanceChannel playerChannel = channels[i];
                        if (!playerChannel.currentCue.isPlaying && playerChannel.currentCue.positionReady)
                            playerChannel.PlayCue (interestTransforms[i]);
                        
                        if (!playerChannel.currentCue.isActive && playerChannel.isActive) {

                            Debug.Log("ending cue cause not current cue active " + playlist.name);
                            playerChannel.OnCueEnd (this, performance_root_transform, interestTransforms[i]);
                        }
                        
                        //if (!playerChannel.isActive)
                        //    channels[i].OnPerformanceEnd(this);
                    }
                }

                if (allDone) {
                    Debug.Log("all done");
                    if (on_performance_done != null) {
                        on_performance_done();
                        on_performance_done = null;
                    }
                    AssetObjectsManager.ReturnPerformanceToPool(performance_key);

                    if (looped) {
                        playlist.InitializePerformance(orig_players, performance_root_transform.position, performance_root_transform.rotation, looped, orig_performance_done_callback);
                    }
                    return;
                }
                for (int i = 0; i < c; i++) {
                    channels[i].UpdateChannel(interestTransforms[i]);
                }
            }
        }
    }
}