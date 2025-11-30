using AdventOfCode.Utils;

namespace AdventOfCode._2015;
internal class Day11 : DayRunner<string>
{
    public override string Parse(FileReference file)
    {
        return file.GetText();
    }

    public override void Part1(string data, RunSettings settings)
    {
        char[] password = [.. data];
        NextValidPassword(password);
        Console.WriteLine($"Next password is {string.Join("", password)}");
    }

    public override void Part2(string data, RunSettings settings)
    {
        char[] password = [.. data];
        NextValidPassword(password);
        NextValidPassword(password);
        Console.WriteLine($"Next next password is {string.Join("", password)}");
    }

    private static void NextValidPassword(char[] password)
    {
        do
        {
            NextPassword(password);
        } while (!(!HasForbiddenCharacter(password) && HasIncrementingThreeLetters(password) && HasTwoPairs(password)));
    }

    private static void NextPassword(char[] password)
    {
        if (HasForbiddenCharacter(password))
        {
            do
            {
                var pos = password.Index().First(tuple => FORBIDDEN_CHARACTERS.Contains(tuple.Item)).Index;
                NextPassword(password, pos);
                for (var i = pos + 1; i < password.Length; ++i)
                    password[i] = 'a';
            } while (HasForbiddenCharacter(password));
            return;
        }
        NextPassword(password, password.Length - 1);
    }

    private static void NextPassword(char[] password, int index)
    {
        if (index < 0)
            return;
        var ch = ++password[index];
        if (ch > 'z')
        {
            password[index] = 'a';
            NextPassword(password, index - 1);
        }
    }

    private static bool HasIncrementingThreeLetters(char[] password)
        => password.Zip(password.Skip(1)).Zip(password.Skip(2))
            .Select(tuple => (a: tuple.First.First, b: tuple.First.Second, c: tuple.Second))
            .Any(tuple => (tuple.a + 1) == tuple.b && (tuple.b + 1) == tuple.c);

    private static readonly HashSet<char> FORBIDDEN_CHARACTERS = ['i', 'o', 'l'];

    private static bool HasForbiddenCharacter(char[] password)
        => password.Any(FORBIDDEN_CHARACTERS.Contains);

    private static bool HasTwoPairs(char[] password)
        => password.Zip(password.Skip(1)).Index()
            .Any(tuple => tuple.Item.First == tuple.Item.Second 
                        && password.Skip(tuple.Index + 2).Zip(password.Skip(tuple.Index + 3))
                                .Any(tuple2 => tuple2.First != tuple.Item.First && tuple2.First == tuple2.Second));
}
