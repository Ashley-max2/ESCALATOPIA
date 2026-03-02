using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Oculta una imagen cuando CUALQUIERA de los paneles esta activo.
/// Cuando todos estan desactivados, la imagen vuelve a verse.
/// </summary>
public class HideImageOnPanel : MonoBehaviour
{
    [Tooltip("La imagen que se ocultara")]
    [SerializeField] private Image imageToHide;

    [Tooltip("Si cualquiera de estos paneles esta activo, la imagen se oculta")]
    [SerializeField] private GameObject[] panelsToWatch;

    private void Update()
    {
        if (imageToHide == null || panelsToWatch == null) return;

        bool anyActive = false;
        foreach (var panel in panelsToWatch)
        {
            if (panel != null && panel.activeInHierarchy)
            {
                anyActive = true;
                break;
            }
        }

        imageToHide.enabled = !anyActive;
    }
}
