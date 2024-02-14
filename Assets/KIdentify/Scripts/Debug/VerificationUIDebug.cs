using UnityEngine;
using UnityEngine.EventSystems;

public class VerificationUISlug : MonoBehaviour
{
    public GameObject verificationUI;
    public GameObject successUI;

    private bool isDoubleClick = false;
    private float doubleClickTimeThreshold = 0.3f;
    private float lastClickTime = 0f;

    private void Start()
    {
        // Attach an event trigger to the verification UI game object
        if (!verificationUI.TryGetComponent<EventTrigger>(out var eventTrigger))
        {
            eventTrigger = verificationUI.AddComponent<EventTrigger>();
        }

        // Add a pointer click event to the event trigger
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener(OnVerificationClick);
        eventTrigger.triggers.Add(clickEntry);
    }

    private void OnVerificationClick(BaseEventData eventData)
    {
        Slug.LogT2(this, "Verification UI clicked");
        if (isDoubleClick)
        {
            // Perform the double-click action (switch to success UI)
            SwitchToUI(successUI);
        }
        else
        {
            // Check if it's a double-click within the time threshold
            if (Time.time - lastClickTime <= doubleClickTimeThreshold)
            {
                isDoubleClick = true;
                lastClickTime = 0f;
            }
            else
            {
                lastClickTime = Time.time;
            }
        }
    }

    private void SwitchToUI(GameObject ui)
    {
        // Disable the verification UI
        verificationUI.SetActive(false);

        // Enable the success UI
        ui.SetActive(true);
    }
}

