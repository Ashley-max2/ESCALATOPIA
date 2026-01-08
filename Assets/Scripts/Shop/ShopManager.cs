using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    public List<Gadget> availableGadgets = new List<Gadget>();
    public int currentCurrency = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public bool PurchaseGadget(Gadget gadget)
    {
        if (gadget.isUnlocked) return true; // Already owned

        if (currentCurrency >= gadget.cost)
        {
            currentCurrency -= gadget.cost;
            gadget.isUnlocked = true;
            Debug.Log($"Purchased {gadget.displayName}");
            return true;
        }
        else
        {
            Debug.Log("Not enough currency!");
            return false;
        }
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
    }
}
