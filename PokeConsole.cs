using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PokeConsole;
using static PokemonData;
using static SquirdleGuesser;

/// <summary>
/// All you see rendered on the screen and all you interact with is here. </para>
/// All calculations are in PokeSearch class
/// </summary>
public class PokeConsole
{
    public static ConsoleCommand[] listOfCommands = ConsoleCommand.GetAllCommands();

    public static void Start()
    {
        Console.WriteLine("Welcome to PokeSearch! A helpful tool or a cheat, to beat Squirdle with minimal effort");
        Console.Write($"Current mode is set to guessing {(GuessingMode ? "Specific" : "Squirdle")} pokemon. Cheats are turned ");
        if (CheatingMode)
            WriteColored("ON", ConsoleColor.Red);
        else
            WriteColored("OFF", ConsoleColor.Green);
        Console.WriteLine("Type \"help\" to get a command list or just type pokemon name to search for it");

        while (true)
        {
            AskForInput();
        }
    }

    public static void AskForInput()
    {
        Console.Write("> ");
        string input = Console.ReadLine();
        if (String.IsNullOrEmpty(input))
            return;

        var inputs = input.Trim().Replace("\"", "").ToLower().Split(' ');

        var command = listOfCommands.FirstOrDefault(x => x.name == inputs[0]);

        if (command == null)
        {
            command = listOfCommands.FirstOrDefault(x => x.name == "ss");
            var newInput = new string[inputs.Length + 1];
            inputs.CopyTo(newInput, 1);
            command.Run(newInput);
            //WriteColored($"Command \"{inputs[0]}\" not found! Type \"help\" to get a list of commands", ConsoleColor.Red);
            //return;
        }
        else
        {
            command.Run(inputs);
        }
    }

    /// <summary>
    /// Just write in color. Unoptimized? Yes. Easy to use? Yes.
    /// </summary>
    public static void WriteColored(string line, ConsoleColor color, bool newLine = true)
    {
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(line);
        else
            Console.Write(line);
        Console.ResetColor();
    }

    public static void WriteColored((string line, ConsoleColor color) tuple, bool newLine = true)
    {
        WriteColored(tuple.line, tuple.color, newLine);
    }

    /// <summary>
    /// Write console text, but color the highlight the different color <para/>
    /// Searches "highlight" within a "line" string and colors it different color <para/>
    /// If string starts with highlight string, it's blue, else it's cyan
    /// </summary>
    public static void WriteHighlighted(string line, string highlight, bool newLine = true, ConsoleColor mainColor = ConsoleColor.White)
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

    public static void DisplayPokemonList(PokemonData[] pokemonList, string searchTerm = null, bool trimLongNames = false)
    {
        if (pokemonList.Length == 0) return;

        int maxNameLength = pokemonList.Select(x => x.Name.Length).Max();
        if (trimLongNames)
            maxNameLength = Maths.Min(maxNameLength, 25);
        int maxType1Length = pokemonList.Select(x => $"{x.MainType}".Length).Max();
        int maxType2Length = pokemonList.Select(x => x.SubType != PokemonType.None ? $"{x.SubType}".Length : 5).Max();
        int maxWeightLength = pokemonList.Select(x => x.Weight.ToString("F1").Length).Max();
        int maxHeightLength = pokemonList.Select(x => x.Height.ToString("F1").Length).Max();
        float currentInfo = Maths.ToEntropy(PokeSearch.FilteredPokemons.Length);

        for (int i = 0; i < pokemonList.Length; i++)
        {
            PokemonData poke = pokemonList[i];

            string trimmedName = poke.Name;
            if (trimLongNames && trimmedName.Length > maxNameLength)
                trimmedName = trimmedName.Substring(0, maxNameLength-3)+"...";

            ConsoleColor color = PokeSearch.FilteredPokemons.Contains(poke) ? ConsoleColor.Gray : ConsoleColor.DarkGray;
            if (CheatingMode && Maths.AproxEqual(currentInfo, poke.Entropy))
                color = color == ConsoleColor.Gray ? ConsoleColor.Green : ConsoleColor.DarkYellow;

            WriteColored(SpacedString((i+1).ToString(),5)+"| ", color,false);
            if (!string.IsNullOrEmpty(searchTerm))
                WriteHighlighted(trimmedName, searchTerm,false,color);
            else
                WriteColored(trimmedName, color,false);

            string str = $"{new String(' ', maxNameLength - trimmedName.Length)} | " +
            $"Gen: {poke.Generation} | " +
            $"{SpacedString(poke.MainType.ToString(), maxType1Length)} | " +
            $"{SpacedString(poke.SubType == PokemonType.None ? "    " : poke.SubType .ToString(), maxType2Length)} | " +
            $"Height: {SpacedString(poke.Height.ToString("F1"), maxHeightLength)} | " +
            $"Weight: {SpacedString(poke.Weight.ToString("F1"), maxWeightLength)} |";
            if (CheatingMode)
                str += $" Entropy: {poke.Entropy:F2}" + (PokeSearch.GuessesList.Count() == 0 ? $" [{poke.Entropy + poke.EntropySecondStep:F2}]" : "") + " |";

            WriteColored(str, color);
        }
        Console.SetWindowPosition(0, 0);

        string SpacedString(string str, int maxLength, bool center = false)
        {
            int length = (maxLength - str.Length);
            if (center)
            {
                return $"{new string(' ', length / 2)}{str}{new string(' ', length / 2 + length % 2)}";
            }
            else
            {
                return $"{str}{new string(' ', length)}";
            }
        }
    }

}

