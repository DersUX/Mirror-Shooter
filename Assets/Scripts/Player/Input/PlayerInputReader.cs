using UnityEngine;
using UnityEngine.InputSystem;
using ShooterMirror.Player.Input.Generated;

namespace ShooterMirror.Player.Input
{
	[DisallowMultipleComponent]
	public class PlayerInputReader : MonoBehaviour
	{
		private PlayerControls _controls;
		private PlayerControls.PlayerActions _player;

		public bool HasControl => Application.isFocused && Cursor.lockState == CursorLockMode.Locked;

		private void Awake()
		{
			_controls = new PlayerControls();
			_player = _controls.Player;
		}

		private void OnEnable()
		{
			_player.Enable();
			SetCursorCaptured(true);
		}

		private void OnDisable()
		{
			_controls?.Disable();

			SetCursorCaptured(false);
		}

		private void OnDestroy()
		{
			_controls?.Dispose();
		}

		public PlayerInputSnapshot Read()
		{
			if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
			{
				SetCursorCaptured(false);
			}
			else if (Mouse.current?.leftButton.wasPressedThisFrame == true && Cursor.lockState != CursorLockMode.Locked)
			{
				SetCursorCaptured(true);
			}

			if (!HasControl)
			{
				return default;
			}

			return new PlayerInputSnapshot(_player.Move.ReadValue<Vector2>(),
				_player.Jump.WasPressedThisFrame(),
				_player.Crouch.IsPressed(),
				_player.Crouch.WasPressedThisFrame(),
				_player.Look.ReadValue<Vector2>());
		}

		private static void SetCursorCaptured(bool captured)
		{
			Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !captured;
		}
	}
}