using UnityEngine;
using UnityEngine.UI;

namespace KIdentify.UI {
	public class CatchGame : MonoBehaviour {
		public enum GameState {
			Waiting,
			Start,
			End
		}

		public AgeGateMiniGameUI ageGateMiniGameUI;
		public GameObject fallingObjectPrefab;
		public Canvas canvas;
		public RectTransform canvasRectTransform;
		private readonly float gameDuration = 10f; // Game duration in seconds
		private float gameTimer = 0f;
		private GameState gameState = GameState.Waiting;

		private float timer = 0f;
		public float minSpawnInterval = 0.0f;
		public float maxSpawnInterval = 0.5f;

		private float topY;
		private float leftX;
		private float rightX;

		public void StartMiniGame() {
			Initialize();
			ResetGame();
			gameState = GameState.Start;
		}

		private void Initialize() {
			if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown) {
				topY = canvasRectTransform.rect.yMax;
			}
			else {
				topY = canvasRectTransform.rect.yMax;
			}

			leftX = canvasRectTransform.rect.xMin + 100f;
			rightX = canvasRectTransform.rect.xMax - 100f;
		}

		private void Update() {
			if (gameState == GameState.Start) {
				// Count down the timer
				timer += Time.deltaTime;
				gameTimer += Time.deltaTime;

				// Spawn a falling object at random intervals
				if (timer >= Random.Range(minSpawnInterval, maxSpawnInterval)) // Adjust the probability as needed
				{
					SpawnFallingObject();
					timer = 0f;
				}

				// Check if the game duration is reached
				if (gameTimer >= gameDuration) {
					// Game over, you can add your logic here (e.g., show a message)
					Debug.Log("Game Over!");
					// Optionally, you can restart the game
					// ResetGame();
					gameState = GameState.End;
					ageGateMiniGameUI.OnGameOver();
				}

				// Handle input for moving the player on the canvas
				//HandleInput();
			}
		}

		private void SpawnFallingObject() {
			// Instantiate a falling object on the canvas
			GameObject fallingObject = Instantiate(fallingObjectPrefab, canvas.transform);
			// Attach a click handler to the image
			Button imageButton = fallingObject.GetComponent<Button>();
			if (imageButton != null) {
				imageButton.onClick.AddListener(() => {
					ageGateMiniGameUI.IncrementScore();
					Destroy(fallingObject);
				});
			}

			RectTransform rectTransform = fallingObject.GetComponent<RectTransform>();

			// Set the initial position of the falling object
			rectTransform.anchoredPosition = new Vector2(Random.Range(leftX, rightX), topY);

			// Set the falling object's speed
			fallingObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0f, Random.Range(-300f, -500f));
		}

		private void ResetGame() {
			// Reset the timer and any other necessary variables
			gameTimer = 0f;
			// You may want to reset the player's position or any other aspects
		}
	}
}