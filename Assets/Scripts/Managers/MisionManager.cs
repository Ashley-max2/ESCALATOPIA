using UnityEngine;
using DG.Tweening;

public class MisionManager : MonoBehaviour
{
    [Header("Panel de Misiones")]
    public GameObject missionPanel;
    [Header("Animación")]
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;
    private bool openClosePanel;

    void Start()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
            canvasGroup = missionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = missionPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
        }
    }

    
    public void ObjectCollected()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, fadeDuration);
            Debug.Log("Objeto recogido → Panel de misiones abierto");
        }
    }
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (missionPanel != null)
            {
                if (missionPanel.activeSelf)
                {
                    // Cerrar con fade out
                    canvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
                    {
                        missionPanel.SetActive(false);
                    });
                    Debug.Log("Panel CERRADO");
                }
                else
                {
                    // Abrir con fade in
                    missionPanel.SetActive(true);
                    canvasGroup.alpha = 0f;
                    canvasGroup.DOFade(1f, fadeDuration);
                    Debug.Log("Panel ABIERTO");
                }
            }
        }
    }

}