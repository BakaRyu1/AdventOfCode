using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day21 : DayRunner<Day21.Data>
{
    public struct Item(string name, int cost, int damage, int armor)
    {
        public string Name = name;
        public int Cost = cost;
        public int Damage = damage;
        public int Armor = armor;
    }
    public struct Data
    {
        public Item[] Weapons;
        public Item[] Armors;
        public Item[] Rings;
        public int BossHealth;
        public int BossDamage;
        public int BossArmor;
    }
    public override Data Parse(FileReference file)
    {
        var insideShop = true;
        var weapons = new List<Item>();
        var armors = new List<Item>();
        var rings = new List<Item>();
        List<Item>? currentList = null;
        int? bossHealth = null;
        int? bossDamage = null;
        int? bossArmor = null;
        foreach (var line in file.GetLines())
        {
            if (line.Length == 0)
                continue;
            if (insideShop)
            {
                if (line == "---")
                {
                    insideShop = false;
                    continue;
                }
                if (line.StartsWith("Weapons:"))
                {
                    currentList = weapons;
                    continue;
                }
                if (line.StartsWith("Armor:"))
                {
                    currentList = armors;
                    continue;
                }
                if (line.StartsWith("Rings:"))
                {
                    currentList = rings;
                    continue;
                }
                if (currentList == null)
                    throw new InvalidOperationException("Found item, but no list header was specified.");
                var pos = line.IndexOf("  ");
                if (pos < 0)
                    throw new InvalidOperationException($"Item is in invalid format: {line}");
                var name = line.Substring(0, pos);
                var parameters = line.AsSpan(pos).SplitInts().ToList();
                if (parameters.Count != 3)
                    throw new InvalidOperationException($"Item {name} has invalid parameters count: {parameters.Count}");
                currentList.Add(new(name, parameters[0], parameters[1], parameters[2]));
            }
            else
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
                    case "Armor":
                        if (bossArmor != null)
                            throw new InvalidOperationException("Boss Armor has already been defined.");
                        bossArmor = value;
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown boss parameter: {name}");
                }
            }
        }
        if (bossHealth == null || bossDamage == null || bossArmor == null)
            throw new InvalidOperationException("Boss has not been properly defined.");
        return new()
        {
            Weapons = [.. weapons],
            Armors = [.. armors],
            Rings = [.. rings],
            BossHealth = bossHealth.Value,
            BossDamage = bossDamage.Value,
            BossArmor = bossArmor.Value
        };
    }

    public override void Part1(Data data, RunSettings settings)
    {
        var bestCost = int.MaxValue;
        Item[] bestItems = [];
        foreach (var items in EnumerateAllSets(data))
        {
            var cost = items.Aggregate(0, (num, item) => num + item.Cost);
            if (cost >= bestCost)
                continue;
            if (TryBattle(data, items))
            {
                bestCost = cost;
                bestItems = items;
            }
        }
        Console.WriteLine($"Best cost is {bestCost}");
        Console.WriteLine($"Best items are {string.Join(", ", RemoveNullItems(bestItems).Select(item => item.Name))}");
    }

    public override void Part2(Data data, RunSettings settings)
    {
        var worstCost = -1;
        Item[] worstItems = [];
        foreach (var items in EnumerateAllSets(data))
        {
            var cost = items.Aggregate(0, (num, item) => num + item.Cost);
            if (cost < worstCost)
                continue;
            if (!TryBattle(data, items))
            {
                worstCost = cost;
                worstItems = items;
            }
        }
        Console.WriteLine($"Worst cost is {worstCost}");
        Console.WriteLine($"Worst items are {string.Join(", ", RemoveNullItems(worstItems).Select(item => item.Name))}");
    }

    public override void InitSettings(ref RunSettings settings)
    {
        settings.File1 ??= FileReference.Resource(typeof(Day21), "day21-input.txt");
    }

    private static readonly Item NULL_ITEM = new("", 0, 0, 0);
    private static readonly Item[] NULL_ITEM_LIST = [NULL_ITEM];
    private static IEnumerable<Item[]> EnumerateAllSets(Data data)
    {
        foreach (var weapon in data.Weapons)
        {
            foreach (var armor in NULL_ITEM_LIST.Concat(data.Armors))
            {
                foreach (var (i, ring1) in NULL_ITEM_LIST.Concat(data.Rings).Index())
                {
                    var otherRings = i == 0 ? NULL_ITEM_LIST : NULL_ITEM_LIST.Concat(data.Rings.Skip(i + 1));
                    foreach (var ring2 in otherRings)
                    {
                        yield return [weapon, armor, ring1, ring2];
                    }
                }
            }
        }
    }
    private static Item[] RemoveNullItems(IEnumerable<Item> items) => [.. items.Where(item => item.Cost > 0)];

    private static bool TryBattle(Data data, IEnumerable<Item> items)
    {
        var playerDamage = items.Aggregate(0, (num, item) => num + item.Damage);
        var playerArmor = items.Aggregate(0, (num, item) => num + item.Armor);
        var playerAttack = Math.Max(1, playerDamage - data.BossArmor);
        var bossAttack = Math.Max(1, data.BossDamage - playerArmor);
        var playerHealth = 100;
        var bossHealth = data.BossHealth;
        while (true)
        {
            bossHealth -= playerAttack;
            if (bossHealth <= 0)
                return true;
            playerHealth -= bossAttack;
            if (playerHealth <= 0)
                return false;
        }
    }
}
