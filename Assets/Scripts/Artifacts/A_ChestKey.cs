using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Artifacts/Chest Key")]
public class A_ChestKey : A_Base
{
    public int numberOfCoins;

    public override void OnChestOpen()
    {
        triggered = true;

        FindObjectOfType<CoinSpawner>().CreateCoins(numberOfCoins);
    }
}
