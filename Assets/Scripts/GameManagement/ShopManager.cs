using benjohnson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : Singleton<ShopManager>
{
    // Components
    ArrangeGrid gridLayout;

    [SerializeField] GameObject shopItemPrefab;

    protected override void Awake()
    {
        base.Awake();

        gridLayout = GetComponent<ArrangeGrid>();


    }
    void Start()
    {
        if (GameManager.instance.stage >= 4)
            GameManager.instance.LoadWinScreen();
        LoadShop();
    }

    void LoadShop()
    {
        LinkedList<A_Base> artifacts = new LinkedList<A_Base>(ArtifactManager.instance.GetRandomArtifacts(6)); //  Changed to LinkedList
        foreach (A_Base a in artifacts)
        {
            Instantiate(shopItemPrefab, transform).GetComponent<ShopItem>().Visualize(a);
        }
        gridLayout.Arrange();
        ReloadPrices();
    }

    public void ExitShop()
    {
        GameManager.instance.LoadNextStage();
    }

    public void ReloadPrices()
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<ShopItem>().UpdateCounter(Player.instance.Wallet.money);
        }
    }

 
}