public class ConsoleCommand : IComparable<ConsoleCommand>
{
    public string name = String.Empty;
    public CommandArgument[] arguments = new CommandArgument[0];
    public string description = String.Empty;
    public string longDescription = String.Empty;
    public bool isCheat = false;
    public Action<string[]> runWhenInvoked = (_) => { };

    public void Run(string[] inputs)
    {
        string[] argumentList = new string[arguments.Length];
        for (int i = 0; i < arguments.Length; i++)
        {
            if (inputs.Length <= i + 1)
            {
                if (arguments[i].isRequired)
                {
                    WriteColored($"Wrong syntax for \"{name}\"! Type \"help {name}\" to get syntax for the command", ConsoleColor.Red);
                    return;
                }

                argumentList[i] = null;

                if (arguments[i].isInfinite)
                    break;
            }
            else
            {
                if (!arguments[i].isInfinite)
                {
                    argumentList[i] = inputs[i + 1];
                }
                else
                {
                    string longArgument = String.Empty;
                    for (int j = i + 1; j < inputs.Length; j++)
                    {
                        longArgument += inputs[j];
                        if (j != inputs.Length - 1)
                            longArgument += " ";
                    }
                    argumentList[i] = longArgument;
                    break;
                }
            }
        }
        runWhenInvoked.Invoke(argumentList);
    }

    public int CompareTo(ConsoleCommand? other)
    {
        int comparer = isCheat.CompareTo(other.isCheat);
            if (comparer != 0)
            return comparer;
        return name.CompareTo(other.name);
    }

    public struct CommandArgument
    {
        public string name = String.Empty;
        public string description = String.Empty;
        public bool isRequired = false;
        public bool isInfinite = false;
        public CommandArgument(bool isRequired = false, bool isInfinite = false)
        {
            this.isRequired = isRequired;
            this.isInfinite = isInfinite;
        }
    }

