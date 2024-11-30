using UnityEngine;

[CreateAssetMenu(menuName = "Artifacts/Ancient Flask")]
public class A_AncientFlask : A_Base
{
    public int healthIncrease;

    public override void OnBossDefeated()
    {
        triggered = true;

        Player.instance.Health.IncreaseHealth(healthIncrease);
    }
}
