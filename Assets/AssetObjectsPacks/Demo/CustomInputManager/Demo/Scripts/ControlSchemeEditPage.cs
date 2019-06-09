
using UnityEngine;
using System.IO;
using Syd.UI;

namespace CustomInputManager.Examples
{
	public class ControlSchemeEditPage : MonoBehaviour 
	{
		public UIButtonProfile normalProfile, scanningProfile;
		public RectTransform rebindParent;
		public GameObject rebindElement;

		[SerializeField] private TextAsset m_defaultInputProfile = null;
		[SerializeField] private string m_controlSchemeName = null;


		void BuildRebindElements () {
			ControlScheme controlScheme = InputManager.GetControlScheme(m_controlSchemeName);
			
			for (int i = 0; i < controlScheme.Actions.Count; i++) {
				if (controlScheme.Actions[i].rebindable) {

					GameObject newRebindElement = Instantiate(rebindElement);
					newRebindElement.transform.SetParent(rebindParent);

					RebindInputButton rebinder = newRebindElement.GetComponent<RebindInputButton>();
					rebinder.actionName = controlScheme.Actions[i].Name;
					
					rebinder.m_controlSchemeName = m_controlSchemeName;
					rebinder.normalProfile = normalProfile;
					rebinder.scanningProfile = scanningProfile;

					rebinder.Initialize();
				}
			}

		}


		void Awake () {
			BuildRebindElements();
			UIElement[] elements = GetComponentsInChildren<UIElement>();
			foreach (var s in elements) {
				s.onCancel += InputManager.SaveCustomBindings;

				UIButton asButton = s as UIButton;
				if (asButton != null && asButton.isBackButton) {
					asButton.onClick += InputManager.SaveCustomBindings;
				}
			}
		}
		
		public void ResetScheme()
		{
			ControlScheme defControlScheme = null;
			using(StringReader reader = new StringReader(m_defaultInputProfile.text))
			{
				defControlScheme = new InputLoaderXML(reader).Load(m_controlSchemeName);
			}

			if(defControlScheme != null)
			{
				ControlScheme controlScheme = InputManager.GetControlScheme(m_controlSchemeName);
				if(defControlScheme.Actions.Count == controlScheme.Actions.Count)
				{
					for(int i = 0; i < defControlScheme.Actions.Count; i++)
					{
						controlScheme.Actions[i].Copy(defControlScheme.Actions[i]);
					}

					InputManager.Reinitialize();

					RebindInputButton[] rebinders = GetComponentsInChildren<RebindInputButton>();
					foreach (var t in rebinders) {
						t.RefreshText();
					}
				}
				else
				{
					Debug.LogError("Current and default control scheme don't have the same number of actions");
				}
			}
			else
			{
				Debug.LogErrorFormat("Default input profile doesn't contain a control scheme named '{0}'", m_controlSchemeName);
			}
		}
	}
}