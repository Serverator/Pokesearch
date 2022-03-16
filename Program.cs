using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using static PokemonData;
using static WordleGuesser;
using static Guess;
using static Maths;
using System;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;

class WordleGuesser
{
    public static PokemonData[] PokemonDB;
    public static PokemonSearch PokeSearch;
    //public static List<Guess> Guesses = new List<Guess>();

    public static void Main(string[] args)
    {
        
        PokemonDB = JsonSerializer.Deserialize<PokemonData[]>(PokeDB.database, new JsonSerializerOptions { IncludeFields = true });
        PokeSearch = new();

        #region
        /*
        //Calculatus maximus
        for (int i = 0; i < PokemonDB.Length; i++)
        {
            Console.Clear();
            Console.WriteLine(((float)(i + 1) / PokemonDB.Length * 100).ToString("F2"));

            var guesser = PokemonDB[i];

            List<Guess> guesses = GetListFromGuesser(guesser, PokemonDB);

            double firstStepEntropy = 0;
            double secondStepEntropy = 0;
            for (int j = 0; j < guesses.Count; j++)
            {
                var pokemons = PokemonSearch.Filter(guesses[j], PokemonDB).ToArray();
                firstStepEntropy += ToEntropy(PokemonDB.Length, pokemons.Count())*((double)guesses[j].GuessId/PokemonDB.Length);

                for (int k = 0; k < PokemonDB.Length; k++)
                {
                    var guesser2 = PokemonDB[k];
                    List<Guess> guesses2 = GetListFromGuesser(guesser2, pokemons);
                    double entropySum = 0;
                    for (int l = 0; l < guesses2.Count; l++)
                    {
                        int pokemons2 = PokemonSearch.Filter(guesses2[l], pokemons).Count();
                        entropySum += ToEntropy(pokemons.Length, pokemons2) * ((double)guesses2[l].GuessId / pokemons.Length);
                    }
                    if (entropySum > secondStepEntropy)
                        secondStepEntropy = entropySum;
                }
            }

            //firstStepInfo /= PokemonDB.Length;
            PokemonDB[i].Information = (float)firstStepEntropy;
            PokemonDB[i].InformationDouble = (float)secondStepEntropy;
        }

        string json = JsonSerializer.Serialize<PokemonData[]>(PokemonDB, new JsonSerializerOptions { IncludeFields = true });
        using (StreamWriter sw = File.CreateText(@"X:\PokeDB2.json"))
            sw.Write(json);
        */
        #endregion

        while (true)
        {
            Guess.MakeGuess();
        }
    }
}
[Serializable]
public class PokemonData : IComparable<PokemonData>
{
    public enum PokemonType { Normal, Fire, Water, Grass, Electric, Ice, Fighting, Poison, Ground, Flying, Psychic, Bug, Rock, Ghost, Dark, Dragon, Steel, Fairy, None };
    public string Name;
    public byte Generation;
    public PokemonType MainType;
    public PokemonType SubType;
    public float Height;
    public float Weight;
    public float Information = -1;
    public float InformationDouble = -1;

    public PokemonData()
    {

    }

    public PokemonData(string Data)
    {
        string[] Datas = Data.Split(',');
        Name = Datas[0];
        Generation = byte.Parse(Datas[1]);
        MainType = (PokemonType)Enum.Parse(typeof(PokemonType), Datas[2]);
        SubType = String.IsNullOrWhiteSpace(Datas[3]) ? PokemonType.None : (PokemonType)Enum.Parse(typeof(PokemonType), Datas[3]);
        Height = float.Parse(Datas[4], CultureInfo.InvariantCulture);
        Weight = float.Parse(Datas[5], CultureInfo.InvariantCulture);
    }

    public int CompareTo(PokemonData obj)
    {
        if (PokeSearch.GuessesList.Count == 0)
        {
            int comparer1 = -(Information+InformationDouble).CompareTo(obj.Information+obj.InformationDouble);
            if (comparer1 != 0)
                return comparer1;
        }
        int comparer = -this.Information.CompareTo(obj.Information);
        if (comparer != 0)
            return comparer;
        comparer = (PokeSearch.FilteredPokemons.Contains(this) ? -1 : 0) + (PokeSearch.FilteredPokemons.Contains(obj) ? 1 : 0);
        if (comparer != 0)
            return comparer;
        comparer = Generation.CompareTo(obj.Generation);
        if (comparer != 0)
            return comparer;
        return Name.CompareTo(obj.Name);
    }

