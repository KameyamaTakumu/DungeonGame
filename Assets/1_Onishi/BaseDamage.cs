using UnityEngine;

public class BaseDamage
{
    public static BaseDamage instance;

    public void Damage(int amount, int hp)
    {
        hp -= amount;

        if (hp < 0)
        {
            hp = 0;
        }
    }
}
