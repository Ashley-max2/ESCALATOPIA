using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Escalatopia.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIView : MonoBehaviour
    {
        [Header("View Configuration")]
        public string viewId;
        public bool startHidden = true;
        public bool blocksInput = false;
        
        [Header("Animation")]
        public float fadeDuration = 0.3f;

        public UnityEvent onOpen;
        public UnityEvent onClose;

        protected CanvasGroup canvasGroup;
        protected bool isOpen;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (string.IsNullOrEmpty(viewId))
                viewId = GetType().Name;
        }

        protected virtual void Start()
        {
            if (startHidden)
            {
                CloseImmediate();
            }
            else
            {
                OpenImmediate();
            }
        }

        public virtual void Open()
        {
            if (isOpen) return;
            isOpen = true;
            gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(Fade(0, 1));
            onOpen?.Invoke();
        }

        public virtual void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            StopAllCoroutines();
            StartCoroutine(Fade(1, 0, () => gameObject.SetActive(false)));
            onClose?.Invoke();
        }

        public void OpenImmediate()
        {
            isOpen = true;
            gameObject.SetActive(true);
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            onOpen?.Invoke();
        }

        public void CloseImmediate()
        {
            isOpen = false;
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            onClose?.Invoke();
        }

        protected IEnumerator Fade(float start, float end, System.Action onComplete = null)
        {
            float elapsed = 0f;
            canvasGroup.alpha = start;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = end;
            canvasGroup.interactable = end > 0;
            canvasGroup.blocksRaycasts = end > 0;
            
            onComplete?.Invoke();
        }
    }
}
