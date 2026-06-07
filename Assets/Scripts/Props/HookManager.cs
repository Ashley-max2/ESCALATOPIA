using UnityEngine;
using FMODUnity;

public class HookManager : MonoBehaviour
{
    [SerializeField] private HaveHookManager haveHookManager;

    [Header("Audio")]
    [SerializeField] private EventReference pickupSound;

    public MeshRenderer[] meshes;
    public GameObject objectToDelete1;
    public GameObject objectToDelete2;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hacer invisibles todas las meshes
            foreach (MeshRenderer mesh in meshes)
            {
                mesh.enabled = false;
            }

            // Borrar los otros dos objetos
            Destroy(objectToDelete1);
            Destroy(objectToDelete2);

            // Sonido de recogida
            if (!pickupSound.IsNull)
                RuntimeManager.PlayOneShot(pickupSound, transform.position);

            // Activar gancho
            haveHookManager.SetHasHook(true);
        }
    }
}