    public override string ToString()
    {
        return Name;
    }
}

public class PokemonSearch
{
    // Not Inclusive
    (byte Min, byte? Exact, byte Max) Generation = new(byte.MinValue, null, byte.MaxValue);
    // Lookup
    bool[] MainType = new bool[Enum.GetNames(typeof(PokemonType)).Length];
    bool[] SubType = new bool[Enum.GetNames(typeof(PokemonType)).Length];
    // Not inclusive
    (float Min, float? Exact, float Max) Height = new(float.MinValue, null, float.MaxValue);
    (float Min, float? Exact, float Max) Weight = new(float.MinValue, null, float.MaxValue);

    public List<Guess> GuessesList = new List<Guess>();

    public PokemonSearch()
    {
        for (int i = 0; i < Enum.GetNames(typeof(PokemonType)).Length; i++)
        {
            MainType[i] = true;
            SubType[i] = true;
        }
        Filter();
    }

    public PokemonData[] FilteredPokemons;

    private void Filter()
    {
        FilteredPokemons = PokemonDB
            .Where(x => Generation.Exact is not null ? x.Generation == Generation.Exact : x.Generation > Generation.Min && x.Generation < Generation.Max)
            .Where(x => MainType[(int)x.MainType])
            .Where(x => SubType[(int)x.SubType])
            .Where(x => Height.Exact is not null ? x.Height == Height.Exact : x.Height > Height.Min && x.Height < Height.Max)
            .Where(x => Weight.Exact is not null ? x.Weight == Weight.Exact : x.Weight > Weight.Min && x.Weight < Weight.Max)
            .ToArray();
    }

    public IEnumerable<PokemonData> Filter(Guess guess)
    {
        return FilteredPokemons
            .Where(x => (guess.Generation == ValueGuess.Equal ? guess.Pokemon.Generation == x.Generation : (guess.Generation == ValueGuess.Higher ? x.Generation > guess.Pokemon.Generation : x.Generation < guess.Pokemon.Generation))
            && (guess.MainType == TypeGuess.Equal ? guess.Pokemon.MainType == x.MainType : (guess.MainType == TypeGuess.Switch ? guess.Pokemon.MainType == x.SubType : guess.Pokemon.MainType != x.MainType))
            && (guess.SubType == TypeGuess.Equal ? guess.Pokemon.SubType == x.SubType : (guess.SubType == TypeGuess.Switch ? guess.Pokemon.SubType == x.MainType : guess.Pokemon.SubType != x.SubType))
            && (guess.Height == ValueGuess.Equal ? guess.Pokemon.Height == x.Height : (guess.Height == ValueGuess.Higher ? x.Height > guess.Pokemon.Height : x.Height < guess.Pokemon.Height))
            && (guess.Weight == ValueGuess.Equal ? guess.Pokemon.Weight == x.Weight : (guess.Weight == ValueGuess.Higher ? x.Weight > guess.Pokemon.Weight : x.Weight < guess.Pokemon.Weight)));
    }

    public static IEnumerable<PokemonData> Filter(Guess guess, PokemonData[] list)
    {
        return list
            .Where(x => (guess.Generation == ValueGuess.Equal ? guess.Pokemon.Generation == x.Generation : (guess.Generation == ValueGuess.Higher ? x.Generation > guess.Pokemon.Generation : x.Generation < guess.Pokemon.Generation))
            && (guess.MainType == TypeGuess.Equal ? guess.Pokemon.MainType == x.MainType : (guess.MainType == TypeGuess.Switch ? guess.Pokemon.MainType == x.SubType : guess.Pokemon.MainType != x.MainType))
            && (guess.SubType == TypeGuess.Equal ? guess.Pokemon.SubType == x.SubType : (guess.SubType == TypeGuess.Switch ? guess.Pokemon.SubType == x.MainType : guess.Pokemon.SubType != x.SubType))
            && (guess.Height == ValueGuess.Equal ? guess.Pokemon.Height == x.Height : (guess.Height == ValueGuess.Higher ? x.Height > guess.Pokemon.Height : x.Height < guess.Pokemon.Height))
            && (guess.Weight == ValueGuess.Equal ? guess.Pokemon.Weight == x.Weight : (guess.Weight == ValueGuess.Higher ? x.Weight > guess.Pokemon.Weight : x.Weight < guess.Pokemon.Weight)));
    }

