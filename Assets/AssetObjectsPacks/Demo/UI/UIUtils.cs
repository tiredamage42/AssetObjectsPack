using UnityEngine.EventSystems;

namespace Syd.UI {

    public class UIUtils 
    {
        public static void RestoreUIInputControl () {
			StandaloneInputModule module = EventSystem.current.GetComponent<StandaloneInputModule>();
			module.OverrideInput (false);

			//give a slight delay
			module.ActionTime();
		}

        public static void OverrideUIInputControl () {
            EventSystem.current.GetComponent<StandaloneInputModule>().OverrideInput(true);		
        }
    }
}
