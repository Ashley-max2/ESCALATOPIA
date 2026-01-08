using UnityEngine;
using Escalatopia.UI;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea]
    public string message = "Tutorial Message";
    public bool oneShot = true;
    public float duration = 4f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneShot) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            ShowMessage();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!oneShot && other.CompareTag("Player"))
        {
            HideMessage();
        }
    }

    private void ShowMessage()
    {
        DialogueView view = UIManager.Instance?.GetView<DialogueView>();
        if (view != null) // Typed lookup
        {
            view.ShowMessage(message);
            if (duration > 0)
                Invoke(nameof(HideMessage), duration);
        }
        else
        {
            // Fallback string lookup
            UIView v = UIManager.Instance?.GetView("Dialogue_View");
            if (v is DialogueView dv)
            {
                dv.ShowMessage(message);
                if (duration > 0)
                    Invoke(nameof(HideMessage), duration);
            }
        }
    }

    private void HideMessage()
    {
        DialogueView view = UIManager.Instance?.GetView<DialogueView>();
        if (view != null)
        {
            view.HideMessage();
        }
        else
        {
             UIView v = UIManager.Instance?.GetView("Dialogue_View");
             if (v != null) v.Close();
        }
    }
}
