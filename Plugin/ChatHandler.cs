using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using NoireLib;
using NoireLib.Helpers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace PuppetMaster_Enhanced;

public class ChatHandler
{
    public static bool WhitelistPass(string clearFromPlayer, string clearFromWorld, out WhitelistedPlayer? foundWhitelistedPlayer)
    {
        foundWhitelistedPlayer = null;

        if (Configuration.Instance.EnableWhitelist && Configuration.Instance.WhitelistedPlayers.Count == 0)
            return false;

        foreach (var whitelistedPlayer in Configuration.Instance.WhitelistedPlayers)
        {
            if (IsPlayerWhitelisted(clearFromPlayer, clearFromWorld, whitelistedPlayer, out foundWhitelistedPlayer))
                return true;
        }

        return !Configuration.Instance.EnableWhitelist;
    }

    private static bool IsPlayerWhitelisted(string clearFromPlayer, string clearFromWorld, WhitelistedPlayer whitelistedPlayer, out WhitelistedPlayer? foundWhitelistedPlayer)
    {
        foundWhitelistedPlayer = null;

        if (!IsValidPlayerEntry(clearFromPlayer, clearFromWorld, whitelistedPlayer.PlayerName, whitelistedPlayer.PlayerWorld, whitelistedPlayer.Enabled))
            return false;

        bool playerNameMatch = whitelistedPlayer.StrictPlayerName
            ? string.Equals(clearFromPlayer, whitelistedPlayer.PlayerName.Trim(), StringComparison.OrdinalIgnoreCase)
            : CommonHelper.RegExpMatch(clearFromPlayer, whitelistedPlayer.PlayerName);

        bool playerWorldMatch = whitelistedPlayer.PlayerWorld.Trim() == "*" || string.Equals(clearFromWorld, whitelistedPlayer.PlayerWorld.Trim(), StringComparison.OrdinalIgnoreCase);

        if (playerNameMatch && playerWorldMatch)
        {
            foundWhitelistedPlayer = whitelistedPlayer;
            return true;
        }

        return false;
    }

    public static bool BlacklistPass(string ClearFromPlayer, string ClearFromWorld)
    {
        if (!Configuration.Instance.EnableBlacklist || Configuration.Instance.BlacklistedPlayers.Count == 0)
            return true;

        foreach (var blacklistedPlayer in Configuration.Instance.BlacklistedPlayers)
        {
            if (IsPlayerBlacklisted(ClearFromPlayer, ClearFromWorld, blacklistedPlayer))
                return false;
        }

        return true;
    }

    private static bool IsPlayerBlacklisted(string clearFromPlayer, string clearFromWorld, BlacklistedPlayer blacklistedPlayer)
    {
        if (!IsValidPlayerEntry(clearFromPlayer, clearFromWorld, blacklistedPlayer.PlayerName, blacklistedPlayer.PlayerWorld, blacklistedPlayer.Enabled))
            return false;

        bool playerNameMatch = blacklistedPlayer.StrictPlayerName
            ? string.Equals(clearFromPlayer, blacklistedPlayer.PlayerName.Trim(), StringComparison.OrdinalIgnoreCase)
            : CommonHelper.RegExpMatch(clearFromPlayer, blacklistedPlayer.PlayerName);

        bool playerWorldMatch = blacklistedPlayer.PlayerWorld.Trim() == "*" || string.Equals(clearFromWorld, blacklistedPlayer.PlayerWorld.Trim(), StringComparison.OrdinalIgnoreCase);

        return playerNameMatch && playerWorldMatch;
    }

    public static bool IsChannelEnabled(XivChatType type, List<ChannelSetting> Channels)
    {
        foreach (var enabledChannel in Channels)
        {
            if (enabledChannel.ChatType == type && enabledChannel.Enabled)
                return true;
        }

        return false;
    }

