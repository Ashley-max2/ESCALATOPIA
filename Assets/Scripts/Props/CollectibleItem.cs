using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Componente simple para un objeto que el jugador puede recoger con la tecla E.
/// Mantiene un booleano público que indica si el objeto ya fue recogido.
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    [Header("Item")]
    public string itemName = "Objeto de la montaña";

    [Header("Pickup")]
    [Tooltip("Tecla para recoger el objeto cuando el jugador está cerca.")]
    public KeyCode pickupKey = KeyCode.E;

    [Header("Events")]
    public UnityEvent onCollected;

    private bool isCollected = false;
    private bool playerInside = false;

    public bool IsCollected => isCollected;

    private void Update()
    {
        if (!playerInside || isCollected) return;

        if (Input.GetKeyDown(pickupKey))
        {
            Collect();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
    }

    public void Collect()
    {
        if (isCollected) return;

        isCollected = true;
        onCollected?.Invoke();
        gameObject.SetActive(false);
    }
}
