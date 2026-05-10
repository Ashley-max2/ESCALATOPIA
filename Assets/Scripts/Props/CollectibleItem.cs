using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente para objetos que se recogen automáticamente al entrar en el trigger.
/// Sin interacción manual requerida.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Objeto de la montaña";

    [Header("Events")]
    public UnityEvent onCollected;

    private bool isCollected = false;

    public bool IsCollected => isCollected;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Recoger automáticamente
        if (!isCollected)
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;
        onCollected?.Invoke();
        gameObject.SetActive(false);
    }
}
