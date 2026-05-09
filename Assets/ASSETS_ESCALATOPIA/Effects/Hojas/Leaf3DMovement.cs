using UnityEngine;

public class Leaf3DMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float fallSpeed = 1.5f;

    public float xAmplitude = 1f;
    public float xFrequency = 2f;

    public float zAmplitude = 0.5f;
    public float zFrequency = 1.5f;

    [Header("Rotación")]
    public float rotationAmplitude = 30f;
    public float rotationFrequency = 2f;

    public float spinSpeed = 50f;

    private float timeOffset;
    private Vector3 currentPosition;

    void Start()
    {
        currentPosition = transform.position;

        timeOffset = Random.Range(0f, 100f);

        // Variación aleatoria
        xAmplitude *= Random.Range(0.7f, 1.3f);
        zAmplitude *= Random.Range(0.7f, 1.3f);

        fallSpeed *= Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        float time = Time.time + timeOffset;

        // Caída
        currentPosition.y -= fallSpeed * Time.deltaTime;

        // Zig-zag
        currentPosition.x += Mathf.Sin(time * xFrequency) * xAmplitude * Time.deltaTime;

        // Movimiento profundidad
        currentPosition.z += Mathf.Cos(time * zFrequency) * zAmplitude * Time.deltaTime;

        transform.position = currentPosition;

        // Balanceo hoja
        float tiltX = Mathf.Sin(time * rotationFrequency) * rotationAmplitude;

        float tiltZ = Mathf.Cos(time * rotationFrequency) * rotationAmplitude;

        // Spin continuo
        transform.rotation = Quaternion.Euler(
            tiltX,
            time * spinSpeed,
            tiltZ
        );
    }
}