using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FruitImage : MonoBehaviour {
	public Sprite[] spriteArray;

	[SerializeField] private Image image;

	private void OnEnable() {
		int randomImageIndex = Random.Range(0, spriteArray.Length);
		image.sprite = spriteArray[randomImageIndex];
	}
}
