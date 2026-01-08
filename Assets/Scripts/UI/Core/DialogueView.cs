using UnityEngine;
using TMPro;

namespace Escalatopia.UI
{
    public class DialogueView : UIView
    {
        [Header("References")]
        public TextMeshProUGUI messageText;
        public GameObject container;

        public void ShowMessage(string message)
        {
            if (messageText != null)
                messageText.text = message;
            
            Open();
        }

        public void HideMessage()
        {
            Close();
        }
    }
}
