using NoireLib.Helpers;
using System;

namespace PuppetMaster_Enhanced;

[Serializable]
public class BlacklistedPlayer
{
    public readonly string Id;
    public string PlayerName = string.Empty;
    public string PlayerWorld = string.Empty;
    public bool Enabled = true;
    public bool StrictPlayerName = true;

    public BlacklistedPlayer(string name = "", string homeworld = "")
    {
        PlayerName = name;
        PlayerWorld = homeworld;
        Id = RandomGenerator.GenerateGuidString();
    }
}