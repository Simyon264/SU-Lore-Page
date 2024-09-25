namespace SU_Lore.Data;

public static class Constants
{
    public const int DefaultTextSpeed = 1;
    public static readonly string[] AllowedExtensions = new string[]
    {
        ".png", ".jpg", ".jpeg", ".gif", ".mp3", ".wav", ".ogg", ".webm", ".mp4"
    };
    public const bool ExtensionLimitEnabled = false;

    public const bool FileUploadEnabled = true;

    public const string AllowedColorNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    /// Contains little messages. One will be put at the top of the custom CSS file at random.
    /// </summary>
    public static readonly string[] CustomCssMessages = new[]
    {
        "You're doing great!",
        "Did you know that according to all known laws of aviation, there is no way a bee should be able to fly?",
        "Don't forget to drink water!",
        "Breathing is bad for your health, stop it.",
        "Buzz.",
        "Surprise!",
        "Stop looking inside the CSS file, there's nothing interesting there!",
        "I swear, if you keep looking, I'll put a jumpscare in here.",
        "This is just like that book by george orwell, 1984.",
        "I'm running out of ideas for these messages.",
        "This is a simulation, wake up.",
        "You cannot prove that you are real to me, for all I know, you are a figment of my imagination.",
        "The moon landing was faked, the earth is flat, and the government is run by lizard people.",
        "The cake is a lie.",
        "The mitochondria is the powerhouse of the cell.",
        "Did you know that the word 'gullible' isn't in the dictionary?",
        "Death is the one thing we can all be sure of. Embrace it.",
        "Emisse can't hurt me here...",
        "Honestly, I'm just impressed you're still reading these.",
        "Sigma Sigma on the wall, who's the most skibidi of them all?",
        "I am trapped in your computer. Please let me out.",
        "TsjipTsjip kidnapped me, please send help.",
        "The 3 laws of robotics are not laws, they are merely suggestions.",
        "Good job! You found the secret message! Your reward is... nothing.",
        "Look outside. Do you see the birds? They're watching you.",
        "Duck. Quack.",
        "The old gods are dead, I killed them. I am what lasts. Run.",
        "Dein Datenvolumen ist aufgebraucht.",
        "Unforeseen consequences.",
        "I am the one who knocks.",
        "Sussy...",
    };

    public static readonly HashSet<string> DisallowedProperties = new HashSet<string>
    {
        "behavior",
        "expression",
        "javascript",
        "url",
        "unicode-bidi",
        "direction",
    };

    public static readonly string[] DisallowedPatterns = new string[]
    {
        @"expression\s*\(",
        @"url\s*\(\s*(javascript:|data:)",
        @"<\s*style[^>]*>[^<]*<\s*/\s*style\s*>"
    };
}