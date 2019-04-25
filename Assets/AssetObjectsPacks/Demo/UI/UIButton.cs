using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Syd.UI {

    [RequireComponent(typeof(Button))]
    public class UIButton : UIElement
    {
        public event System.Action onClick;
        public bool isBackButton;
        
        void Awake () {
            OnEnable();
            ((Button)selectable).onClick.AddListener( OnClick );
        }

        void OnClick () {
            if (onClick != null) {
                onClick();
            }
        }
    }
}
