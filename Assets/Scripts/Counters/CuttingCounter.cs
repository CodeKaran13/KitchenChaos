using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttingCounter : BaseCounter, IHasProgress {

	public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;

	public event EventHandler OnCut;

	[SerializeField] private CuttingRecipeSO[] cuttingRecipeSOsArray;

	private int cuttingProgress;

	public override void Interact(Player player) {
		if(!HasKitchenObject()) {
			// There is no kitchen object here
			if(player.HasKitchenObject()) {
				// Player is carrying something
				if(HasRecipeWithInput(player.GetKitchenObject().GetKitchenObjectSO())) {
					// Player carrying something that can be cut
					player.GetKitchenObject().SetKitchenObjectParent(this);
					cuttingProgress = 0;

					CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
					OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
						progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMAX
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
					}
				}
			}
			else {
				// Player not carrying anything
				GetKitchenObject().SetKitchenObjectParent(player);
			}
		}
	}

	public override void InteractAlternate(Player player) {
		if(HasKitchenObject() && HasRecipeWithInput(GetKitchenObject().GetKitchenObjectSO())) {
			// There is kitchen object here AND it can be cut
			cuttingProgress++;
			OnCut?.Invoke(this, EventArgs.Empty);

			CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(GetKitchenObject().GetKitchenObjectSO());
			OnProgressChanged?.Invoke(this, new IHasProgress.OnProgressChangedEventArgs {
				progressNormalized = (float)cuttingProgress / cuttingRecipeSO.cuttingProgressMAX
			});

			if(cuttingProgress >= cuttingRecipeSO.cuttingProgressMAX) {
				KitchenObjectSO kitchenObjectSO = GetOutputForInput(GetKitchenObject().GetKitchenObjectSO());

				GetKitchenObject().DestroySelf();

				KitchenObject.SpawnKitchenObject(kitchenObjectSO, this);
			}
		}
	}

	private bool HasRecipeWithInput(KitchenObjectSO inputKitchenObjectSO) {
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		return cuttingRecipeSO != null;
	}

	private KitchenObjectSO GetOutputForInput(KitchenObjectSO inputKitchenObjectSO) {
		CuttingRecipeSO cuttingRecipeSO = GetCuttingRecipeSOWithInput(inputKitchenObjectSO);
		if(cuttingRecipeSO != null) {
			return cuttingRecipeSO.output;
		}
		else {
			return null;
		}
	}

	private CuttingRecipeSO GetCuttingRecipeSOWithInput(KitchenObjectSO inputKitchenObjectSO) {
		foreach(CuttingRecipeSO cuttingRecipeSO in cuttingRecipeSOsArray) {
			if(cuttingRecipeSO.input == inputKitchenObjectSO) {
				return cuttingRecipeSO;
			}
		}
		return null;
	}
}