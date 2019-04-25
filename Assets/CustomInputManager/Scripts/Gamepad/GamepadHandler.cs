using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CustomInputManager
{
    public enum GamepadAxis
	{
		LeftThumbstickX, LeftThumbstickY,
		RightThumbstickX, RightThumbstickY,
		DPadX, DPadY,
		LeftTrigger, RightTrigger,
		None
	}

    public enum GamepadButton
	{
		LeftStick, RightStick,
		LeftBumper, RightBumper,
		DPadUp, DPadDown, DPadLeft, DPadRight,
		Back, Start,
		ActionBottom, ActionRight, ActionLeft, ActionTop,
		None
	}

    public enum GamepadDPadType { Axis = 0, Button }
    
    [System.Serializable] public class GamepadHandler
    {
        
        struct DPadState
        {
            public float[] axes; //x, y
            public ButtonState Up, Down, Left, Right;
            public static DPadState Empty => new DPadState() {
                axes = new float[2],

                Up = ButtonState.Released,
                Down = ButtonState.Released,
                Right = ButtonState.Released,
                Left = ButtonState.Released
            };
        }

        [SerializeField] private GenericGamepadProfile m_gamepadProfile;
        [Tooltip("At what interval(in sec) to check how many joysticks are connected.")]
        [SerializeField] private float m_joystickCheckFrequency = 1.0f;
        [SerializeField] private float m_dpadGravity = 3.0f;
        [SerializeField] private float m_dpadSensitivity = 3.0f;
        [SerializeField] private bool m_dpadSnap = true;
        
        DPadState[] m_dpadState;
        bool[] m_joystickState;
        Dictionary<int, string> m_axisNameLookupTable;

        public GenericGamepadProfile GamepadProfile
        {
            get { return m_gamepadProfile; }
            set { m_gamepadProfile = value; }
        }
        
        
        public void Awake(InputManager inputManager)
        {
            m_axisNameLookupTable = new Dictionary<int, string>();
            m_joystickState = new bool[InputBinding.MAX_JOYSTICKS];

            m_dpadState = new DPadState[InputBinding.MAX_JOYSTICKS];
            for (int i = 0; i < InputBinding.MAX_JOYSTICKS; i++) {
                m_dpadState[i] = DPadState.Empty;
            }

            GenerateAxisNameLookupTable();
            inputManager.StartCoroutine(CheckForGamepads());
        }



        private IEnumerator CheckForGamepads()
        {
            while(true)
            {
                string[] names = InputManager.GetJoystickNames();
                for(int i = 0; i < m_joystickState.Length; i++)
                {
                    m_joystickState[i] = names.Length > i && !string.IsNullOrEmpty(names[i]);
                }

                yield return new WaitForSecondsRealtime(m_joystickCheckFrequency);
            }
        }

		public bool GamepadIsConnected(int gamepad)
        {
            return m_joystickState[gamepad];
        }


        private void GenerateAxisNameLookupTable()
        {
            for(int joy = 0; joy < InputBinding.MAX_JOYSTICKS; joy++)
            {
                for(int axis = 0; axis < InputBinding.MAX_JOYSTICK_AXES; axis++)
                {
                    int key = joy * InputBinding.MAX_JOYSTICK_AXES + axis;
                    m_axisNameLookupTable[key] = string.Format("joy_{0}_axis_{1}", joy, axis);
                }
            }
        }

        public void OnUpdate(float deltaTime)
        {
            if(m_gamepadProfile != null)
            {
                
                if(m_gamepadProfile.DPadType == GamepadDPadType.Button)
                {
                    // mimic axis values
                    UpdateDPadAxis(deltaTime, 0, m_gamepadProfile.DPadRightButton, m_gamepadProfile.DPadLeftButton);
                    UpdateDPadAxis(deltaTime, 1, m_gamepadProfile.DPadUpButton, m_gamepadProfile.DPadDownButton);
                }
                else
                {
                    // mimic button values
                    UpdateDPadButton();
                }
            }
        }

        private void UpdateDPadAxis(float deltaTime, int axis, int posButton, int negButton)
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {

                bool posPressed = GamepadIsConnected(i) ? GetButton(posButton, i) : false;
                bool negPressed = GamepadIsConnected(i) ? GetButton(negButton, i) : false;

                if(posPressed)
                {
                    if(m_dpadState[i].axes[axis] < InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].axes[axis] = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].axes[axis] += m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].axes[axis] > InputBinding.AXIS_POSITIVE)
                    {
                        m_dpadState[i].axes[axis] = InputBinding.AXIS_POSITIVE;
                    }
                }
                else if(negPressed)
                {
                    if(m_dpadState[i].axes[axis] > InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].axes[axis] = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].axes[axis] -= m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].axes[axis] < InputBinding.AXIS_NEGATIVE)
                    {
                        m_dpadState[i].axes[axis] = InputBinding.AXIS_NEGATIVE;
                    }
                }
                else
                {
                    if(m_dpadState[i].axes[axis] < InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].axes[axis] += m_dpadGravity * deltaTime;
                        if(m_dpadState[i].axes[axis] > InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].axes[axis] = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                    else if(m_dpadState[i].axes[axis] > InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].axes[axis] -= m_dpadGravity * deltaTime;
                        if(m_dpadState[i].axes[axis] < InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].axes[axis] = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                }
            }
        }

        

        private void UpdateDPadButton()
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {
                float x = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + m_gamepadProfile.DPadXAxis]);
                float y = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + m_gamepadProfile.DPadYAxis]);

                m_dpadState[i].Up = GetNewDPadButtonState(y >= 0.9f, m_dpadState[i].Up);
                m_dpadState[i].Down = GetNewDPadButtonState(y <= -0.9f, m_dpadState[i].Down);
                
                m_dpadState[i].Left = GetNewDPadButtonState(x <= -0.9f, m_dpadState[i].Left);
                m_dpadState[i].Right = GetNewDPadButtonState(x >= 0.9f, m_dpadState[i].Right);
            }
        }

        private ButtonState GetNewDPadButtonState(bool isPressed, ButtonState oldState)
        {
            ButtonState newState = isPressed ? ButtonState.Pressed : ButtonState.Released;
            if(oldState == ButtonState.Pressed || oldState == ButtonState.JustPressed)
                newState = isPressed ? ButtonState.Pressed : ButtonState.JustReleased;
            else if(oldState == ButtonState.Released || oldState == ButtonState.JustReleased)
                newState = isPressed ? ButtonState.JustPressed : ButtonState.Released;

            return newState;
        }

        

        HashSet<int> checkedGamepadLTriggersForInitialization = new HashSet<int>();
        HashSet<int> checkedGamepadRTriggersForInitialization = new HashSet<int>();
        
        // xbox controller triggers on OSX initialize at 0 but have range -1, 1
        float AdjustOSXAxis (float rawAxis, int gamepad, ref HashSet<int> checkSet) {
            float adjustedAxis = 0.0f;
            bool checkedTrigger = checkSet.Contains(gamepad);
            if((rawAxis > -0.9f && rawAxis < -0.0001f) && !checkedTrigger){
                checkSet.Add(gamepad);
                checkedTrigger = true;
            }
            if(checkedTrigger) {
                adjustedAxis = (rawAxis + 1) * 0.5f;
            }
            return adjustedAxis;
        }

        public float GetAxis(GamepadAxis axis, int gamepad)
        {
            if(m_gamepadProfile == null)
                return 0.0f;

            int axisID = -1;

            switch(axis)
            {
            case GamepadAxis.LeftThumbstickX: axisID = m_gamepadProfile.LeftStickXAxis; break;
            case GamepadAxis.LeftThumbstickY: axisID = m_gamepadProfile.LeftStickYAxis; break;
            case GamepadAxis.RightThumbstickX: axisID = m_gamepadProfile.RightStickXAxis; break;
            case GamepadAxis.RightThumbstickY: axisID = m_gamepadProfile.RightStickYAxis; break;
            
            case GamepadAxis.DPadX: 
                if (m_gamepadProfile.DPadType == GamepadDPadType.Button) {
                    return m_dpadState[gamepad].axes[0];
                }
                axisID = m_gamepadProfile.DPadXAxis; break;
            case GamepadAxis.DPadY: 
                if (m_gamepadProfile.DPadType == GamepadDPadType.Button) {
                    return m_dpadState[gamepad].axes[1];
                }
            
                axisID = m_gamepadProfile.DPadYAxis; break;
            
            case GamepadAxis.LeftTrigger:
                axisID = m_gamepadProfile.LeftTriggerAxis;
                {
                    float rawAxis = Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + axisID]);
                    return AdjustOSXAxis (rawAxis, gamepad, ref checkedGamepadLTriggersForInitialization);
                }
            case GamepadAxis.RightTrigger:
                axisID = m_gamepadProfile.RightTriggerAxis;
                {
                    float rawAxis = Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + axisID]);
                    return AdjustOSXAxis (rawAxis, gamepad, ref checkedGamepadRTriggersForInitialization);
                }
            }

            return axisID >= 0 ? Input.GetAxis(m_axisNameLookupTable[gamepad * InputBinding.MAX_JOYSTICK_AXES + axisID]) : 0.0f;
        }

        public float GetAxisRaw(GamepadAxis axis, int gamepad)
        {
            float value = GetAxis(axis, gamepad);
            return Mathf.RoundToInt(value);// Mathf.Approximately(value, 0) ? 0.0f : Mathf.Sign(value);
        }

        public bool GetButton(GamepadButton button, int gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick: return GetButton(m_gamepadProfile.LeftStickButton, gamepad);
            case GamepadButton.RightStick: return GetButton(m_gamepadProfile.RightStickButton, gamepad);
            case GamepadButton.LeftBumper: return GetButton(m_gamepadProfile.LeftBumperButton, gamepad);
            case GamepadButton.RightBumper: return GetButton(m_gamepadProfile.RightBumperButton, gamepad);
            
            case GamepadButton.Back: return GetButton(m_gamepadProfile.BackButton, gamepad);
            case GamepadButton.Start: return GetButton(m_gamepadProfile.StartButton, gamepad);
            case GamepadButton.ActionBottom: return GetButton(m_gamepadProfile.ActionBottomButton, gamepad);
            case GamepadButton.ActionRight: return GetButton(m_gamepadProfile.ActionRightButton, gamepad);
            case GamepadButton.ActionLeft: return GetButton(m_gamepadProfile.ActionLeftButton, gamepad);
            case GamepadButton.ActionTop: return GetButton(m_gamepadProfile.ActionTopButton, gamepad);
            
            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButton(m_gamepadProfile.DPadUpButton, gamepad) : m_dpadState[gamepad].Up == ButtonState.Pressed;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButton(m_gamepadProfile.DPadDownButton, gamepad) : m_dpadState[gamepad].Down == ButtonState.Pressed;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButton(m_gamepadProfile.DPadLeftButton, gamepad) : m_dpadState[gamepad].Left == ButtonState.Pressed;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButton(m_gamepadProfile.DPadRightButton, gamepad) : m_dpadState[gamepad].Right == ButtonState.Pressed;
            
            default:
                return false;
            }
        }

        public bool GetButtonDown(GamepadButton button, int gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick: return GetButtonDown(m_gamepadProfile.LeftStickButton, gamepad);
            case GamepadButton.RightStick: return GetButtonDown(m_gamepadProfile.RightStickButton, gamepad);
            case GamepadButton.LeftBumper: return GetButtonDown(m_gamepadProfile.LeftBumperButton, gamepad);
            case GamepadButton.RightBumper: return GetButtonDown(m_gamepadProfile.RightBumperButton, gamepad);
            
            case GamepadButton.Back: return GetButtonDown(m_gamepadProfile.BackButton, gamepad);
            case GamepadButton.Start: return GetButtonDown(m_gamepadProfile.StartButton, gamepad);
            case GamepadButton.ActionBottom: return GetButtonDown(m_gamepadProfile.ActionBottomButton, gamepad);
            case GamepadButton.ActionRight: return GetButtonDown(m_gamepadProfile.ActionRightButton, gamepad);
            case GamepadButton.ActionLeft: return GetButtonDown(m_gamepadProfile.ActionLeftButton, gamepad);
            case GamepadButton.ActionTop: return GetButtonDown(m_gamepadProfile.ActionTopButton, gamepad);
            
            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonDown(m_gamepadProfile.DPadUpButton, gamepad) : m_dpadState[gamepad].Up == ButtonState.JustPressed;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonDown(m_gamepadProfile.DPadDownButton, gamepad) : m_dpadState[gamepad].Down == ButtonState.JustPressed;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonDown(m_gamepadProfile.DPadLeftButton, gamepad) : m_dpadState[gamepad].Left == ButtonState.JustPressed;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonDown(m_gamepadProfile.DPadRightButton, gamepad) : m_dpadState[gamepad].Right == ButtonState.JustPressed;
            
            default:
                return false;
            }
        }

        public bool GetButtonUp(GamepadButton button, int gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick: return GetButtonUp(m_gamepadProfile.LeftStickButton, gamepad);
            case GamepadButton.RightStick: return GetButtonUp(m_gamepadProfile.RightStickButton, gamepad);
            case GamepadButton.LeftBumper: return GetButtonUp(m_gamepadProfile.LeftBumperButton, gamepad);
            case GamepadButton.RightBumper: return GetButtonUp(m_gamepadProfile.RightBumperButton, gamepad);
            
            case GamepadButton.Back: return GetButtonUp(m_gamepadProfile.BackButton, gamepad);
            case GamepadButton.Start: return GetButtonUp(m_gamepadProfile.StartButton, gamepad);
            case GamepadButton.ActionBottom: return GetButtonUp(m_gamepadProfile.ActionBottomButton, gamepad);
            case GamepadButton.ActionRight: return GetButtonUp(m_gamepadProfile.ActionRightButton, gamepad);
            case GamepadButton.ActionLeft: return GetButtonUp(m_gamepadProfile.ActionLeftButton, gamepad);
            case GamepadButton.ActionTop: return GetButtonUp(m_gamepadProfile.ActionTopButton, gamepad);

            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonUp(m_gamepadProfile.DPadUpButton, gamepad) : m_dpadState[gamepad].Up == ButtonState.JustReleased;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonUp(m_gamepadProfile.DPadDownButton, gamepad) : m_dpadState[gamepad].Down == ButtonState.JustReleased;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonUp(m_gamepadProfile.DPadLeftButton, gamepad) : m_dpadState[gamepad].Left == ButtonState.JustReleased;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ? GetButtonUp(m_gamepadProfile.DPadRightButton, gamepad) : m_dpadState[gamepad].Right == ButtonState.JustReleased;
            
            default:
                return false;
            }
        }

        bool GetButton(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKey(keyCode);
        }
        bool GetButtonDown(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKeyDown(keyCode);
        }
        bool GetButtonUp(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKeyUp(keyCode);
        }
    }
}