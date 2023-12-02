using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IKitchenObjectParent {

	public static Player Instance { get; private set; }

	public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
	public class OnSelectedCounterChangedEventArgs : EventArgs {
		public BaseCounter selectedCounter;
	}
	public event EventHandler OnPickedSomething;

	[SerializeField] private float moveSpeed = 7f;
	[SerializeField] private GameInput gameInput;
	[SerializeField] private LayerMask counterLayerMask;
	[SerializeField] private Transform kitchenObjectHoldPoint;

	private bool isWalking = false;
	private Vector3 lastInteractDir;
	private BaseCounter selectedCounter;
	private KitchenObject kitchenObject;

	private void Awake() {
		Instance = this;
	}

	private void Start() {
		gameInput.OnInteractAction += GameInput_OnInteractAction;
		gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
	}

	private void GameInput_OnInteractAlternateAction(object sender, EventArgs e) {
		if(!GameManager.Instance.IsGamePlaying()) { return; }

		if(selectedCounter != null) {
			selectedCounter.InteractAlternate(this);
		}
	}

	private void GameInput_OnInteractAction(object sender, System.EventArgs e) {
		if(!GameManager.Instance.IsGamePlaying()) { return; }

		if(selectedCounter != null) {
			selectedCounter.Interact(this);
		}
	}

	private void Update() {
		HandleMovement();
		HandleInteractions();

	}

	public bool IsWalking() {
		return isWalking;
	}

	private void HandleMovement() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

		float moveDistance = moveSpeed * Time.deltaTime;
		float playerHeight = 2f;
		float playerRadius = .7f;
		bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

		if(!canMove) {
			// Cannot move towards moveDir

			// Attempt only X movement
			Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
			canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

			if(canMove) {
				// Can move only on the X
				moveDir = moveDirX;
			}
			else {
				// Cannot move only on X

				// Attemt only on Z
				Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
				canMove = moveDir.z != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);
				if(canMove) {
					moveDir = moveDirZ;
				}
				else {
					// Cannot move in any direction
				}
			}
		}

		if(canMove) {
			transform.position += moveDir * moveDistance;
		}


		isWalking = moveDir != Vector3.zero;

		float rotateSpeed = 10f;
		transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
	}

	private void HandleInteractions() {
		Vector2 inputVector = gameInput.GetMovementVectorNormalized();
		Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

		if(moveDir != Vector3.zero) {
			lastInteractDir = moveDir;
		}

		float interactDistance = 2f;
		if(Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, counterLayerMask)) {
			if(raycastHit.transform.TryGetComponent<BaseCounter>(out BaseCounter baseCounter)) {
				// Has ClearCounter
				if(baseCounter != selectedCounter) {
					SetSelectedCounter(baseCounter);
				}
			}
			else {
				SetSelectedCounter(null);
			}
		}
		else {
			SetSelectedCounter(null);
		}
	}

	private void SetSelectedCounter(BaseCounter selectedCounter) {
		this.selectedCounter = selectedCounter;

		OnSelectedCounterChanged?.Invoke(this, new OnSelectedCounterChangedEventArgs {
			selectedCounter = selectedCounter
		});
	}

	public Transform GetKitchenObjectFollowTransform() {
		return kitchenObjectHoldPoint;
	}

	public void SetKitchenObject(KitchenObject kitchenObject) {
		this.kitchenObject = kitchenObject;

		if(kitchenObject != null) {
			OnPickedSomething?.Invoke(this, EventArgs.Empty);
		}
	}

	public KitchenObject GetKitchenObject() {
		return kitchenObject;
	}

	public void ClearKitchenObject() {
		kitchenObject = null;
	}

	public bool HasKitchenObject() {
		return kitchenObject != null;
	}
}
