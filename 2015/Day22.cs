using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day22 : DayRunner<Day22.Data>
{
    public struct Data
    {
        public int BossHealth;
        public int BossDamage;
    }

    public override Data Parse(FileReference file)
    {
        int? bossHealth = null;
        int? bossDamage = null;
        foreach (var line in file.GetLines())
        {
            var pos = line.IndexOf(':');
            if (pos < 0)
                throw new InvalidOperationException($"Invalid boss data format: {line}");
            var name = line.Substring(0, pos);
            var value = line.AsSpan(pos + 1).ToInt();
            switch (name)
            {
                case "Hit Points":
                    if (bossHealth != null)
                        throw new InvalidOperationException("Boss Hit Points has already been defined.");
                    bossHealth = value;
                    break;
                case "Damage":
                    if (bossDamage != null)
                        throw new InvalidOperationException("Boss Damage has already been defined.");
                    bossDamage = value;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown boss parameter: {name}");
            }
        }
        if (bossHealth == null || bossDamage == null)
            throw new InvalidOperationException("Boss has not been properly defined.");
        return new()
        {
            BossHealth = bossHealth.Value,
            BossDamage = bossDamage.Value
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var (bestManaConsumed, bestSpells) = FindBestSpells(data, false);
        Console.WriteLine($"Best mana consumption is {bestManaConsumed}");
        Console.WriteLine($"Best spells are {string.Join(", ", bestSpells.Select(type => SPELLS[type].Name))}");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var (bestManaConsumed, bestSpells) = FindBestSpells(data, true);
        Console.WriteLine($"Best mana consumption with health drain is {bestManaConsumed}");
        Console.WriteLine($"Best spells are {string.Join(", ", bestSpells.Select(type => SPELLS[type].Name))}");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day22), "day22-input.txt");
    }

    private enum SpellType
    {
        MagicMissile,
        Drain,
        Shield,
        Poison,
        Recharge
    }
    private class BattleState
    {
        public int PlayerHealth;
        public int PlayerMana;
        public int PlayerArmor = 0;
        public int BossHealth;
        public int TotalManaConsumed = 0;
        public Dictionary<SpellType, int> Effects = [];

        public BattleState(int playerHealth, int playerMana, int bossHealth)
        {
            PlayerHealth = playerHealth;
            PlayerMana = playerMana;
            BossHealth = bossHealth;
        }

        public BattleState(BattleState source)
        {
            PlayerHealth = source.PlayerHealth;
            PlayerMana = source.PlayerMana;
            PlayerArmor = source.PlayerArmor;
            BossHealth = source.BossHealth;
            TotalManaConsumed = source.TotalManaConsumed;
            foreach (var (key, value) in source.Effects)
                Effects[key] = value; 
        }

        public override bool Equals(object? obj)
        {
            if (obj is not BattleState other)
                return false;
            return PlayerHealth == other.PlayerHealth
                && PlayerMana == other.PlayerMana
                && PlayerArmor == other.PlayerArmor
                && BossHealth == other.BossHealth
                && TotalManaConsumed == other.TotalManaConsumed
                && Effects.Count == other.Effects.Count
                && !Effects.Except(other.Effects).Any();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PlayerHealth, PlayerMana, PlayerArmor, BossHealth, TotalManaConsumed, Effects.GetHashCode());
        }
    }
    private class SpellInfo(SpellType type, string name, int cost, int duration, Action<BattleState> action)
    {
        public SpellType Type = type;
        public string Name = name;
        public int Cost = cost;
        public int Duration = duration;
        public Action<BattleState> Action = action;
    }

    private static readonly Dictionary<SpellType, SpellInfo> SPELLS = ((List<SpellInfo>)[
        new(SpellType.MagicMissile, "Magic Missile", 53, 0, state => state.BossHealth -= 4),
        new(SpellType.Drain, "Drain", 73, 0, state => { state.BossHealth -= 2; state.PlayerHealth += 2; }),
        new(SpellType.Shield, "Shield", 113, 6, state => state.PlayerArmor = 7),
        new(SpellType.Poison, "Poison", 173, 6, state => state.BossHealth -= 3),
        new(SpellType.Recharge, "Recharge", 229, 5, state => state.PlayerMana += 101)
    ]).ToDictionary(item => item.Type);

    private static void ApplyEffects(BattleState state)
    {
        state.PlayerArmor = 0;
        foreach (var type in state.Effects.Keys.ToArray())
        {
            var spell = SPELLS[type];
            spell.Action(state);
            --state.Effects[type];
            if (state.Effects[type] <= 0)
                state.Effects.Remove(type);
        }
    }

    private static IEnumerable<(SpellType, BattleState)> GenerateStateForSpells(BattleState state)
    {
        foreach (var spell in SPELLS.Values)
        {
            if (spell.Cost > state.PlayerMana)
                continue;
            if (spell.Duration > 0)
            {
                if (state.Effects.ContainsKey(spell.Type))
                    continue;
                var newState = new BattleState(state);
                newState.Effects[spell.Type] = spell.Duration;
                newState.PlayerMana -= spell.Cost;
                newState.TotalManaConsumed += spell.Cost;
                yield return (spell.Type, newState);
            }
            else
            {
                var newState = new BattleState(state);
                newState.PlayerMana -= spell.Cost;
                newState.TotalManaConsumed += spell.Cost;
                spell.Action(newState);
                yield return (spell.Type, newState);
            }
        }
    }

    private static (int, SpellType[]) FindBestSpells(Data data, bool withHealthDrain)
    {
        var initialState = new BattleState(50, 500, data.BossHealth);
        var visited = new HashSet<BattleState>();
        var queue = new Queue<(BattleState, SpellType[])>();
        var bestManaConsumed = int.MaxValue;
        SpellType[] bestSpells = [];
        queue.Enqueue((initialState, []));
        while (queue.Count > 0)
        {
            var (state, spells) = queue.Dequeue();
            if (state.TotalManaConsumed > bestManaConsumed)
                continue;
            if (!visited.Add(state))
                continue;
            // Player turn
            if (withHealthDrain)
            {
                --state.PlayerHealth;
                if (state.PlayerHealth <= 0)
                    continue;
            }
            ApplyEffects(state);
            if (state.BossHealth <= 0)
            {
                if (state.TotalManaConsumed < bestManaConsumed)
                {
                    bestManaConsumed = state.TotalManaConsumed;
                    bestSpells = spells;
                }
                continue;
            }
            foreach (var (spell, newState) in GenerateStateForSpells(state))
            {
                if (newState.BossHealth <= 0)
                {
                    if (newState.TotalManaConsumed < bestManaConsumed)
                    {
                        bestManaConsumed = newState.TotalManaConsumed;
                        bestSpells = [.. spells, spell];
                    }
                    continue;
                }
                else if (newState.PlayerHealth <= 0)
                    continue;

                // Boss turn
                ApplyEffects(newState);
                if (newState.BossHealth <= 0)
                {
                    if (newState.TotalManaConsumed < bestManaConsumed)
                    {
                        bestManaConsumed = newState.TotalManaConsumed;
                        bestSpells = [.. spells, spell];
                    }
                    continue;
                }
                var damage = Math.Max(1, data.BossDamage - newState.PlayerArmor);
                newState.PlayerHealth -= damage;
                if (newState.PlayerHealth <= 0)
                    continue;

                queue.Enqueue((newState, [.. spells, spell]));
            }
        }
        return (bestManaConsumed, bestSpells);
    }
}