    public void Update(Guess guess)
    {
        if (Generation.Exact is null)
            switch (guess.Generation)
            {
                case ValueGuess.Equal:
                    Generation = new(0, guess.Pokemon.Generation, 0);
                    break;
                case ValueGuess.Lower:
                    Generation = new(Generation.Min, null, Min(guess.Pokemon.Generation, Generation.Max));
                    break;
                case ValueGuess.Higher:
                    Generation = new(Max(guess.Pokemon.Generation, Generation.Min), null, Generation.Max);
                    break;
            }

        switch (guess.MainType)
        {
            case TypeGuess.Equal:
                for (int i = 0; i < MainType.Length; i++)
                    MainType[i] = i == (int)guess.Pokemon.MainType;
                break;
            case TypeGuess.Wrong:
                MainType[(int)guess.Pokemon.MainType] = false;
                break;
            case TypeGuess.Switch:
                for (int i = 0; i < SubType.Length; i++)
                    SubType[i] = i == (int)guess.Pokemon.MainType;
                break;
        }

        switch (guess.SubType)
        {
            case TypeGuess.Equal:
                for (int i = 0; i < SubType.Length; i++)
                    SubType[i] = i == (int)guess.Pokemon.SubType;
                break;
            case TypeGuess.Wrong:
                SubType[(int)guess.Pokemon.SubType] = false;
                break;
            case TypeGuess.Switch:
                for (int i = 0; i < MainType.Length; i++)
                    MainType[i] = i == (int)guess.Pokemon.SubType;
                break;
        }

        if (Height.Exact is null)
            switch (guess.Height)
            {
                case ValueGuess.Equal:
                    Height = new(0, guess.Pokemon.Height, 0);
                    break;
                case ValueGuess.Lower:
                    Height = new(Height.Min, null, Min(guess.Pokemon.Height, Height.Max));
                    break;
                case ValueGuess.Higher:
                    Height = new(Max(guess.Pokemon.Height, Height.Min), null, Height.Max);
                    break;
            }

        if (Weight.Exact is null)
            switch (guess.Weight)
            {
                case ValueGuess.Equal:
                    Weight = new(0, guess.Pokemon.Weight, 0);
                    break;
                case ValueGuess.Lower:
                    Weight = new(Weight.Min, null, Min(guess.Pokemon.Weight, Weight.Max));
                    break;
                case ValueGuess.Higher:
                    Weight = new(Max(guess.Pokemon.Weight, Weight.Min), null, Weight.Max);
                    break;
            }
        Console.Clear();
        Console.WriteLine("Updating information...");

        /*
         var guesser = PokemonDB[i];

            List<Guess> guesses = new List<Guess>();
            for (int j = 0; j < PokemonDB.Length; j++) // Sort into different guesses
            {
                bool isFound = false;
                Guess guess = new Guess(guesser, PokemonDB[j]);
                for (int k = 0; k < guesses.Count; k++)
                {
                    if (guesses[k] == guess)
                    {
                        guess.GuessId = guesses[k].GuessId + 1;
                        guesses[k]= guess;
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    guess.GuessId = 1;
                    guesses.Add(guess);
                }
            }

            double firstStepEntropy = 0;
            for (int k = 0; k < guesses.Count; k++)
            {
                var pokemons = PokemonSearch.Filter(guesses[k], PokemonDB).ToArray();
                firstStepEntropy += ToEntropy(PokemonDB.Length, pokemons.Count())*((double)guesses[k].GuessId/PokemonDB.Length);
            }
        */

        float Info = guess.Pokemon.Information;
        float PreviousInfo = (float)-Math.Log2((double)1 / PokeSearch.FilteredPokemons.Length);
        Filter();
        float NewInfo = (float)-Math.Log2((double)1 / PokeSearch.FilteredPokemons.Length);

        float ActualEntropy = PreviousInfo - NewInfo;
        guess.GuessId = GuessesList.Count() + 1;
        guess.ExpectedEntropy = Info;
        guess.ActualEntropy = ActualEntropy;
        GuessesList.Add(guess);

        for (int i = 0; i < PokemonDB.Length; i++)
        {
            var guesser = PokemonDB[i];
            List<Guess> guesses = GetListFromGuesser(guesser, FilteredPokemons);

            float entropy = 0;
            for (int k = 0; k < guesses.Count; k++)
            {
                int pokemons = PokemonSearch.Filter(guesses[k], FilteredPokemons).Count();
                entropy += ToEntropy(FilteredPokemons.Length,pokemons) * ((float)guesses[k].GuessId / FilteredPokemons.Length);
            }
            PokemonDB[i].Information = (float)entropy;
        }
        Console.Clear();
        //Console.WriteLine($"Guess №{guess.GuessId}: {guess.Pokemon.Name} | Expected entropy: {Info:F2} | Actual entropy: {ActualEntropy:F2}");

    }
}

