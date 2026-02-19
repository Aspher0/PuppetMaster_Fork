using NoireLib.Configuration;
using System;
using System.Collections.Generic;

namespace PuppetMaster_Enhanced;

[Serializable]
public class Configuration : NoireConfigBase<Configuration>
{
    public override int Version { get; set; } = 1;

    public override string GetConfigFileName() => "PuppetMasterConfig";

    [AutoSave]
    public virtual string DefaultTriggerPhrase { get; set; } = string.Empty;

    [AutoSave]
    public virtual bool DefaultAllowSit { get; set; } = false;

    [AutoSave]
    public virtual bool EnablePlugin { get; set; } = true;

    [AutoSave]
    public virtual bool EnableWhitelist { get; set; } = true;

    [AutoSave]
    public virtual bool EnableBlacklist { get; set; } = true;

    [AutoSave]
    public virtual bool DefaultMotionOnly { get; set; } = false;

    [AutoSave]
    public virtual bool DefaultAllowAllCommands { get; set; } = false;

    [AutoSave]
    public virtual bool DefaultUseRegex { get; set; } = false;

    [AutoSave]
    public virtual string DefaultCustomPhrase { get; set; } = string.Empty;

    [AutoSave]
    public virtual string DefaultReplaceMatch { get; set; } = string.Empty;

    [AutoSave]
    public virtual string DefaultTestInput { get; set; } = string.Empty;

    public List<ChannelSetting> DefaultEnabledChannels { get; set; } = new List<ChannelSetting>();

    public List<BlacklistedPlayer> BlacklistedPlayers { get; set; } = new List<BlacklistedPlayer>();

    public List<WhitelistedPlayer> WhitelistedPlayers { get; set; } = new List<WhitelistedPlayer>();
}