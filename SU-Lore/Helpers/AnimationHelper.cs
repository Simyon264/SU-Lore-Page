using System.Reflection;

namespace SU_Lore.Helpers;

public class AnimationHelper
{
    private List<Animation.Animation> Animations { get; } = new();

    public AnimationHelper()
    {
        // Use reflection to find all classes that inherit from Animation
        var animationTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Animation.Animation)));

        foreach (var animationType in animationTypes)
        {
            var instance = Activator.CreateInstance(animationType);
            if (instance is not Animation.Animation animation)
            {
                throw new InvalidOperationException($"Type {animationType.FullName} does not inherit from Animation."); // fancy null check
            }

            Animations.Add(animation);
        }
    }

    /// <summary>
    /// Returns a pooled instance of an animation from its name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Animation.Animation GetAnimationFromName(string name)
    {
        var animation = Animations.FirstOrDefault(animation => animation.GetType().Name == name);
        if (animation == null)
        {
            throw new InvalidOperationException($"No animation found with the name {name}.");
        }

        return animation;
    }

    /// <summary>
    /// Returns a fresh instance of an animation from its name.
    /// </summary>
    public Animation.Animation GetAnimationFreshInstanceFromName(string name)
    {
        var animationType = Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.Name == name && t.IsSubclassOf(typeof(Animation.Animation)));

        if (animationType == null)
        {
            throw new InvalidOperationException($"No animation found with the name {name}.");
        }

        var instance = Activator.CreateInstance(animationType);
        if (instance is not Animation.Animation animation)
        {
            throw new InvalidOperationException($"Type {animationType.FullName} does not inherit from Animation.");
        }

        return animation;
    }
}