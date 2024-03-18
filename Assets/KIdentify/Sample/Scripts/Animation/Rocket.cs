using System.Collections;
using UnityEngine;

namespace KIdentify.Sample.Animation
{
	public class Rocket : MonoBehaviour
	{
		[SerializeField] private Animator animator;
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Vector2 startPosition;
		[SerializeField] private Vector2 endPosition;

		readonly float lerpDuration = 1.5f;
		private readonly string ROCKET_PLAY = "IsPlaying";

		public void PlayAnimation()
		{
			ResetPosition();
			animator.SetBool(ROCKET_PLAY, true);
			StartCoroutine(MoveRocket());
		}

		IEnumerator MoveRocket()
		{
			float timeElapsed = 0;
			while (timeElapsed < lerpDuration)
			{
				rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, timeElapsed / lerpDuration);
				timeElapsed += Time.deltaTime;
				yield return null;
			}
			rectTransform.anchoredPosition = endPosition;
			StopAnimation();
			ResetPosition();
		}

		private void StopAnimation()
		{
			animator.SetBool(ROCKET_PLAY, false);
			AnimationManager.Instance.FinishAnimation();
		}

		private void ResetPosition()
		{
			rectTransform.anchoredPosition = startPosition;
		}
	}
}