public struct Guess
{
    public enum ValueGuess { Equal, Higher, Lower }
    public enum TypeGuess { Equal, Wrong, Switch }
    public PokemonData Pokemon;
    public ValueGuess Generation;
    public TypeGuess MainType;
    public TypeGuess SubType;
    public ValueGuess Height;
    public ValueGuess Weight;

    public int GuessId = 0;
    public float ExpectedEntropy = 0;
    public float ActualEntropy = 0;

    public Guess(PokemonData pokemon, string guessString)
    {
        Pokemon = pokemon;
        Generation = DecodeValue(guessString[0]);
        MainType = DecodeType(guessString[1]);
        SubType = DecodeType(guessString[2]);
        Height = DecodeValue(guessString[3]);
        Weight = DecodeValue(guessString[4]);

        ValueGuess DecodeValue(char v) => v switch
        {
            'E' => ValueGuess.Equal,
            'C' => ValueGuess.Equal,
            '=' => ValueGuess.Equal,
            'H' => ValueGuess.Higher,
            '^' => ValueGuess.Higher,
            'L' => ValueGuess.Lower,
            'v' => ValueGuess.Lower,
            _ => throw new ArgumentException()
        };

        TypeGuess DecodeType(char t) => t switch
        {
            'E' => TypeGuess.Equal,
            'C' => TypeGuess.Equal,
            '=' => TypeGuess.Equal,
            'W' => TypeGuess.Wrong,
            'X' => TypeGuess.Wrong,
            'S' => TypeGuess.Switch,
            _ => throw new ArgumentException()
        };
    }

    public Guess(PokemonData pokemon, PokemonData anwser)
    {
        Pokemon = pokemon;
        if (pokemon.Generation < anwser.Generation)
            Generation = ValueGuess.Higher;
        else if (pokemon.Generation > anwser.Generation)
            Generation = ValueGuess.Lower;
        else
            Generation = ValueGuess.Equal;

        if (pokemon.MainType == anwser.MainType)
            MainType = TypeGuess.Equal;
        else if (pokemon.MainType == anwser.SubType)
            MainType = TypeGuess.Switch;
        else
            MainType = TypeGuess.Wrong;

        if (pokemon.SubType == anwser.SubType)
            SubType = TypeGuess.Equal;
        else if (pokemon.SubType == anwser.MainType)
            SubType = TypeGuess.Switch;
        else
            SubType = TypeGuess.Wrong;

        if (pokemon.Height < anwser.Height)
            Height = ValueGuess.Higher;
        else if (pokemon.Height > anwser.Height)
            Height = ValueGuess.Lower;
        else
            Height = ValueGuess.Equal;

        if (pokemon.Weight < anwser.Weight)
            Weight = ValueGuess.Higher;
        else if (pokemon.Weight > anwser.Weight)
            Weight = ValueGuess.Lower;
        else
            Weight = ValueGuess.Equal;
    }

    public static bool operator ==(Guess a, Guess b)
    {
        return a.Generation == b.Generation
            && a.MainType   == b.MainType
            && a.SubType    == b.SubType
            && a.Height     == b.Height
            && a.Weight     == b.Weight;
    }

    public static bool operator !=(Guess a, Guess b)
    {
        return !(a == b);
    }

