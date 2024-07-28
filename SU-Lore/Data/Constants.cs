namespace SU_Lore.Data;

public static class Constants
{
    public const int DefaultTextSpeed = 1;
    public static readonly string[] AllowedExtensions = new string[]
    {
        ".png", ".jpg", ".jpeg", ".gif", ".mp3", ".wav", ".ogg", ".webm", ".mp4"
    };
    
    public const string AllowedColorNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    /// <summary>
    /// Contains little messages. One will be put at the bottom of the custom CSS file at random.
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
        "I'm not saying it was aliens, but it was aliens.",
        "The cake is a lie.",
        "The mitochondria is the powerhouse of the cell.",
        "I am inevitable.",
        "Dread it. Run from it. Destiny arrives all the same.",
        "Did you know that the word 'gullible' isn't in the dictionary?",
        "Death is the one thing we can all be sure of. Embrace it.",
        "I'm not saying you're going to die, but you're going to die.",
    };
}