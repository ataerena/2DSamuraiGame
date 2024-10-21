public abstract class Weapon
{
    protected virtual int Damage { get; set; } = 10;
    protected virtual int MaxDurability { get; set; } = 10;
    protected int durability;
    protected bool isDisabled = false;
    void Start()
    {
        durability = MaxDurability;
    }

    public virtual int GetDamage()
    {
        return Damage;
    }

    protected virtual int GetDurability()
    {
        return durability;
    }

    protected virtual void LoseDurability(int lossValue)
    {
        if (isDisabled) return;

        if (durability > 0) {
            durability -= lossValue;
        } else {
            isDisabled = true;
        }
    }

    protected virtual void RestoreDurability()
    {
        isDisabled = false;
        durability = MaxDurability;
    }
}