    public static List<Guess> GetListFromGuesser(PokemonData guesser, PokemonData[] data)
    {
        List<Guess> guesses = new List<Guess>();
        for (int i = 0; i < data.Length; i++) // Sort into different guesses
        {
            bool isFound = false;
            Guess guess = new Guess(guesser, data[i]);
            for (int k = 0; k < guesses.Count; k++)
            {
                if (guesses[k] == guess)
                {
                    guess.GuessId = guesses[k].GuessId + 1;
                    guesses[k] = guess;
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                guess.GuessId = 1;
                guesses.Add(guess);
            }
        }
        return guesses;
    }

    public static void MakeGuess()
    {
        Console.Clear();
        const int MAX_POKEMONS_DISPLAYED = 1500;
        PokemonData Pokemon = null;
        while (Pokemon == null)
        {
            Console.ResetColor();
            if (PokeSearch.GuessesList.Count() != 0)
            {
                Console.WriteLine("All guesses:");
                foreach (var guess in PokeSearch.GuessesList)
                {
                    Console.Write($"Guess №{guess.GuessId} - {guess.Pokemon.Name} | Expected entropy: {guess.ExpectedEntropy:F2} | Actual entropy: {guess.ActualEntropy:F2} ");
                    WriteColored($"({(guess.ActualEntropy < guess.ExpectedEntropy ? "" : "+")}{guess.ActualEntropy - guess.ExpectedEntropy:F2})", guess.ActualEntropy < guess.ExpectedEntropy ? ConsoleColor.Red : ConsoleColor.Green);
                }
                Console.WriteLine();
            }
            if (PokeSearch.FilteredPokemons.Length == 1)
            {
                WriteColored($"Congratulations! The pokemon you were looking for is {PokeSearch.FilteredPokemons.First().Name}", ConsoleColor.Green);
                Console.WriteLine("\nPress Enter to restart");
                Console.ReadLine();
                WordleGuesser.Main(null);
                return;
            }
            else if (PokeSearch.GuessesList.Count() != 0)
            {
                WriteColored($"Pokemon matches: {PokeSearch.FilteredPokemons.Length} | Bits: {-Math.Log2((double)1 / PokeSearch.FilteredPokemons.Length):F2}", ConsoleColor.Cyan);
            }
            if (PokeSearch.FilteredPokemons.Length == 0)
            {
                WriteColored($"We did not find the pokemon you are searching for", ConsoleColor.Red);
                Console.WriteLine("\nPress Enter to restart");
                Console.ReadLine();
                WordleGuesser.Main(null);
                return;
            }
            if (PokeSearch.FilteredPokemons.Length <= 5)
            {
                Console.Write($"Possible answers: ");
                for (int i = 0; i < PokeSearch.FilteredPokemons.Length; i++)
                    Console.Write(PokeSearch.FilteredPokemons[i].Name + (i != PokeSearch.FilteredPokemons.Length - 1 ? ", " : ""));
                Console.WriteLine();
            }
            else
            {
                if (PokeSearch.GuessesList.Count == 0)
                    Console.WriteLine($"List of commands \n" +
                        $"All - Displays list of all pokemons\n" +
                        $"Best - Diplays best 25 picks\n" +
                        $"Worst - Diplays worst 25 picks\n" +
                        $"Correct - Diplays list of all possible answers\n" +
                        $"Help - How to understand entropy\n" +
                        $"Reset - Resets all of the guesses\n");
                Console.WriteLine($"Type in guessed pokemon name or command:");
            }
            string guessString = (Console.ReadLine() ?? "").Trim().ToLower();
            PokemonData[] foundPokemon;
        FilterString:
            if (guessString == "all")
            {
                foundPokemon = PokemonDB;
                Array.Sort(foundPokemon);
                Console.Clear();
                Console.WriteLine($"List of all pokemons");
            }
            else if (guessString == "best")
            {
                foundPokemon = PokemonDB;
                Array.Sort(foundPokemon);
                foundPokemon = foundPokemon.Take(25).ToArray();
                Console.Clear();
                Console.WriteLine($"List of 25 best picks");
            }
            else if (guessString == "worst")
            {
                foundPokemon = PokemonDB;
                Array.Sort(foundPokemon);
                Array.Reverse(foundPokemon);
                foundPokemon = foundPokemon.Take(25).ToArray();
                Console.Clear();
                Console.WriteLine($"List of 25 worst picks");
            }
            else if (guessString == "correct")
            {
                foundPokemon = PokeSearch.FilteredPokemons;
                Array.Sort(foundPokemon);
                Console.Clear();
                Console.WriteLine($"List of all possible correct anwsers");
            }
            else if (guessString == "help")
            {
                Console.WriteLine($"What is entropy (Information)");
                Console.WriteLine($"The entropy is how many times it will cut all possible answers in half (on average)");
                Console.WriteLine($"Here are some examples: \n" +
                    $"For example there are 1000 pokemons and you guess Bulbasaur with expected entropy of 1 bit\n" +
                    $"If we guess him, we expect possible anwsers to cut in half 1 time, so we will be left with 500 pokemons on average \n" +
                    $"But, after guessing him, we are left with 250 pokemons, so the actual entropy will be 2 (Cut in half 2 times) \n\n" +
                    $"If Simipour has Entropy of 5 bits, guessing it will cut possible anwsers in 1/32 (on average)\n" +
                    $"So on average we will go from 1000 to around 31 pokemons to guess from. (1000/2^5 = 31.25)\n\n" +
                    $"You can also display the amount of possible answers in entropy. It will be it's uncertainty\n" +
                    $"The formula is -log2(1/x), where x is amount of possible guesses. With 1000 we will get ~10 bits\n" +
                    $"Whih is basically that you will need to cut it in half 10 times to get the correct answer\n");
                Console.ReadKey();
                Console.Clear();
                continue;
            }
            else if (guessString == "reset")
            {
                Console.Clear();
                WordleGuesser.Main(null);
                return;
            }
            else
            {
                var firstFilter = PokemonDB.Where(x => x.Name.ToLower().StartsWith(guessString) && PokeSearch.FilteredPokemons.Contains(x)).ToList();
                firstFilter.Sort();
                var secondFilter = PokemonDB.Where(x => !firstFilter.Contains(x) && x.Name.ToLower().Contains(guessString)).ToArray();
                Array.Sort(secondFilter);
                firstFilter.AddRange(secondFilter);
                foundPokemon = firstFilter.ToArray();
                Console.Clear();
                Console.WriteLine($"Found {foundPokemon.Length} pokemons, which one did you mean?");
            }
            if (foundPokemon.Length == 0)
            {
                Console.Clear();
                WriteColored("Pokemons not found", ConsoleColor.Red);
                continue;
            }
            else if (foundPokemon.Length == 1)
            {
                Pokemon = foundPokemon.Single();
                break;
            }
            else if (foundPokemon.Length <= MAX_POKEMONS_DISPLAYED)
            {
                Console.WriteLine($" 0  - None of them");
                int maxNameLength = foundPokemon.Select(x => x.Name.Length).Max();
                int maxTypeLength = foundPokemon.Select(x => (x.SubType != PokemonType.None ? $"{x.MainType}, {x.SubType}" : $"{x.MainType}").Length).Max();
                int maxWeightLength = foundPokemon.Select(x => x.Weight.ToString("F1").Length).Max();
                int maxHeightLength = foundPokemon.Select(x => x.Height.ToString("F1").Length).Max();
                float currentInfo = ToEntropy(PokeSearch.FilteredPokemons.Length);// (float)-Math.Log2((double)1 / PokeSearch.FilteredPokemons.Length);

                for (int i = 0; i < foundPokemon.Length; i++)
                {
                    string typeString = foundPokemon[i].SubType != PokemonType.None ? $"{foundPokemon[i].MainType}, {foundPokemon[i].SubType}" : $"{foundPokemon[i].MainType}";
                    ConsoleColor color = PokeSearch.FilteredPokemons.Contains(foundPokemon[i]) ? ConsoleColor.White : ConsoleColor.DarkGray;
                    if (AproxEqual(currentInfo, foundPokemon[i].Information))
                        color = color == ConsoleColor.White ? ConsoleColor.Green : ConsoleColor.DarkYellow;
                    WriteColored($" {i + 1}{(foundPokemon.Length >= 100 ? (i + 1 < 100 ? (i + 1 < 10 ? "   " : "  ") : " ") : (i + 1 < 10 ? "  " : " "))}- ", color, false);
                    WriteHighlighted(foundPokemon[i].Name, guessString, false, color);
                    WriteColored(new String(' ', maxNameLength - foundPokemon[i].Name.Length) +
                        $" | Gen: {foundPokemon[i].Generation} | " +
                        typeString + new String(' ', maxTypeLength - typeString.Length) +
                        $" | Height: {foundPokemon[i].Height:F1}" +
                        new String(' ', maxHeightLength - foundPokemon[i].Height.ToString("F1").Length) +
                        $" | Weight: {foundPokemon[i].Weight:F1}" +
                        new String(' ', maxWeightLength - foundPokemon[i].Weight.ToString("F1").Length) +
                        $" | Info: {foundPokemon[i].Information:F2}" + 
                        (PokeSearch.GuessesList.Count() == 0 ? $" [{foundPokemon[i].Information+foundPokemon[i].InformationDouble:F2}]" : ""), color);
                }
                Console.SetWindowPosition(0, 0);
                while (true)
                {
                    string input = Console.ReadLine() ?? "";
                    if (int.TryParse(input, out int result))
                    {
                        if (!(result > foundPokemon.Length || result <= 0))
                        {
                            Pokemon = foundPokemon[result - 1];
                            break;
                        }
                        else if (result == 0)
                        {
                            Console.Clear();
                            break;
                        }
                        else
                        {
                            WriteColored($"Wrong value, please type value from 1 to {foundPokemon.Length}", ConsoleColor.Red);
                            continue;
                        }
                    }
                    else
                    {
                        input = input.ToLower().Trim();
                        if ((input == "none" || input == "repick" || input == "other" || input == "reselect"))
                        {
                            Console.Clear();
                            break;
                        }
                        guessString = input;
                        goto FilterString;
                    }
                }
            }
            else // Found more than MAX_POKEMONS_DISPLAYED pokemons
            {
                Console.Clear();
                WriteColored("Too many pokemons are found! Please be more specific", ConsoleColor.Red);
            }
        }
        Console.Clear();
        // Guess format: L - lower (value), H - higher (value), E - Exact (value and type), W - Wrong (type), S - Switch (type)
        while (true)
        {
            Console.WriteLine($"Selected pokemon: {Pokemon.Name}. Type \"Repick\" to reselsect the pokemon.");
            Console.WriteLine($"Please enter Squirdle result in \"?????\" format, where ? needs to be L(ower),H(igher),C(orrect),W(rong),S(witch)");
            string guessString = (Console.ReadLine() ?? "").Trim().ToUpper();
            if (guessString.ToLower() == "repick" || guessString.ToLower() == "reselect")
            {
                Console.Clear();
                MakeGuess();
                return;
            }
            if (guessString.Length != 5 || !Regex.IsMatch(guessString, "[LHEC^v=][EWSCX=][EWSCX=][LHEC^v=][LHEC^v=]"))
            {
                Console.Clear();
                WriteColored("Wrong format", ConsoleColor.Red);
                continue;
            }
            PokeSearch.Update(new Guess(Pokemon, guessString));
            return;
        }

        static void WriteColored(string line, ConsoleColor color, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(line);
            else
                Console.Write(line);
            Console.ResetColor();
        }

        static void WriteHighlighted(string line, string highlight, bool newLine = true, ConsoleColor mainColor = ConsoleColor.White)
        {
            var split = line.Split(' ');

            for (int i = 0; i < split.Length; i++)
            {
                Write(split[i] + (i != split.Length - 1 ? " " : string.Empty), i);
            }

            if (newLine)
                Console.WriteLine();

            void Write(string word, int wordid)
            {
                var split = word.ToLower().Split(highlight);

                if (split[0].Length != 0)
                    split[0] = SetFirstToUpper(split[0]);

                for (int i = 0; i < split.Length; i++)
                {
                    WriteColored(split[i], mainColor, false);
                    if (i != split.Length - 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        if (i == 0 && split[0].Length == 0)
                        {
                            if (wordid == 0)
                                Console.ForegroundColor = ConsoleColor.Blue;
                            Console.Write(SetFirstToUpper(highlight));
                        }
                        else
                        {
                            Console.Write(highlight);
                        }
                        Console.ResetColor();
                    }
                }
            }

            static string SetFirstToUpper(string str) => char.ToUpper(str[0]) + str.Substring(1);
        }

    }
}