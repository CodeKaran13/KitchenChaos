using ReadyPlayerMe.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RenderPanel : MonoBehaviour {

	[SerializeField] private PlayerSelectionUI playerSelectionUI;

	[SerializeField] private string url = "";
	[SerializeField] private string avatarName = "Karan";
	[SerializeField] private AvatarRenderSettings avatarRenderSettings;
	[SerializeField] private GameObject panel;
	[SerializeField] private Image avatarImage;
	[SerializeField] private TextMeshProUGUI nameText;

	[SerializeField] private Image panelImage;
	[SerializeField] private Color selectedColor;
	[SerializeField] private Color deselectedColor;

	public bool IsReady { get; private set; }

	public string Url {
		get { return url; }
	}

	private void Start() {
		var avatarRenderLoader = new AvatarRenderLoader {
			OnCompleted = SetImage
		};

		var texture = playerSelectionUI.GetRenderForAvatar(url);

		if (texture != null) {
			SetImage(texture);
		}
		else {
			Debug.Log($"Loading player with url: {url}");
			avatarRenderLoader.LoadRender(url, avatarRenderSettings);
		}

	}

	public void ShowPanel() {
		panel.SetActive(true);
	}

	public void HidePanel() {
		panel.SetActive(false);
	}

	public void SetSelected() {
		panelImage.color = selectedColor;
	}

	public void SetDeselected() {
		panelImage.color = deselectedColor;
	}

	private void SetImage(Texture2D texture) {
		Debug.Log($"Finish loading player with url: {url}");
		playerSelectionUI.SaveAvatarRender(this, texture);

		var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f));

		avatarImage.sprite = sprite;
		avatarImage.preserveAspect = true;
		nameText.text = avatarName;

		IsReady = true;
		playerSelectionUI.PlayersLoaded++;
	}

	#region BUTTON ONCLICK

	public void OnClick(int index) {
		playerSelectionUI.SelectPlayer(index);
	}

	#endregion
}
