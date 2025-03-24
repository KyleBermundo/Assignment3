using benjohnson;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int money;
    private bool infiniteMoneyEnabled = false; // Cheat flag

    // Extra coins when picking up money
    [HideInInspector] public int extraCoins;

    // UI Components
    [SerializeField] private Counter counter;
    private ScaleAnimator anim;

    private const int MaxMoney = int.MaxValue; // Maximum possible money

    void Awake()
    {
        extraCoins = 0;
        anim = counter?.transform.parent.GetComponent<ScaleAnimator>(); // Avoid potential null reference
        UpdateCounter();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            EnableInfiniteMoney();
        }
    }

    private void UpdateCounter()
    {
        if (counter == null) return;

        counter.SetText(money.ToString(), 3);
        ShopManager.instance?.ReloadPrices(); // Shorter null check
    }

    public void AddMoney(int value, int count = 1)
    {
        int totalValue = value + extraCoins * count;
        if (infiniteMoneyEnabled || totalValue <= 0) return;

        money += totalValue;
        PlayerStats.instance.coinsCollected += totalValue;

        if (Player.instance == null) return;

        UpdateCounter();
        anim?.SetScale(new Vector2(1.5f, 1.5f)); // Safe null check
    }

    public void Buy(int cost)
    {
        if (!infiniteMoneyEnabled)
        {
            money -= cost; // Only subtract money if the cheat is OFF
        }

        UpdateCounter();
        anim?.SetScale(new Vector2(1.5f, 1.5f));
        SoundManager.instance?.PlaySound("Buy"); // Safe null check
    }

    public void EnableInfiniteMoney()
    {
        infiniteMoneyEnabled = true;
        money = MaxMoney;
        UpdateCounter();
        Debug.Log("Infinite money activated!");
    }
}
