using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Base class for enemy behavior, stats, activation, and combat interactions.
/// </summary>
[CreateAssetMenu(fileName = "Character Visual Info", menuName = "Entities/Player/New Character Visual")]
public class PlayerVisualData : ScriptableObject
{
    public Sprite playerHudIcon;
    public AnimatorController playerAnimations;

    [Range(0,10)] public ParticleSystem[] particles;
}