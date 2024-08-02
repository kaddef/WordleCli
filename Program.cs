using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

public class Game
{
    private HttpClient client;
    public string? word;
    public int guessCount = 0;
    public bool found = false; 
    public List<string> guesses = new List<string>();
    public List<int[]> guessResults = new List<int[]>();
    public string header = @"  
  _      __  ____    ___    ___    __    ____  ──────▄▀▄─────▄▀▄
 | | /| / / / __ \  / _ \  / _ \  / /   / __/  ─────▄█░░▀▀▀▀▀░░█▄
 | |/ |/ / / /_/ / / , _/ / // / / /__ / _/    ─▄▄──█░░░░░░░░░░░█──▄▄
 |__/|__/  \____/ /_/|_| /____/ /____//___/    █▄▄█─█░░▀░░┬░░▀░░█─█▄▄█
";

    public Game(HttpClient client)
    {
        this.client = client;
    }

    public async Task StartGame()
    {
        word = await GetWord();
        MainMenu();
    }

    public void MainMenu()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(header);
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
        Console.Clear();
        GameLoop();
    }

    public void GameLoop()
    {
        while (true)
        {
            Console.Clear() ;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(header);
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < guesses.Count; i++)
            {
                Console.Write("".PadLeft(27));
                string guessChar = guesses[i];
                int[] res = guessResults[i];
                for (int j = 0; j < 5; j++)
                {
                    if (res[j] == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    } else if (res[j] == 2)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    } else if (res[j] == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.Write("[{0}]", guessChar[j]);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.WriteLine();
            }
            for (int i = guesses.Count; i < 6; i++)
            {
                Console.Write("".PadLeft(27));
                Console.WriteLine("[ ][ ][ ][ ][ ]");
            }
            if(found == true)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nCongratulations, you found the word");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Game over, press any key to close the window...");
                Console.ReadKey(true);
                return;
            }
            else if(guessCount > 5)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nUnfortunately, you couldn't guess the word. The word was {0}.", word);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Game over, press any key to close the window...");
                Console.ReadKey(true);
                return;
            }
            else
            {
                found = getGuess();
            }
        }
    }

    public bool getGuess()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Please guess a word: ");
            string? guess = Console.ReadLine();

            if (!string.IsNullOrEmpty(guess) && guess.Length == 5 && Regex.IsMatch(guess, @"^[a-zA-Z]+$"))
            {
                guessCount++;
                guesses.Add(guess.ToUpper());
                guessResults.Add(GuessCheck(guess));
                return GuessCheck(guess).All(item => item == 1); ;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nInput cannot be null, empty, or less than 5 characters. Please try again.");

                Console.SetCursorPosition(0, Console.CursorTop - 3);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }
    }

    public int[] GuessCheck(string guess)
    {
        int[] result = new int[5];
        for (int i = 0; i < result.Length; i++)
        {
            if (guess[i] == word[i])
            {
                result[i] = 1;
            }
            else if (guess[i] != word[i] && word.Contains(guess[i]))
            {
                result[i] = 2;
            }
            else
            {
                result[i] = 0;
            }
        }
        return result;
    }

    public async Task<string?> GetWord()
    {
        try
        {
            HttpResponseMessage response = await client.GetAsync("https://random-word-api.herokuapp.com/word?length=5");
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            List<string>? jsonList = JsonSerializer.Deserialize<List<string>>(content);
            if (jsonList != null && jsonList.Count > 0)
            {
                return jsonList[0];
            }
            else
            {
                Console.WriteLine("Cannot able to get word. Server returned empty JSON");
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("Cannot able to get word. Server returned error");
            Console.WriteLine(e.Message);
            return null;
        }
    }
}

class Program
{
    static async Task Main()
    {
        Console.SetWindowSize(71, 20);
        HttpClient client = new HttpClient(); 
        Game game = new Game(client);
        await game.StartGame();
        return;
    }
}

