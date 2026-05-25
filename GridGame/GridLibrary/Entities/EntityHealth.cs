namespace GridLibrary.Entities;

public class EntityHealth(int maxHealth)
{
    public int MaxHealth { get; private set; } = maxHealth;
    public int CurrentHealth { get; private set; } = maxHealth;
    public bool IsDead => CurrentHealth is 0;

    public void Add(int healthPoints)
    {
        CurrentHealth += healthPoints;
        if (CurrentHealth > MaxHealth)
            CurrentHealth = MaxHealth;
    }

    public void Subtract(int healthPoints)
    {
        CurrentHealth -= healthPoints;
        if (CurrentHealth < 0)
            CurrentHealth = 0;
    }

    public void SetMaxAndCurrent(int value)
    {
        MaxHealth = value;
        CurrentHealth = value;
    }

    public override string ToString()
    {
        return $"Max: {MaxHealth}, Current: {CurrentHealth}";
    }
}