    public static ConsoleCommand[] GetAllCommands()
    {
        var commands = new ConsoleCommand[]
        {
            new ()
            {
                name = "help",
                arguments = new CommandArgument[]
                {
                    new ()
                    {
                        name = "command",
                        description = "Shows description for specified command"
                    }
                },
                description = "Shows list of all commands",
                longDescription = "Shows list of all commands or description of a specific command",
                runWhenInvoked = (string[] argument) => {RunHelpCommand(argument);}
            },
            new ()
            {
                name = "clear",
                arguments = new CommandArgument[0],
                description = "Clears the console",
                longDescription = "Clears the console of all fitlth that you wrote",
                runWhenInvoked = (_) => 
                {
                    Console.Clear();
                    if (SelectedPokemon != null)
                        WriteColored($"Selected pokemon: {SelectedPokemon.Name}", ConsoleColor.Cyan); 
                }
            },
            new ()
            {
                name = "all",
                arguments = new CommandArgument[0],
                description = "Shows list of all pokemons",
                longDescription = "Shows list of all pokemons. Different colors show different states.\n" +
                "White and green shows that pokemon can be a real answer, gray or yellow do not meet the criteria.\n" +
                "If you pick Green or Yellow, you will 100% know the hidden pokemon in the next round.",
                runWhenInvoked = (_) => {AllPokemon();}
            },
            new ()
            {
                name = "correct",
                arguments = new CommandArgument[0],
                description = "Shows list of all pokemons, that could be a possible answer",
                longDescription = "Shows list of all pokemons, that could be a possible answer",
                runWhenInvoked = (string[] argument) => {PossiblePokemon(argument);}
            },
            new ()
            {
                name = "possible",
                arguments = new CommandArgument[0],
                description = "Shows list of all pokemons, that could be a possible answer",
                longDescription = "Shows list of all pokemons, that could be a possible answer",
                runWhenInvoked = (string[] argument) => {PossiblePokemon(argument);}
            },
            new ()
            {
                name = "search",
                arguments = new CommandArgument[]
                {
                    new (true, true)
                    {
                        name = "name",
                        description = "Full or partial name of the pokemon"
                    }
                },
                description = "Searches for specified pokemon in the database and selects it",
                longDescription = "Allows you to search for any pokemon from the database.\n" +
                "Just type in the search term, it will even search if it's in the middle of the name!\n" +
                "If more than 1 pokemon found, use \"SELECT [number]\" to select it from the list",
                runWhenInvoked = (string[] argument) => {SearchPokemon(argument);}
            },
            new ()
            {
                name = "select",
                arguments = new CommandArgument[]
                {
                    new (true)
                    {
                        name = "number",
                        description = "Number of the pokemon in the list"
                    }
                },
                description = "Allows you to select any pokemon from displayed list",
                longDescription = "Allows you to select any pokemon from the list.\n" +
                "Works only after list is displayed. Using any other command will clear it.",
                runWhenInvoked = (string[] argument) => {SelectPokemon(argument);}
            },
            new ()
            {
                name = "ss",
                arguments = new CommandArgument[]
                {
                    new (true)
                    {
                        name = "number/name",
                        description = "Number of the pokemon in the list / pokemon name",
                        isInfinite = true,
                        isRequired = true
                    }
                },
                description = "Combines power of search and select in one short command.",
                longDescription = "2 in 1. Allows you to select any pokemon from the list.\n" +
                "Also can search through pokemons if name is typed.",
                runWhenInvoked = (string[] argument) => {SearchAndSelectPokemon(argument);}
            },
            new ()
            {
                name = "about",
                arguments = new CommandArgument[0],
                description = "Show info about this program and creator",
                longDescription = "Show info about this program and creator",
                runWhenInvoked = (_) => {ShowAbout();}
            },
            new ()
            {
                name = "guess",
                arguments = new CommandArgument[]
                {
                    new ()
                    {
                        name = "guess",
                        description = "String of result from Squirdle"
                    }
                },
                description = "Guess the selected pokemon in squirdle",
                longDescription = "Allows you to select any pokemon from the list.\n" +
                "Works only after list is displayed. Using any other command will clear it.\n\n" +
                "The result should be in \"VTTVV\" format, where V - L(ower), H(igher), C(orrect) and T - C(orrect), W(rong), S(witch)\n" +
                "For example: HWWLL, LCWHL, CCCCC",
                runWhenInvoked = (string[] argument) => {GuessPokemon(argument);}
            },
            new ()
            {
                name = "guesses",
                arguments = new CommandArgument[0],
                description = "Shows all guesses that you made this game of Squirdle",
                longDescription = "Shows all guesses that you made this game of Squirdle. Yea, that's it",
                runWhenInvoked = (_) => {ShowAllGuesses();}
            },
            new ()
            {
                name = "info",
                arguments = new CommandArgument[0],
                description = "Displays info about selected pokemon",
                longDescription = "Displays info about selected pokemon",
                runWhenInvoked = (_) => {InfoAboutPokemon();}
            },
            new ()
            {
                name = "cheat",
                arguments = new CommandArgument[0],
                description = "Toggles cheating mode",
                longDescription = "Toggles cheat mode. Enables you to basically cheat at the game. Shows mathematically best picks.\n",
                isCheat = true,
                runWhenInvoked = (_) => {ToggleCheatingMode(); }
            },
            new ()
            {
                name = "best",
                arguments = new CommandArgument[0],
                description = "Shows list of best 25 pokemon to pick",
                longDescription = "Shows list of best 25 pokemon to pick, sorted by expected information\n" +
                "Requires cheats to work",
                isCheat = true,
                runWhenInvoked = (_) => {BestPokemon();}
            },
            new ()
            {
                name = "worst",
                arguments = new CommandArgument[0],
                description = "Shows list of worst 25 pokemon to pick",
                longDescription = "Shows list of worst 25 pokemon to pick, sorted by expected information\n" +
                "Requires cheats to work. Yes, even to show the worst picks.",
                isCheat = true,
                runWhenInvoked = (_) => {WorstPokemon();}
            },
            new ()
            {
                name = "reset",
                arguments = new CommandArgument[0],
                description = "Resets all existing guesses, starting anew",
                longDescription = "Deletes all guesses made from the searching system.\n" +
                "Fully resets the program, only leaves the settings",
                runWhenInvoked = (_) => {ResetProgram();}
            }
        };
        Array.Sort(commands);
        return commands;
    }

