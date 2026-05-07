using UnityEngine;

public class InteraccionJugador : MonoBehaviour
{
    [SerializeField] private float distanciaRay = 3f;
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private LayerMask capaInteractuable;

    private void Start()
    {
        if (camaraJugador == null)
            camaraJugador = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, distanciaRay, capaInteractuable))
            {
                CofreTimeline cofre = hit.collider.GetComponent<CofreTimeline>();

                if (cofre != null)
                {
                    cofre.Interactuar();
                }
            }
        }
    }
}