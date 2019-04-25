using UnityEngine;

namespace Syd.UI {

    [CreateAssetMenu()]
    public class UIButtonProfile : ScriptableObject
    {
        public Color32 normalColor = new Color32(255,255,255,255);
        public Color32 highlightedColor = new Color32(255,255,255,255);
        public Color32 pressedColor = new Color32(200,200,200,255);
        public Color32 disabledColor = new Color32(200,200,200,128);
        public Color32 textColor = new Color32(0,0,0,255);

        public Sprite sprite;
    }
}
