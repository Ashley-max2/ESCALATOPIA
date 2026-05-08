using UnityEngine;

public class HookAim : MonoBehaviour
{
    public GrapplingHook grapplingHook;
    public RectTransform miraRectTransform; // Asignar el RectTransform de la mira (punteia)
    private Vector2 centerPosition;

    void Start()
    {
        if (miraRectTransform != null)
        {
            centerPosition = miraRectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (grapplingHook != null && grapplingHook.CurrentHookPoint != null && !grapplingHook.IsActive)
        {
            // Calcular posición en pantalla del target
            Vector3 screenPos = Camera.main.WorldToScreenPoint(grapplingHook.CurrentHookPoint.position);
            if (screenPos.z > 0) // Delante de la cámara
            {
                // Convertir a posición local en el canvas
                Vector2 canvasPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    miraRectTransform.parent as RectTransform,
                    screenPos,
                    null, // Para Screen Space Overlay
                    out canvasPos
                );
                miraRectTransform.anchoredPosition = canvasPos;
            }
            else
            {
                miraRectTransform.anchoredPosition = centerPosition;
            }
        }
        else
        {
            miraRectTransform.anchoredPosition = centerPosition;
        }
    }
}
