using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CuttingCounter;

public class StoveCounter : BaseCounter, IHasProgress {

	public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
	public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

	public class OnStateChangedEventArgs: EventArgs {
		public State state;
	}

	public enum State {
		Idle,
		Frying,
		Fried,
		Burned
	}

	[SerializeField] private FryingRecipeSO[] fryingRecipeSOsArray;
	[SerializeField] private BurningRecipeSO[] burningRecipeSOArray;

	private float fryingTimer;
	private FryingRecipeSO fryingRecipeSO;
	private State state;
	private float burningTimer;
	private BurningRecipeSO burningRecipeSO;

	private void Start() {
		state = State.Idle;
	}

	private void Update() {
		if(HasKitchenObject()) {
			switch(state) {
				case State.Idle:
					break;
				case State.Frying:
					fryingTimer += Time.deltaTime;
					
					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
						progressNormalized = fryingTimer / fryingRecipeSO.fryingTimerMax,
					});
					
					if(fryingTimer >= fryingRecipeSO.fryingTimerMax) {
						// Fried

						GetKitchenObject().DestroySelf();

						KitchenObject.SpawnKitchenObject(fryingRecipeSO.output, this);

						state = State.Fried;
						burningRecipeSO = GetBurningRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
						burningTimer = 0;

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});
					}
					break;
				case State.Fried:
					burningTimer += Time.deltaTime;
					
					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
						progressNormalized = burningTimer / burningRecipeSO.burningTimerMax,
					});
					
					if(burningTimer >= burningRecipeSO.burningTimerMax) {
						// Burned

						GetKitchenObject().DestroySelf();

						KitchenObject.SpawnKitchenObject(burningRecipeSO.output, this);

						state = State.Burned;

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});

						OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
							progressNormalized = 0f
						});
					}
					break;
				case State.Burned:
					break;
			}
		}
	}

	public override void Interact(Player player) {
		if(!HasKitchenObject()) {
			// There is no kitchen object here
			if(player.HasKitchenObject()) {
				// Player is carrying something
				if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					// Player carrying something that can be fried
					player.GetKitchenObject().SetKitchenObjectParent(this);

					fryingRecipeSO = GetFryingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());

					state = State.Frying;
					fryingTimer = 0f;

					OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
						state = state
					});

					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
						progressNormalized = fryingTimer
					});
				}
			}
			else {
				// Player not carrying anything
			}
		}
		else {
			// There is kitchen object here
			if(player.HasKitchenObject()) {
				// Player is carrying something
				if(player.GetKitchenObject().TryGetPlate(out PlateKitchenObject plateKitchenObject)) {
					// Player is holding a plate
					if(plateKitchenObject.TryAddIngredient(GetKitchenObject().GetKitchenObjectSO())) {
						GetKitchenObject().DestroySelf();

						state = State.Idle;

						OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
							state = state
						});

						OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
							progressNormalized = 0f
						});
					}
				}
			}
			else {
				// Player not carrying anything
				GetKitchenObject().SetKitchenObjectParent(player);
				state = State.Idle;

				OnStateChanged?.Invoke(this, new OnStateChangedEventArgs {
					state = state
				});

				OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
					progressNormalized = 0f
				});
			}
		}
	}

	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		return fryingRecipeSO != null;
	}

	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
		FryingRecipeSO fryingRecipeSO = GetFryingRecipeSOWithInput(inputKitchenObjectSO);
		if(fryingRecipeSO != null) {
			return fryingRecipeSO.output;
		}
		else {
			return null;
		}
	}

	private FryingRecipeSO GetFryingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach(FryingRecipeSO fryingRecipeSO in fryingRecipeSOsArray) {
			if(fryingRecipeSO.input == inputKitchenObjectSO) {
				return fryingRecipeSO;
			}
		}
		return null;
	}

	private BurningRecipeSO GetBurningRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach(BurningRecipeSO burningRecipeSO in burningRecipeSOArray) {
			if(burningRecipeSO.input == inputKitchenObjectSO) {
				return burningRecipeSO;
			}
		}
		return null;
	}
}
