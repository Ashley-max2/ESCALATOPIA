using UnityEngine;

public class AutomaticDoubleDoor : MonoBehaviour
{
    [Header("Referencias")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Configuración")]
    [Tooltip("Distancia que se abre cada puerta")]
    public float openDistance = 1.8f;

    [Tooltip("Velocidad de movimiento")]
    public float speed = 3f;

    [Header("Dirección de apertura")]
    [Tooltip("Dirección en la que se abre la puerta izquierda (usa los ejes locales del Empty)")]
    public Vector3 openDirection = new Vector3(0, -1, 0);   // Cambia esto según necesites

    [Tooltip("Tag del jugador")]
    public string playerTag = "Player";

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;
    private bool playerInside = false;

    private void Start()
    {
        if (leftDoor == null || rightDoor == null)
        {
            Debug.LogError("¡Asigna Left Door y Right Door en el inspector!");
            return;
        }

        leftClosedPos = leftDoor.localPosition;
        rightClosedPos = rightDoor.localPosition;

        // Calculamos las posiciones abiertas usando la dirección que tú definas
        leftOpenPos = leftClosedPos + openDirection * openDistance;
        rightOpenPos = rightClosedPos - openDirection * openDistance;  // La derecha va en sentido contrario
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            OpenDoors();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            CloseDoors();
        }
    }

    private void OpenDoors()
    {
        if (!isOpen)
        {
            isOpen = true;
            StopAllCoroutines();
            StartCoroutine(MoveDoors(leftOpenPos, rightOpenPos));
        }
    }

    private void CloseDoors()
    {
        if (isOpen && !playerInside)
        {
            isOpen = false;
            StopAllCoroutines();
            StartCoroutine(MoveDoors(leftClosedPos, rightClosedPos));
        }
    }

    private System.Collections.IEnumerator MoveDoors(Vector3 targetLeft, Vector3 targetRight)
    {
        while (true)
        {
            float step = speed * Time.deltaTime;

            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, targetLeft, step);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, targetRight, step);

            if (Vector3.Distance(leftDoor.localPosition, targetLeft) < 0.01f &&
                Vector3.Distance(rightDoor.localPosition, targetRight) < 0.01f)
            {
                break;
            }

            yield return null;
        }
    }

    public void ForceOpen() => OpenDoors();
    public void ForceClose() => CloseDoors();
}