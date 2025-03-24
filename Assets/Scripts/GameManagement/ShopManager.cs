using benjohnson;
using UnityEngine;
using System.Collections.Generic; // Required for LinkedList

public class ShopManager : Singleton<ShopManager>
{
    // Components
    private ArrangeGrid gridLayout;

    [SerializeField] private GameObject shopItemPrefab;

    protected override void Awake()
    {
        base.Awake();
        gridLayout = GetComponent<ArrangeGrid>();
    }

    void Start()
    {
        if (GameManager.instance.stage >= 4)
        {
            GameManager.instance.LoadWinScreen();
            return; // Prevents unnecessary shop loading
        }

        LoadShop();
    }

    private void LoadShop()
    {
        if (shopItemPrefab == null || ArtifactManager.instance == null) return;

        // Using LinkedList to store artifacts
        LinkedList<A_Base> artifacts = new LinkedList<A_Base>(ArtifactManager.instance.GetRandomArtifacts(6));

        foreach (A_Base artifact in artifacts)
        {
            Instantiate(shopItemPrefab, transform)?.GetComponent<ShopItem>()?.Visualize(artifact);
        }

        gridLayout?.Arrange();
        ReloadPrices();
    }

    public void ExitShop()
    {
        GameManager.instance?.LoadNextStage();
    }

    public void ReloadPrices()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<ShopItem>()?.UpdateCounter(Player.instance?.Wallet.money ?? 0);
        }
    }
}
