using UnityEngine;
using UnityEngine.Playables;

public class ActivadorTimelineInteractivo : MonoBehaviour
{
    [SerializeField] private GameObject timelineObject;
    [SerializeField] private float distanciaRay = 3f;

    private Camera camaraJugador;

    private void Start()
    {
        camaraJugador = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(camaraJugador.transform.position, camaraJugador.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, distanciaRay))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (LlaveRecogible.llaveRecogida)
                    {
                        PlayableDirector director = timelineObject.GetComponent<PlayableDirector>();

                        if (director != null)
                        {
                            director.Play();
                            Debug.Log("Timeline activada");
                        }
                    }
                    else
                    {
                        Debug.Log("Necesitas la llave");
                    }
                }
            }
        }
    }
}