using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day15 : DayRunner<Day15.Ingredient[]>
{
    public class Ingredient
    {
        public string Name = "";
        public Dictionary<string, int> Traits = [];
    }
    public override Ingredient[] Parse(FileReference file)
    {
        var ingredients = new List<Ingredient>();
        foreach (var line in file.GetLines())
        {
            var pos = line.IndexOf(':');
            var name = line.Substring(0, pos);
            var traits = new Dictionary<string, int>();
            var traitsStr = line.AsSpan(pos + 1);
            foreach (var trait in traitsStr.Split(','))
            {
                var traitStr = traitsStr[trait].Trim();
                var space = traitStr.IndexOf(' ');
                traits[traitStr.Slice(0, space).ToString()] = traitStr.Slice(space + 1).ToInt();
            }
            ingredients.Add(new()
            {
                Name = name,
                Traits = traits
            });
        }
        return [.. ingredients];
    }

    private class Teaspoon(Ingredient ingredient, int amount)
    {
        public Ingredient Ingredient = ingredient;
        public int Amount = amount;
    }

    public override void Part1(Ingredient[] data, RunSettings settings)
    {
        var queue = new Queue<(Teaspoon[], int)>();
        queue.Enqueue(([], 0));
        var bestScore = 0;
        var bestRecipe = Array.Empty<Teaspoon>();
        while (queue.Count > 0)
        {
            var (recipe, ingredientIndex) = queue.Dequeue();
            var remainingSpace = 100 - recipe.Aggregate(0, (num, t) => num + t.Amount);
            var ingredient = data[ingredientIndex];
            if (ingredientIndex == (data.Length - 1))
            {
                Teaspoon[] newRecipe = [.. recipe, new Teaspoon(ingredient, remainingSpace)];
                var score = CalculateScore(newRecipe);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRecipe = newRecipe;
                }
            }
            else
            {
                for (var i = 0; i <= remainingSpace; ++i)
                    queue.Enqueue(([.. recipe, new Teaspoon(ingredient, i)], ingredientIndex + 1));
            }
        }
        Console.WriteLine($"Best score is {bestScore}");
        Console.WriteLine($"Recipe is {string.Join(", ", bestRecipe.Select(t => $"{t.Ingredient.Name} x {t.Amount}"))}");
    }

    public override void Part2(Ingredient[] data, RunSettings settings)
    {
        var queue = new Queue<(Teaspoon[], int)>();
        queue.Enqueue(([], 0));
        var bestScore = 0;
        var bestRecipe = Array.Empty<Teaspoon>();
        while (queue.Count > 0)
        {
            var (recipe, ingredientIndex) = queue.Dequeue();
            var remainingSpace = 100 - recipe.Aggregate(0, (num, t) => num + t.Amount);
            var ingredient = data[ingredientIndex];
            if (ingredientIndex == (data.Length - 1))
            {
                Teaspoon[] newRecipe = [.. recipe, new Teaspoon(ingredient, remainingSpace)];
                if (CalculateCalories(newRecipe) != 500)
                    continue;
                var score = CalculateScore(newRecipe);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestRecipe = newRecipe;
                }
            }
            else
            {
                var totalCalories = CalculateCalories(recipe);
                var ingredientCalories = ingredient.Traits.GetValueOrDefault("calories");
                for (var i = 0; i <= remainingSpace; ++i, totalCalories += ingredientCalories)
                {
                    if (totalCalories > 500)
                        break;
                    Teaspoon[] newRecipe = [.. recipe, new Teaspoon(ingredient, i)];
                    queue.Enqueue((newRecipe, ingredientIndex + 1));
                }
            }
        }
        Console.WriteLine($"Best score is {bestScore}");
        Console.WriteLine($"Recipe is {string.Join(", ", bestRecipe.Select(t => $"{t.Ingredient.Name} x {t.Amount}"))}");
    }

    private static int CalculateScore(Teaspoon[] recipe)
    {
        var scores = new Dictionary<string, int>();
        foreach (var teaspoon in recipe)
        {
            foreach (var trait in teaspoon.Ingredient.Traits)
            {
                if (trait.Key == "calories")
                    continue;
                if (!scores.ContainsKey(trait.Key))
                    scores[trait.Key] = 0;
                scores[trait.Key] += teaspoon.Amount * trait.Value;
            }
        }
        return scores.Values.Select(v => Math.Max(v, 0)).Aggregate(1, (num, v) => num * v);
    }

    private static int CalculateCalories(Teaspoon[] recipe)
    {
        return recipe.Aggregate(0, (num, t) => num + t.Ingredient.Traits.GetValueOrDefault("calories") * t.Amount);
    }
}
