using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {
	public static DeliveryManager Instance { get; private set; }

	public event EventHandler OnRecipeSpawned;
	public event EventHandler OnRecipeCompleted;
	public event EventHandler OnRecipeSuccess;
	public event EventHandler OnRecipeFailed;

	[SerializeField] private RecipeListSO recipeListSO;

	private List<RecipeSO> waitingRecipeSoList;
	private float spawnRecipeTimer;
	private float spawnRecipeTimerMax = 4f;
	private int waitingRecipeMax = 4;


	private void Awake() {
		Instance = this;
		waitingRecipeSoList = new List<RecipeSO>();
	}

	private void Start() {
		spawnRecipeTimer = spawnRecipeTimerMax;
	}

	private void Update() {
		spawnRecipeTimer -= Time.deltaTime;
		if(spawnRecipeTimer <= 0f) {
			spawnRecipeTimer = spawnRecipeTimerMax;

			if(waitingRecipeSoList.Count < waitingRecipeMax) {
				RecipeSO recipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
				waitingRecipeSoList.Add(recipeSO);

				Debug.Log(recipeSO.recipeName);
				OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
		for(int i = 0; i < waitingRecipeSoList.Count; i++) {
			RecipeSO recipeSO = waitingRecipeSoList[i];

			if(recipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
				// Has the same number of ingredients
				bool plateContentsMatchesRecipe = true;
				foreach(KitchenObjectSO recipeKitchenObjectSO in recipeSO.kitchenObjectSOList) {
					// Cycling through all ingredients in the Recipe
					bool isIngredientFound = false;
					foreach(KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
						// Cycling through all ingredients in the Plate
						if(plateKitchenObjectSO == recipeKitchenObjectSO) {
							// Ingredient matches!
							isIngredientFound = true;
							break;
						}
					}
					if(!isIngredientFound) {
						// This Recipe ingredient was not found on the plate
						plateContentsMatchesRecipe = false;
					}
				}

				if(plateContentsMatchesRecipe) {
					// Player delievered the correct recipe!

					waitingRecipeSoList.RemoveAt(i);

					OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
					OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
					return;
				}
			}
		}

		// No matches found!
		// Player did not deliver correct recipe
		OnRecipeFailed?.Invoke(this, EventArgs.Empty);

	}

	public List<RecipeSO> GetWaitingRecipeSOList() {
		return waitingRecipeSoList;
	}
}
