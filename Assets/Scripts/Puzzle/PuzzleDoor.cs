using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class PuzzleDoor : MonoBehaviour
{
    [Header("Apertura de puerta")]
    public Vector3 openDirection = Vector3.right;
    public float openDistance = 5f;
    public float openDuration = 1f;

    [Header("Audio")]
    [EventRef] public string openDoorEvent = "event:/Ambient/OpenDoor";

    private bool isOpen = false;

    public void OpenDoor()
    {
        if (isOpen)
            return;

        Vector3 targetPosition = transform.position + openDirection.normalized * openDistance;
        transform.DOMove(targetPosition, openDuration).SetEase(Ease.OutSine);
        RuntimeManager.PlayOneShot(openDoorEvent, transform.position);
        isOpen = true;
        Debug.Log("Puerta del puzzle abierta.");
    }
}
