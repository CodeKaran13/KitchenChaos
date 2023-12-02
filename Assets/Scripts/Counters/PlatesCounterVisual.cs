using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatesCounterVisual : MonoBehaviour {
	
	[SerializeField] private PlatesCounter platesCounter;
	[SerializeField] private Transform counterTopPoint;
	[SerializeField] private Transform plateVisualPrefab;

	private List<GameObject> plateVisualGameObjectList;
	private float plateOffsetY = 0.1f;

	private void Awake() {
		plateVisualGameObjectList = new List<GameObject>();
	}

	private void Start() {
		platesCounter.OnPlatesSpawned += PlatesCounter_OnPlatesSpawned;
		platesCounter.OnPlatesRemoved += PlatesCounter_OnPlatesRemoved;
	}

	private void PlatesCounter_OnPlatesRemoved(object sender, System.EventArgs e) {
		GameObject plate = plateVisualGameObjectList[plateVisualGameObjectList.Count - 1];
		plateVisualGameObjectList.Remove(plate);

		Destroy(plate);
	}

	private void PlatesCounter_OnPlatesSpawned(object sender, System.EventArgs e) {
		Transform plateVisualTransform = Instantiate(plateVisualPrefab, counterTopPoint);

		plateVisualTransform.localPosition = new Vector3(0, plateOffsetY * plateVisualGameObjectList.Count, 0);
		
		plateVisualGameObjectList.Add(plateVisualTransform.gameObject);
	}
}
