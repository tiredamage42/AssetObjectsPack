using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Syd.UI {

    // [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Selectable))]
    [ExecuteInEditMode()]
    public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler, ICancelHandler
    {
        public event System.Action onCancel;
        public UIButtonProfile profile;
        protected Selectable selectable;
        // Image image;
        public Text text;
        void SetColors () {
            if (profile == null) {
                return;
            }
            ColorBlock colors = selectable.colors;

            colors.normalColor = profile.normalColor;
            colors.highlightedColor = profile.highlightedColor;
            colors.pressedColor = profile.pressedColor;
            colors.disabledColor = profile.disabledColor;

            selectable.colors = colors;

            // image.sprite = profile.sprite;

            if (text != null) {
                text.color = profile.textColor;
            }
        }

        protected void OnEnable() {

            text = GetComponentInChildren<Text>();
            // image = GetComponent<Image>();
            selectable = GetComponent<Selectable>();
            
            // image.raycastTarget = false;
            selectable.interactable = true;
            selectable.transition = Selectable.Transition.ColorTint;
            // selectable.targetGraphic = image;
            
            Navigation nav = selectable.navigation;
            nav.mode = Navigation.Mode.Automatic;
            selectable.navigation = nav;
            SetColors();
        }

        
        
        void Update () {
            SetColors();
        }

        public void OnPointerEnter(PointerEventData pointerEventData) {
            if (!EventSystem.current.alreadySelecting) {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }
        public void OnPointerExit(PointerEventData pointerEventData) {
            EventSystem.current.SetSelectedGameObject(null);
        }
        public void OnDeselect(BaseEventData eventData) {
            selectable.OnPointerExit(null);
        }
        public void OnCancel(BaseEventData eventData) {
            if (onCancel != null) {
                onCancel();
            }
        }
    }

}
