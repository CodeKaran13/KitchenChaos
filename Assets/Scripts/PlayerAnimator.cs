using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour {

	private const string IS_WALKING = "IsWalking";

	[SerializeField] private Player player;

	private Animator animator;

	private void Awake() {
		animator = GetComponent<Animator>();
	}

	private void Update() {
		if(player.IsWalking()) {
			animator.SetBool(IS_WALKING, true);
		}
		else {
			animator.SetBool(IS_WALKING, false);
		}
	}
}