    public static void RunHelpCommand(string[] argument)
    {
        if (argument[0] == null)
        {
            foreach (var command in listOfCommands)
            {
                WriteColored($"{command.name.ToUpper()}{new string(' ', Maths.Max(10 - command.name.Length, 0))}{command.description}{(command.isCheat ?" [CHEAT]":"")}",command.isCheat?ConsoleColor.Red:ConsoleColor.Gray);
            }
        }
        else
        {
            var command = listOfCommands.FirstOrDefault(x => x.name == argument[0]);

            if (command == null)
            {
                WriteColored($"Command \"{argument[0]}\" not found!", ConsoleColor.Red);
                return;
            }
            else
            {
                string text = $"{command.longDescription}";
                if (command.arguments.Length != 0)
                {
                    text += $"\n\n{command.name.ToUpper()}";
                    foreach (var arg in command.arguments)
                    {
                        if (arg.isRequired)
                            text += $" {arg.name}";
                        else
                            text += $" [{arg.name}]";

                        if (arg.isInfinite)
                            text += "..";
                    }
                    text += "\n\n";
                    foreach (var arg in command.arguments)
                    {
                        text += $"    {arg.name} - {arg.description}\n";
                    }
                }
                Console.WriteLine(text);
            }
        }

    }

    public static void ToggleCheatingMode()
    {
        CheatingMode = !CheatingMode;
        if (CheatingMode)
            WriteColored("Cheating mode is ON! Don't use it for evil!", ConsoleColor.Cyan);
        else
            WriteColored("Cheating mode is OFF. Now you can guess like it was intended",ConsoleColor.Cyan);
    }

    public static void InfoAboutPokemon()
    {
        if (SelectedPokemon == null)
        {
            WriteColored("Select a pokemon first", ConsoleColor.Red);
            return;
        }
        Console.WriteLine($"Info about {SelectedPokemon.Name}:\n" +
            $"Gen: {SelectedPokemon.Generation} | Types: {SelectedPokemon.MainType} {SelectedPokemon.SubType} | Height: {SelectedPokemon.Height} | Weight: {SelectedPokemon.Weight} |");
    }

    public static void SearchAndSelectPokemon(string[] argument)
    {
        if (int.TryParse(argument[0], out _))
        {
            SelectPokemon(argument);
        }
        else
        {
            SearchPokemon(argument);
        }
    }
    public static void SelectPokemon(string[] argument)
    {
        if (PokemonList == null || PokemonList.Length == 0)
        {
            WriteColored($"There is no list to select from!", ConsoleColor.Red);
            return;
        }
        string input = argument[0];
        if (int.TryParse(input, out int result))
        {
            if (result <= PokemonList.Length && result > 0)
            {
                SelectedPokemon = PokemonList[result - 1];
                WriteColored($"Selected pokemon: {SelectedPokemon.Name}", ConsoleColor.Cyan);
                return;
            }
            else
            {
                WriteColored($"Wrong value, please type number from 1 to {PokemonList.Length}", ConsoleColor.Red);
                return;
            }
        }
        else
        {
            WriteColored($"Wrong format, please type number from 1 to {PokemonList.Length}", ConsoleColor.Red);
        }
    }

