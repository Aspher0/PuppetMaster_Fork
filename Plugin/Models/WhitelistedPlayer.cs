using Newtonsoft.Json;
using NoireLib;
using NoireLib.Helpers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PuppetMaster_Enhanced;

[Serializable]
public class WhitelistedPlayer
{
    public string Id { get; init; } = RandomGenerator.GenerateGuidString();
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerWorld { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool StrictPlayerName { get; set; } = true;


    public bool UseAllDefaultSettings { get; set; } = true;
    public bool UseDefaultTrigger { get; set; } = false;
    public bool UseDefaultRequests { get; set; } = false;
    public bool UseDefaultEnabledChannels { get; set; } = false;


    public string TriggerPhrase { get; set; } = string.Empty;
    public string CustomPhrase { get; set; } = string.Empty;
    public string ReplaceMatch { get; set; } = string.Empty;
    public bool UseRegex { get; set; } = false;
    public string TestInput { get; set; } = string.Empty;


    public bool AllowSit { get; set; } = false;
    public bool MotionOnly { get; set; } = false;
    public bool AllowAllCommands { get; set; } = false;


    public Regex? Rx { get; set; }
    public Regex? CustomRx { get; set; }

    public Service.ParsedTextCommand TextCommand { get; set; } = new Service.ParsedTextCommand();

    public List<ChannelSetting> EnabledChannels { get; set; } = Service.GetDefaultChannelSettings();

    [JsonConstructor]
    public WhitelistedPlayer() { }

    public WhitelistedPlayer(string playerName, string playerHomeworld) : this()
    {
        PlayerName = playerName;
        PlayerWorld = playerHomeworld;
    }

    public void InitializeRegex(bool reload = false)
    {
        if (UseRegex)
        {
            if (string.IsNullOrEmpty(CustomPhrase))
            {
                CustomPhrase = GetDefaultRegex();
                ReplaceMatch = GetDefaultReplaceMatch();
                Configuration.Instance.Save();
                reload = true;
            }
            if (CustomRx == null || reload)
            {
                try
                {
                    CustomRx = new Regex(CustomPhrase);
                    Configuration.Instance.Save();
                }
                catch (Exception ex)
                {
                    NoireLogger.LogError(this, ex, $"Could not initialize Regex for Whitelist entry n°{Id}");
                }
            }
        }
        else
        {
            if (Rx == null || reload)
            {
                try
                {
                    Rx = new Regex(GetDefaultRegex());
                    Configuration.Instance.Save();
                }
                catch (Exception ex)
                {
                    NoireLogger.LogError(this, ex, $"Could not initialize Regex for Whitelist entry n°{Id}");
                }
            }
        }
    }

    public Service.ParsedTextCommand GetTestInputCommand()
    {
        InitializeRegex();
        return Service.GetTestInputCommand(TestInput, UseRegex, CustomRx, Rx, ReplaceMatch, GetDefaultReplaceMatch());
    }

    private string GetDefaultRegex() => Service.BuildRegexPattern(TriggerPhrase);

    public string GetDefaultReplaceMatch() => Service.GetDefaultReplaceMatch();
}
