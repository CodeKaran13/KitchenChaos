using UnityEngine;

public class RPMPlayerAnimator : MonoBehaviour {
	//private const string IS_WALKING = "IsWalking";

	private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
	private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

	private Player player;
	private Animator animator;

	private void Awake() {
		player = transform.parent.GetComponent<Player>();
		animator = GetComponent<Animator>();
	}

	private void OnEnable() {
		player.OnPickedSomething += Player_OnPickedSomething;
		player.OnDroppedSomething += Player_OnDroppedSomething;
	}

	private void OnDisable() {
		player.OnPickedSomething -= Player_OnPickedSomething;
		player.OnDroppedSomething -= Player_OnDroppedSomething;
	}

	private void Player_OnPickedSomething(object sender, System.EventArgs e) {
		if (!GameManager.Instance.IsGamePlaying()) { return; }

		animator.SetTrigger("Carry");
	}

	private void Player_OnDroppedSomething(object sender, System.EventArgs e) {
		if (!GameManager.Instance.IsGamePlaying()) { return; }

		animator.SetTrigger("Carry");
	}

	private void Update() {
		if (player.IsWalking()) {
			animator.SetFloat(MoveSpeedHash, player.MoveSpeed);
		}
		else {
			animator.SetFloat(MoveSpeedHash, 0);
		}
	}

	public void Setup(RuntimeAnimatorController runtimeAnimatorController) {
		if (animator == null) {
			return;
		}

		animator.runtimeAnimatorController = runtimeAnimatorController;
		animator.applyRootMotion = false;
		animator.SetBool(IsGroundedHash, true);
	}
}
