
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Syd.UI;
using CustomInputManager;

public class Game : MonoBehaviour 
{
	public UIMenu pauseMenu;

	static Game m_instance;
	public float defaultTimescale = 1.0f;

	public bool isPaused;
	public static bool IsPaused { get { return m_instance.isPaused; } }
	public static float initialTimeDilation { get { return m_instance.defaultTimescale; } }
	public static float timeDilation {
		get { return m_instance.currentTimeDilation; }
		set { m_instance.currentTimeDilation = value; } 
	}
	
	float currentTimeDilation = 1.0f;
	float initialTimeScale, initialFixedDeltaTime, initialMaxDelta;
    
	
	public static void Pause()
	{
		if (!IsPaused) {

			m_instance.currentTimeDilation = 0.0f;
			m_instance.UpdateTimeValues();

			m_instance.isPaused = true;
			m_instance.StartCoroutine(m_instance._Pause());
		}
			
	}
	IEnumerator _Pause () {
		yield return null;
		pauseMenu.Open();	
	}
	IEnumerator _Unpause () {
		yield return null;
		currentTimeDilation = defaultTimescale;	
		pauseMenu.Close();
	}

	
	
	public static void UnPause()
	{
		if (IsPaused) {
			m_instance.isPaused = false;
			m_instance.StartCoroutine(m_instance._Unpause());
		}
	}

	void UpdateTimeValues () {
		Time.timeScale = initialTimeScale * currentTimeDilation;
		Time.fixedDeltaTime = initialFixedDeltaTime * currentTimeDilation;
		Time.maximumDeltaTime = initialMaxDelta * currentTimeDilation;
	}

	
	void Awake()
	{
		if(m_instance != null)
		{
			Destroy(this);
		}
		else
		{
			m_instance = this;
			SceneManager.sceneLoaded += HandleLevelWasLoaded;
			DontDestroyOnLoad(gameObject);
			pauseMenu.onClose += UnPause;


			initialTimeScale = Time.timeScale;
			initialFixedDeltaTime = Time.fixedDeltaTime;
			initialMaxDelta = Time.maximumDeltaTime;
		}
	}

	void Update () {
			if (!isPaused) {
				currentTimeDilation = Mathf.Clamp(currentTimeDilation, .1f, 1);

				if(InputManager.GetButtonDown("Pause"))
				{
					Pause();
					pauseMenu.Open();
				}
			}

			UpdateTimeValues();
		}

		public void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}


	void HandleLevelWasLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		UnPause();
	}
	void OnDestroy()
	{
		SceneManager.sceneLoaded -= HandleLevelWasLoaded;
	}
	
}
