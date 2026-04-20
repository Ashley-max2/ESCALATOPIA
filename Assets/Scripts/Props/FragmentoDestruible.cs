using UnityEngine;
using System.Collections;

public class FragmentoDestruible : MonoBehaviour
{
    [Header("Vida")]
    public float tiempoVidaTotal = 3f;
    public float duracionFade = 1f;

    [Header("Fuerza")]
    public float fuerzaHorizontal = 6f;
    public float fuerzaVertical = 3f;
    public float fuerzaRandom = 2f;

    private Rigidbody rb;
    private Renderer rend;
    private Material mat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    public void Lanzar(Vector3 centro)
    {
        rb.isKinematic = false;

        // dirección hacia fuera del barril
        Vector3 dir = (transform.position - centro).normalized;

        // impulso fuerte + hacia abajo
        Vector3 fuerza =
            dir * fuerzaHorizontal +
            Vector3.up * fuerzaVertical +
            Vector3.down * 2f +
            Random.insideUnitSphere * fuerzaRandom;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(fuerza, ForceMode.Impulse);

        StartCoroutine(CicloVida());
    }

    IEnumerator CicloVida()
    {
        float espera = tiempoVidaTotal - duracionFade;
        yield return new WaitForSeconds(espera);

        Color c = mat.color;
        float t = 0f;

        while (t < duracionFade)
        {
            float a = Mathf.Lerp(1f, 0f, t / duracionFade);
            mat.color = new Color(c.r, c.g, c.b, a);

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}