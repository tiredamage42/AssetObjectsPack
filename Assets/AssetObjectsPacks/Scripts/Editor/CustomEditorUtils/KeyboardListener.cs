using System.Collections.Generic;
using UnityEngine;
namespace AssetObjectsPacks {
    public class KeyboardListener {
        public bool this [KeyCode c] {
            get {
                bool r;
                if (!listenCodes.TryGetValue(c, out r)) {
                    UnityEngine.Event e = UnityEngine.Event.current;
                    if (e.type != EventType.KeyDown) return false;
                    if (GUIUtils.KeyboardOverriden()) return false;
                    r = e.keyCode == c;
                    if (r) e.Use();
                    listenCodes[c] = r;
                }
                return r;
            }
        }
        public bool shift { get { return UnityEngine.Event.current.shift; } }
        public bool command { get { return UnityEngine.Event.current.command; } }
        public bool ctrl { get { return UnityEngine.Event.current.control; } }
        
        Dictionary<KeyCode, bool> listenCodes = new Dictionary<KeyCode, bool>();
    }
}