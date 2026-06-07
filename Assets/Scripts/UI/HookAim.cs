using UnityEngine;
using DG.Tweening;

public class HookAim : MonoBehaviour
{
    [Header("=== SETTINGS ===")]
    [Tooltip("Si está activo, la mira siempre estará visible en el centro cuando no haya HookPoint.")]
    public bool alwaysShowReticle = true;

    public GrapplingHook grapplingHook;
    public RectTransform miraRectTransform; // Asignar el RectTransform de la mira (punteia)
    private Vector2 centerPosition;
    private Tween miraTween;
    private Vector3 miraOriginalScale;

    void Start()
    {
        if (miraRectTransform != null)
        {
            centerPosition = miraRectTransform.anchoredPosition;
            miraOriginalScale = miraRectTransform.localScale;
        }
    }

    void Update()
    {
        bool targeting = grapplingHook != null && grapplingHook.CurrentHookPoint != null && !grapplingHook.IsActive;
        if (targeting)
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
            // Efecto de agrandar y achicar la mira continuamente
            if (miraTween == null || !miraTween.IsActive())
            {
                // Restablecemos la escala original por si estaba oculta (Vector3.zero)
                miraRectTransform.localScale = miraOriginalScale;
                miraTween = miraRectTransform.DOScale(miraOriginalScale * 1.4f, 0.3f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        else
        {
            miraRectTransform.anchoredPosition = centerPosition;
            // Detener efecto
            if (miraTween != null)
            {
                miraTween.Kill();
                miraTween = null;
            }

            if (alwaysShowReticle)
            {
                // Mantener la mira visible en el centro cuando no hay HookPoint válido.
                miraRectTransform.localScale = miraOriginalScale;
            }
            else
            {
                miraRectTransform.localScale = Vector3.zero;
            }
        }
    }
}
