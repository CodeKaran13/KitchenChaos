using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour {

	public static GameInput Instance { get; private set; }

	private const string PLAYER_PREFS_BINDINGS = "InputBindings";

	public event EventHandler OnInteractAction;
	public event EventHandler OnInteractAlternateAction;
	public event EventHandler OnPauseAction;
	public event EventHandler OnBindingRebind;

	public enum Binding {
		Move_Up,
		Move_Down,
		Move_Left,
		Move_Right,
		Interact,
		InteractAlternate,
		Pause,
	}

	private GameControls gameControls;

	

	private void Awake() {
		Instance = this;
		gameControls = new GameControls();

		if(PlayerPrefs.HasKey(PLAYER_PREFS_BINDINGS)) {
			gameControls.LoadBindingOverridesFromJson(PlayerPrefs.GetString(PLAYER_PREFS_BINDINGS));
		}

		gameControls.Player.Enable();

		gameControls.Player.Interact.performed += Interact_Performed;
		gameControls.Player.InteractAlternate.performed += InteractAlternate_Performed;
		gameControls.Player.Pause.performed += Pause_Performed;
	}

	private void OnDestroy() {
		gameControls.Player.Interact.performed -= Interact_Performed;
		gameControls.Player.InteractAlternate.performed -= InteractAlternate_Performed;
		gameControls.Player.Pause.performed -= Pause_Performed;

		gameControls.Dispose();
	}

	private void Pause_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		OnPauseAction?.Invoke(this, EventArgs.Empty);
	}

	private void InteractAlternate_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
	}

	private void Interact_Performed(UnityEngine.InputSystem.InputAction.CallbackContext obj) {
		OnInteractAction?.Invoke(this, EventArgs.Empty);
	}

	public Vector2 GetMovementVectorNormalized() {
		if(gameControls == null) {
			return Vector2.zero;
		}

		Vector2 inputVector = gameControls.Player.Move.ReadValue<Vector2>();

		inputVector = inputVector.normalized;
		return inputVector;
	}

	public string GetBindingText(Binding binding) {
		switch(binding) {
			default:
			case Binding.Move_Up:
				return gameControls.Player.Move.bindings[1].ToDisplayString();
			case Binding.Move_Down:
				return gameControls.Player.Move.bindings[2].ToDisplayString();
			case Binding.Move_Left:
				return gameControls.Player.Move.bindings[3].ToDisplayString();
			case Binding.Move_Right:
				return gameControls.Player.Move.bindings[4].ToDisplayString();
			case Binding.Interact:
				return gameControls.Player.Interact.bindings[0].ToDisplayString();
			case Binding.InteractAlternate:
				return gameControls.Player.InteractAlternate.bindings[0].ToDisplayString();
			case Binding.Pause:
				return gameControls.Player.Pause.bindings[0].ToDisplayString();
		}
	}

	public void RebindBinding(Binding binding, Action onActionRebound) {
		gameControls.Player.Disable();

		InputAction inputAction;
		int bindingIndex;

		switch(binding) {
			default:
			case Binding.Move_Up:
				inputAction = gameControls.Player.Move;
				bindingIndex = 1;
				break;
			case Binding.Move_Down:
				inputAction = gameControls.Player.Move;
				bindingIndex = 2;
				break;
			case Binding.Move_Left:
				inputAction = gameControls.Player.Move;
				bindingIndex = 3;
				break;
			case Binding.Move_Right:
				inputAction = gameControls.Player.Move;
				bindingIndex = 4;
				break;
			case Binding.Interact:
				inputAction = gameControls.Player.Interact;
				bindingIndex = 0;
				break;
			case Binding.InteractAlternate:
				inputAction = gameControls.Player.InteractAlternate;
				bindingIndex = 0;
				break;
			case Binding.Pause:
				inputAction = gameControls.Player.Pause;
				bindingIndex = 0;
				break;
		}

		inputAction.PerformInteractiveRebinding(bindingIndex)
			.OnComplete(callback => {
				callback.Dispose();
				gameControls.Player.Enable();
				onActionRebound();

				PlayerPrefs.SetString(PLAYER_PREFS_BINDINGS, gameControls.SaveBindingOverridesAsJson());
				PlayerPrefs.Save();

				OnBindingRebind?.Invoke(this, EventArgs.Empty);
			})
			.Start();
	}
}
