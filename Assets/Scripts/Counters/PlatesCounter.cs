using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounter : BaseCounter {

	public event EventHandler OnPlatesSpawned;
	public event EventHandler OnPlatesRemoved;

	[SerializeField] private KitchenObjectSO kitchenObjectSO;

	private float spawnPlateTimer;
	private float spawnPlateTimerMax = 4f;
	private int platesSpawnedAmount;
	private int platesSpawnedAmountMax = 4;

	private void Update() {
		spawnPlateTimer += Time.deltaTime;
		if(spawnPlateTimer > spawnPlateTimerMax) {
			spawnPlateTimer = 0;

			if(GameManager.Instance.IsGamePlaying() && platesSpawnedAmount < platesSpawnedAmountMax) {
				platesSpawnedAmount++;

				OnPlatesSpawned?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public override void Interact(Player player) {
		if(!player.HasKitchenObject()) {
			// Player is empty handed
			if(platesSpawnedAmount > 0) {
				platesSpawnedAmount--;

				KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);
				OnPlatesRemoved?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}
