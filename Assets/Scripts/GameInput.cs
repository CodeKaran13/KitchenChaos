using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {

	public event EventHandler OnInteractAction;
	public event EventHandler OnInteractAlternateAction;

	private GameControls gameControls;



	private void Awake() {
		gameControls = new GameControls();
		gameControls.Player.Enable();

		gameControls.Player.Interact.performed += Interact_Performed;
		gameControls.Player.InteractAlternate.performed += InteractAlternate_Performed;
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
}