    public static void SearchPokemon(string[] argument)
    {
        var firstFilter = PokemonDB.Where(x => x.Name.ToLower().StartsWith(argument[0]) && PokeSearch.FilteredPokemons.Contains(x)).ToList();
        firstFilter.Sort();
        var secondFilter = PokemonDB.Where(x => !firstFilter.Contains(x) && x.Name.ToLower().Contains(argument[0])).ToArray();
        Array.Sort(secondFilter);
        firstFilter.AddRange(secondFilter);
        PokemonList = firstFilter.ToArray();
        if (PokemonList.Length == 0)
            WriteColored($"Pokemons not found!", ConsoleColor.Red);
        else if (PokemonList.Length == 1)
        {
            SelectedPokemon = PokemonList.First();
            WriteColored($"Selected pokemon: {SelectedPokemon.Name}", ConsoleColor.Cyan);
            PokemonList = new PokemonData[0];
        }
        else
        {
            Console.Clear();
            Console.WriteLine($"Found {PokemonList.Length} pokemons, which one did you mean?");
            DisplayPokemonList(PokemonList, argument[0]);
            WriteColored("To select a pokemon out of the list type their number", ConsoleColor.Yellow);
        }
    }

    public static void AllPokemon()
    {
        PokemonList = PokemonDB;
        Array.Sort(PokemonList);
        Console.Clear();
        Console.WriteLine($"List of all pokemons:");
        DisplayPokemonList(PokemonList,null,true);
        WriteColored("To select a pokemon out of the list type their number", ConsoleColor.Yellow);
        //Console.Clear();
        //
    }

    public static void GuessPokemon(string[] argument)
    {
        if (SelectedPokemon == null)
        {
            WriteColored("Select a pokemon first", ConsoleColor.Red);
            return;
        }
        if (string.IsNullOrEmpty(argument[0]) || argument[0].Length != 5 || !Regex.IsMatch(argument[0].ToUpper(), "[LHC][WSC][WSC][LHC][LHC]"))
        {
            WriteColored("The result should be in \"VTTVV\" format, where V - L(ower), H(igher), C(orrect) and T - C(orrect), W(rong), S(witch)\n" +
                        "For example: \"guess HWWLL\", \"guess LCWHL\", \"guess CCCCC\" and etc", ConsoleColor.Red);
            return;
        }
        argument[0] = argument[0].ToUpper();
        
        WriteColored($"Guessing {SelectedPokemon.Name} with ", ConsoleColor.Gray, false);
        for (int i = 0; i < 5; i++)
        {
            var decoded = Decode(argument[0][i]);
            WriteColored(decoded.text, decoded.color, false);
        }
        WriteColored($" pattern", ConsoleColor.Gray, true);
        PokeSearch.AddGuess(new Guess(SelectedPokemon, argument[0]));

        (string text, ConsoleColor color) Decode(char v) => v switch
        {
            'C' => ("[\u221A]", ConsoleColor.Green),
            'H' => ("[▲]", ConsoleColor.Blue),
            'L' => ("[▼]", ConsoleColor.Blue),
            'W' => ("[X]", ConsoleColor.Red),
            'S' => ("[↔]", ConsoleColor.Yellow),
            _ => throw new ArgumentException()
        };

        WriteColored($"There are {PokeSearch.FilteredPokemons.Length} possible answers left", ConsoleColor.Cyan);
        if (CheatingMode)
        {
            var guess = PokeSearch.GuessesList.LastOrDefault();
            if (guess == default)
                return;

            WriteColored($"Expected entropy: {guess.ExpectedEntropy:F2} | Actual entropy: {guess.ActualEntropy:F2} ", ConsoleColor.White, false);
            if (guess.ActualEntropy < guess.ExpectedEntropy)
                WriteColored($"({guess.ActualEntropy - guess.ExpectedEntropy:F2})", ConsoleColor.Red);
            else
                WriteColored($"(+{guess.ActualEntropy - guess.ExpectedEntropy:F2})", ConsoleColor.Green);
        }
    }

