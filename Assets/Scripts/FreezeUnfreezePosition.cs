using UnityEngine;

public class FreezeUnfreezePosition : MonoBehaviour
{
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void FreezePosition()
    {
        if (rb == null) return;

        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezePositionY |
                         RigidbodyConstraints.FreezePositionZ;
    }

    public void UnfreezePosition()
    {
        if (rb == null) return;

        rb.constraints = RigidbodyConstraints.None;
    }
}