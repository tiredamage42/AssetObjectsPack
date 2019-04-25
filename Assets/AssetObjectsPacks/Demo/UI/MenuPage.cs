using UnityEngine;
using UnityEngine.UI;
namespace Syd.UI
{
	public class MenuPage : MonoBehaviour 
	{
		[HideInInspector] public GameObject firstSelected;
		[HideInInspector] public MenuPage parentPage;
		void Awake () {
			firstSelected = GetComponentsInChildren<Selectable>()[0].gameObject;
		}
	}
}