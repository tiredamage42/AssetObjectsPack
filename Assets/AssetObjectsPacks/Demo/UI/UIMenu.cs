using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Syd.UI
{
	public class UIMenu : MonoBehaviour 
	{
		public event System.Action onClose;
		[SerializeField] bool canClose = true;
		[SerializeField] MenuPage m_startPage = null;
		MenuPage m_currentPage;
		bool m_isOpen;
		Image backgroundImage;


		void Start () {

			UIElement[] elements = GetComponentsInChildren<UIElement>();
			foreach (var s in elements) {
				s.onCancel += PageBack;

				UIButton asButton = s as UIButton;
				if (asButton != null && asButton.isBackButton) {
					asButton.onClick += PageBack;
				}
			}


			MenuPage[] allPages = GetComponentsInChildren<MenuPage>();
			foreach (var p in allPages) {
				p.gameObject.SetActive(false);
			}
			backgroundImage.enabled = false;

			UIUtils.OverrideUIInputControl();

			if (!canClose) {
				Open();
			}
		}

		public void Open()
		{
			if (!m_isOpen) {
				backgroundImage.enabled = true;
				
				EventSystem.current.SetSelectedGameObject(null);
				ChangePage(m_startPage, true);
				UIUtils.RestoreUIInputControl();
				m_isOpen = true;
			}
		}	

		public void Close()
		{
			if (canClose) {
				if(m_isOpen)
				{
					backgroundImage.enabled = false;
					if(m_currentPage != null)
					{
						m_currentPage.gameObject.SetActive(false);
						m_currentPage = null;
					}
					m_isOpen = false;

					if (onClose != null) {
						onClose();
					}
					UIUtils.OverrideUIInputControl();
				}
			}
		}

		void Awake () {
			backgroundImage = GetComponent<Image>();
			
		}
		
		public void PageBack() {
			if (m_currentPage.parentPage != null) {
				ChangePage(m_currentPage.parentPage, false);
			}
			else {
				Close();
			}
		}
		public void ChangePage(GameObject page){
			ChangePage(page.GetComponent<MenuPage>(), true);
		}

		void ChangePage(MenuPage page, bool forward){
			if(m_currentPage != null) {
				m_currentPage.gameObject.SetActive(false);
			}
			if (forward) {
				if (page != null) {
					page.parentPage = m_currentPage;
				}
			}

			m_currentPage = page;
			if(m_currentPage != null)
			{
				m_currentPage.gameObject.SetActive(true);
				
				EventSystem.current.SetSelectedGameObject(m_currentPage.firstSelected);
				EventSystem.current.GetComponent<StandaloneInputModule>().currentDefaultSelection = m_currentPage.firstSelected;
			}
		}
	}
}