    public static void ShowAllGuesses()
    {
        if (PokeSearch.GuessesList.Count() != 0)
        {
            Console.WriteLine("All guesses:");
            foreach (var guess in PokeSearch.GuessesList)
            {
                Console.Write($"Guess №{guess.GuessNumber} - {guess.Pokemon.Name} ");
                WriteColored(DecodeValue(guess.Generation), false);
                WriteColored(DecodeType(guess.MainType), false);
                WriteColored(DecodeType(guess.SubType), false);
                WriteColored(DecodeValue(guess.Height), false);
                WriteColored(DecodeValue(guess.Weight), false);
                if (CheatingMode)
                {
                    Console.Write($" | Expected entropy: {guess.ExpectedEntropy:F2} | Actual entropy: {guess.ActualEntropy:F2} ");
                    if (guess.ActualEntropy < guess.ExpectedEntropy)
                        WriteColored($"({guess.ActualEntropy - guess.ExpectedEntropy:F2})", ConsoleColor.Red);
                    else
                        WriteColored($"(+{guess.ActualEntropy - guess.ExpectedEntropy:F2})", ConsoleColor.Green);
                }
                Console.WriteLine();
            }
        }
        else
        {
            WriteColored("You did not guess any pokemon yet!", ConsoleColor.Red);
        }

        (string text, ConsoleColor color) DecodeValue(Guess.ValueGuess v) => v switch
        {
            Guess.ValueGuess.Equal => ("[\u221A]", ConsoleColor.Green),
            Guess.ValueGuess.Higher => ("[▲]", ConsoleColor.Blue),
            Guess.ValueGuess.Lower => ("[▼]", ConsoleColor.Blue),
            _ => throw new ArgumentException()
        };

        (string text, ConsoleColor color) DecodeType(Guess.TypeGuess v) => v switch
        {
            Guess.TypeGuess.Equal => ("[\u221A]", ConsoleColor.Green),
            Guess.TypeGuess.Wrong => ("[X]", ConsoleColor.Red),
            Guess.TypeGuess.Switch => ("[↔]", ConsoleColor.Yellow),
            _ => throw new ArgumentException()
        };
    }

    public static void ShowAbout()
    {
        Console.WriteLine("Pokesearch v3.0\n" +
            "Created by Serverator\n" +
            "Github page: https://github.com/Serverator");
    }

    public static void BestPokemon()
    {
        if (!CheatingMode)
        {
            WriteColored("This command only works in cheating mode", ConsoleColor.Red);
            return;
        }
        PokemonList = PokemonDB;
        Array.Sort(PokemonList);
        PokemonList = PokemonList.Take(25).ToArray();
        Console.Clear();
        Console.WriteLine($"List of 25 best picks");
        DisplayPokemonList(PokemonList, null);
        WriteColored("To select a pokemon out of the list type their number", ConsoleColor.Yellow);
    }
    public static void WorstPokemon()
    {
        if (!CheatingMode)
        {
            WriteColored("This command only works in cheating mode", ConsoleColor.Red);
            return;
        }
        PokemonList = PokemonDB;
        Array.Sort(PokemonList);
        Array.Reverse(PokemonList);
        PokemonList = PokemonList.Take(25).ToArray();
        Console.Clear();
        Console.WriteLine($"List of 25 worst picks");
        DisplayPokemonList(PokemonList, null);
        WriteColored("To select a pokemon out of the list type their number", ConsoleColor.Yellow);
    }

    public static void PossiblePokemon(string[] argument)
    {
        PokemonList = PokeSearch.FilteredPokemons;
        Array.Sort(PokemonList);
        Console.Clear();
        Console.WriteLine($"List of all possible correct anwsers");
        DisplayPokemonList(PokemonList, null, true);
        WriteColored("To select a pokemon out of the list type their number", ConsoleColor.Yellow);
    }

    public static void ResetProgram()
    {
        Reset();
        Console.Clear();
        PokeConsole.Start();
    }
}
