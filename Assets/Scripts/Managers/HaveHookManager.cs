using UnityEngine;

public class HaveHookManager : MonoBehaviour
{
    [SerializeField] private GameObject[] hookObjects;

    private bool hasHook = false;

    private void Start()
    {
        SetObjectsActive(false);
    }

    public void SetHasHook(bool value)
    {
        hasHook = value;
        SetObjectsActive(hasHook);
    }

    private void SetObjectsActive(bool active)
    {
        foreach (GameObject obj in hookObjects)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }
}