using UnityEngine;

namespace CustomInputManager
{
    [CreateAssetMenu(fileName = "New Gamepad Profile", menuName = "CustomInputManager/Input Manager/Gamepad Profile")]
    public class GenericGamepadProfile : ScriptableObject
    {
        [SerializeField] private GamepadDPadType m_dpadType = GamepadDPadType.Axis;
        
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_leftStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_rightStickButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_leftBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_rightBumperButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadUpButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadDownButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_dpadRightButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_backButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_startButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionTopButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionBottomButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionLeftButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)] [SerializeField] private int m_actionRightButton = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightStickXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightStickYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_dpadXAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_dpadYAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_leftTriggerAxis = 0;
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)] [SerializeField] private int m_rightTriggerAxis = 0;

        public GamepadDPadType DPadType => m_dpadType;
        public int LeftStickButton => m_leftStickButton;
        public int RightStickButton => m_rightStickButton;
        public int LeftBumperButton => m_leftBumperButton;
        public int RightBumperButton => m_rightBumperButton;
        public int DPadUpButton => m_dpadUpButton;
        public int DPadDownButton => m_dpadDownButton;
        public int DPadLeftButton => m_dpadLeftButton;
        public int DPadRightButton => m_dpadRightButton;
        public int BackButton => m_backButton;
        public int StartButton => m_startButton;
        public int ActionTopButton => m_actionTopButton;
        public int ActionBottomButton => m_actionBottomButton;
        public int ActionLeftButton => m_actionLeftButton;
        public int ActionRightButton => m_actionRightButton;
        public int LeftStickXAxis => m_leftStickXAxis;
        public int LeftStickYAxis => m_leftStickYAxis;
        public int RightStickXAxis => m_rightStickXAxis;
        public int RightStickYAxis => m_rightStickYAxis;
        public int DPadXAxis => m_dpadXAxis;
        public int DPadYAxis => m_dpadYAxis;
        public int LeftTriggerAxis => m_leftTriggerAxis;
        public int RightTriggerAxis => m_rightTriggerAxis;
    }
}
