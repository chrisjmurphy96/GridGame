namespace GridLibrary.Entities;

public struct AttackResult
{
    public bool Hit;
    public bool Crit;
    public int Damage;
    public AttackResult(bool hit, bool crit, int damage)
    {
        Hit = hit;
        Crit = crit;
        Damage = damage;
    }

    public override readonly string ToString()
    {
        return
@$"{nameof(Hit)}: {Hit}
{nameof(Crit)}: {Crit}
{nameof(Damage)}: {Damage}";
    }
}
