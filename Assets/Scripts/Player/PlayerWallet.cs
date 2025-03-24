using benjohnson;
using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    public int money;
    private bool infiniteMoneyEnabled = false; // Flag for infinite money

    // Variables
    [HideInInspector] public int extraCoins; // When picking up coins, also add extra coins

    // Components
    [SerializeField] Counter counter;
    ScaleAnimator anim;

    void Awake()
    {
        extraCoins = 0;
        anim = counter.transform.parent.GetComponent<ScaleAnimator>();
        UpdateCounter();
    }

    void Update()
    {
        // Press "I" to activate infinite money
        if (Input.GetKeyDown(KeyCode.I))
        {
            EnableInfiniteMoney();
        }
    }

    void UpdateCounter()
    {
        if (counter == null) return;
        counter.SetText(money.ToString(), 3);
        if (ShopManager.instance != null)
            ShopManager.instance.ReloadPrices();
    }

    public void AddMoney(int value, int count = 1)
    {
        if (infiniteMoneyEnabled) return; // Prevent modifying money if cheat is on

        value += extraCoins * count;
        if (value <= 0) return;

        money += value;
        PlayerStats.instance.coinsCollected += value;

        if (Player.instance == null) return;
        UpdateCounter();
        anim.SetScale(new Vector2(1.5f, 1.5f));
    }

    public void Buy(int cost)
    {
        if (!infiniteMoneyEnabled)
        {
            money -= cost; // Only subtract money if cheat is OFF
        }

        UpdateCounter();
        anim.SetScale(new Vector2(1.5f, 1.5f));
        SoundManager.instance.PlaySound("Buy");
    }

    // Cheat Code: Infinite Money
    public void EnableInfiniteMoney()
    {
        infiniteMoneyEnabled = true;
        money = int.MaxValue;
        UpdateCounter();
        Debug.Log("Infinite money activated!");
    }
}
