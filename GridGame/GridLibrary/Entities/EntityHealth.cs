namespace GridLibrary.Entities;

public class EntityHealth(int maxHealth)
{
    public int MaxHealth { get; } = maxHealth;
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

    public override string ToString()
    {
        return $"Max: {MaxHealth}, Current: {CurrentHealth}";
    }
}