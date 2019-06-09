
using UnityEngine;
namespace CustomInputManager.Examples
{
	public class GamepadToggle : MonoBehaviour 
	{
		[SerializeField] private string m_keyboardScheme = null;
		[SerializeField] private string m_gamepadScheme = null;
		[SerializeField] [Range(-1, InputManager.maxPlayers)] int playerID = -1;
		
		void SetSchemeIfNotTarget(string target, string current, int player) {
			if (current != target) {
				InputManager.SetControlScheme(target, player);
			}
		}

		void CheckPlayerGamepadToggle(int player) {
			ControlScheme scheme = InputManager.GetControlScheme(player);

			if (scheme != null) {
				if (InputManager.Gamepad.GamepadIsConnected(player)) {
					SetSchemeIfNotTarget(m_gamepadScheme, scheme.Name, player);
				}
				else {
					SetSchemeIfNotTarget(m_keyboardScheme, scheme.Name, player);
				}
			}
		}

		void Update () {
			if (playerID == -1) {
				for (int i = 0; i < InputManager.maxPlayers; i++) {
					CheckPlayerGamepadToggle(i);	
				}
			}
			else {
				CheckPlayerGamepadToggle(playerID);
			}
		}
	}
}