    public static void DoCommand(XivChatType type, string message, string sender, string sender_world)
    {
        string ClearFromPlayer = sender.Trim().ToLower();
        string ClearFromWorld = sender_world.Trim().ToLower();

        if (ClearFromPlayer.IsNullOrWhitespace() ||
            !Configuration.Instance.EnablePlugin ||
            !BlacklistPass(ClearFromPlayer, ClearFromWorld) ||
            !WhitelistPass(ClearFromPlayer, ClearFromWorld, out WhitelistedPlayer? foundWhitelistedPlayer))
            return;

        bool useAllDefaultSettings = (foundWhitelistedPlayer == null) || foundWhitelistedPlayer.UseAllDefaultSettings;
        bool useDefaultTrigger = (foundWhitelistedPlayer == null) || foundWhitelistedPlayer.UseDefaultTrigger;
        bool useDefaultRequests = (foundWhitelistedPlayer == null) || foundWhitelistedPlayer.UseDefaultRequests;
        bool useDefaultEnabledChannels = (foundWhitelistedPlayer == null) || foundWhitelistedPlayer.UseDefaultEnabledChannels;

        if (!IsChannelEnabled(type, (useAllDefaultSettings || useDefaultEnabledChannels) ? Configuration.Instance.DefaultEnabledChannels : foundWhitelistedPlayer.EnabledChannels))
            return;

        bool flag1 = (!useAllDefaultSettings && !useDefaultTrigger) ? foundWhitelistedPlayer.UseRegex && foundWhitelistedPlayer.CustomRx != null : Configuration.Instance.DefaultUseRegex && Service.CustomRx != null;
        MatchCollection matchCollection = flag1 ? ((!useAllDefaultSettings && !useDefaultTrigger) ? foundWhitelistedPlayer.CustomRx.Matches(message) : Service.CustomRx.Matches(message)) : ((!useAllDefaultSettings && !useDefaultTrigger) ? foundWhitelistedPlayer.Rx.Matches(message) : Service.Rx.Matches(message));

        if (matchCollection.Count == 0)
        {
            return;
        }

        string command = string.Empty;

        try
        {
            command = flag1 ? ((!useAllDefaultSettings && !useDefaultTrigger) ? foundWhitelistedPlayer.CustomRx.Replace(matchCollection[0].Value, foundWhitelistedPlayer.ReplaceMatch) : Service.CustomRx.Replace(matchCollection[0].Value, Configuration.Instance.DefaultReplaceMatch)) : ((!useAllDefaultSettings && !useDefaultTrigger) ? foundWhitelistedPlayer.Rx.Replace(matchCollection[0].Value, foundWhitelistedPlayer.GetDefaultReplaceMatch()) : Service.Rx.Replace(matchCollection[0].Value, Service.GetDefaultReplaceMatch()));
        }
        catch (Exception ex)
        {
            NoireLogger.LogError(ex, "[PuppetMaster] [Error] Regex error while listening for command");
        }

        Service.ParsedTextCommand parsedTextCommand = Service.FormatCommand(command);

        if (string.IsNullOrEmpty(parsedTextCommand.Main))
        {
            return;
        }

        var foundEmote = EmoteHelper.GetEmoteByCommand(parsedTextCommand.Main);
        bool flag2 = foundEmote != null;

        if (flag2)
        {
            if ((parsedTextCommand.Main == "/sit" || parsedTextCommand.Main == "/groundsit" || parsedTextCommand.Main == "/lounge") && ((!useAllDefaultSettings && !useDefaultRequests) ? !foundWhitelistedPlayer.AllowSit : !Configuration.Instance.DefaultAllowSit))
            {
                parsedTextCommand.Main = "/no";
            }

            if ((!useAllDefaultSettings && !useDefaultRequests) ? foundWhitelistedPlayer.MotionOnly : Configuration.Instance.DefaultMotionOnly)
            {
                parsedTextCommand.Args = "motion";
            }
        }

        if ((!useAllDefaultSettings && !useDefaultRequests) ? !(foundWhitelistedPlayer.AllowAllCommands | flag2) : !(Configuration.Instance.DefaultAllowAllCommands | flag2))
        {
            return;
        }

        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
        interpolatedStringHandler.AppendFormatted(parsedTextCommand);
        ChatHelper.SendMessage(interpolatedStringHandler.ToStringAndClear());
    }

    public static void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var senderResolved = SeStringHelper.ResolveSender(sender);

        if (senderResolved != null)
            DoCommand(type, message.ToString(), senderResolved.PlayerName, senderResolved.Homeworld);
    }

    private static bool IsValidPlayerEntry(string player, string world, string entryName, string entryWorld, bool enabled)
    {
        return !string.IsNullOrWhiteSpace(player)
            && !string.IsNullOrWhiteSpace(entryName)
            && !string.IsNullOrWhiteSpace(world)
            && !string.IsNullOrWhiteSpace(entryWorld)
            && enabled;
    }
}
