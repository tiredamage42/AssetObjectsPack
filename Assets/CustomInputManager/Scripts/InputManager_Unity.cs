
using UnityEngine;

namespace CustomInputManager
{
	public partial class InputManager : MonoBehaviour
	{
		public static Vector3 acceleration { get { return Input.acceleration; } }
		public static int accelerationEventCount { get { return Input.accelerationEventCount; } }
		public static AccelerationEvent[] accelerationEvents { get { return Input.accelerationEvents; } }
		public static bool anyKey { get { return Input.anyKey; } }
		public static bool anyKeyDown { get { return Input.anyKeyDown; } }
		public static Compass compass { get { return Input.compass; } }
		public static string compositionString { get { return Input.compositionString; } }
		public static DeviceOrientation deviceOrientation { get { return Input.deviceOrientation; } }
		public static Gyroscope gyro { get { return Input.gyro; } }
		public static bool imeIsSelected { get { return Input.imeIsSelected; } }
		public static string inputString { get { return Input.inputString; } }
		public static LocationService location { get { return Input.location; } }
		public static Vector2 mousePosition { get { return Input.mousePosition; } }
		public static bool mousePresent { get { return Input.mousePresent; } }
		public static Vector2 mouseScrollDelta { get { return Input.mouseScrollDelta; } }
		public static bool touchSupported { get { return Input.touchSupported; } }
		public static int touchCount { get { return Input.touchCount; } }
		public static Touch[] touches { get { return Input.touches; } }
		
		public static bool compensateSensors
		{
			get { return Input.compensateSensors; }
			set { Input.compensateSensors = value; }
		}
		
		public static Vector2 compositionCursorPos
		{
			get { return Input.compositionCursorPos; }
			set { Input.compositionCursorPos = value; }
		}
		
		public static IMECompositionMode imeCompositionMode
		{
			get { return Input.imeCompositionMode; }
			set { Input.imeCompositionMode = value; }
		}
		
		public static bool multiTouchEnabled
		{
			get { return Input.multiTouchEnabled; }
			set { Input.multiTouchEnabled = value; }
		}
		
		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return Input.GetAccelerationEvent(index);
		}
		
		public static float GetAxis(string name, int playerID=0)
		{
			InputAction action = GetAction(playerID, name);
			if(action != null)
			{
				return action.GetAxis(playerID);
			}
			else
			{
				Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return 0.0f;
			}
		}
		
		public static float GetAxisRaw(string name, int playerID=0)
		{
			InputAction action = GetAction(playerID, name);
			if(action != null)
			{
				return action.GetAxisRaw(playerID);
			}
			else
			{
                Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return 0.0f;
			}
		}
		
		public static bool GetButton(string name, int playerID=0)
		{
			InputAction action = GetAction(playerID, name);
			if(action != null)
			{
				return action.GetButton(playerID);
			}
			else
			{
				Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return false;
			}
		}
		
		public static bool GetButtonDown(string name, int playerID=0)
		{
			InputAction action = GetAction(playerID, name);
			if(action != null)
			{
				return action.GetButtonDown(playerID);
			}
			else
			{
                Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return false;
			}
		}
		
		public static bool GetButtonUp(string name, int playerID=0)
		{
			InputAction action = GetAction(playerID, name);
			if(action != null)
			{
				return action.GetButtonUp(playerID);
			}
			else
			{
                Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return false;
			}
		}
		
		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}
		
		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}
		
		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}
		
		public static bool GetMouseButton(int index)
		{
			return Input.GetMouseButton(index);
		}
		
		public static bool GetMouseButtonDown(int index)
		{
			return Input.GetMouseButtonDown(index);
		}
		
		public static bool GetMouseButtonUp(int index)
		{
			return Input.GetMouseButtonUp(index);
		}
		
		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}
		
		public static string[] GetJoystickNames()
		{
			return Input.GetJoystickNames();
		}
        
        public static void ResetInputAxes()
        {
            Input.ResetInputAxes();
        }
	}
}
