using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.EmoteMod
{
    public static class DebugCommands
    {
        [Command("e", "[subcommand] [arg] (might crash the game i warned you)")]
        public static void E(string custom, string emote)
        {
            if (!string.IsNullOrWhiteSpace(emote))
            {
                int customInt;
                int.TryParse(custom, out customInt);
                int emoteInt;
                int.TryParse(emote, out emoteInt);

                if (EmoteModMain.debug)
                    Logger.Log("-----COMMAND-----", $"e {custom} {emote}");

                // this is where emotes should be
                if (custom == "c" || custom == "custom") // custom emotes
                {
                    EmoteModule.Emote(emote, true);
                }
                else if (custom == "toggle" || custom == "t") // toggles
                {
                    // debug (desant r u dum cant u read)
                    if (emote == "debug" || emote == "d")
                        EmoteModMain.debug = !EmoteModMain.debug;
                    // toggle gravity
                    if (emote == "gravity" || emote == "g")
                    {
                        EmoteModMain.Settings.CancelGravity = !EmoteModMain.Settings.CancelGravity;
                        EmoteModMain.echo($"toggled gravity");
                        EmoteModMain.Instance.SaveSettings();
                        EmoteModMain.Instance.LoadSettings();
                    }
                    // print current animation
                    if (emote == "i")
                    {
                        Engine.Commands.Log($"current animation: {EmoteModMain.player.Sprite.CurrentAnimationID}");
                    }
                    // print current state
                    if (emote == "s")
                    {
                        Engine.Commands.Log(EmoteModMain.player.StateMachine.State);
                    }
                    // dump animations of current sprite mode to log file
                    if (emote == "dump")
                    {
                        foreach (KeyValuePair<string, Sprite.Animation> animation in EmoteModMain.player.Sprite.Animations)
                        {
                            Logger.Log("ANIMAAAAAAAATIOOOOOOOONSSSSSSSSS", animation.Key);
                        }
                    }
                    // tobble backpack
                    if (emote == "bp")
                    {
                        if (EmoteModMain.Settings.Backpack + 1 > 2)
                            EmoteModMain.Settings.Backpack = 0;
                        else
                            EmoteModMain.Settings.Backpack++;
                        switch (EmoteModMain.Settings.Backpack)
                        {
                            case 0:
                                EmoteModMain.player.ResetSprite(BackpackModule.player.DefaultSpriteMode);
                                EmoteModMain.echo("backpack default");
                                break;
                            case 1:
                                EmoteModMain.player.ResetSprite(PlayerSpriteMode.Madeline);
                                EmoteModMain.echo("backpack force on");
                                break;
                            case 2:
                                EmoteModMain.player.ResetSprite(PlayerSpriteMode.MadelineNoBackpack);
                                EmoteModMain.echo("backpack force off");
                                break;
                        }
                        EmoteModMain.Instance.SaveSettings();
                        EmoteModMain.Instance.LoadSettings();
                    }
                    if (emote == "funnycommand" || emote == "fc")
                    {
                        if (EmoteModMain.Settings.Backpack != 3)
                        {
                            EmoteModMain.Settings.Backpack = 3;
                            EmoteModMain.player.ResetSprite(PlayerSpriteMode.Playback);
                            EmoteModMain.echo("W H I T E L I N E  A C T I V A T E D");
                        }
                        else
                        {
                            EmoteModMain.Settings.Backpack = 0;
                            EmoteModMain.player.ResetSprite(BackpackModule.player.DefaultSpriteMode);
                            EmoteModMain.echo("backpack default");
                        }
                        EmoteModMain.Instance.SaveSettings();
                        EmoteModMain.Instance.LoadSettings();
                    }
                }
                else if (int.TryParse(custom, out customInt) && customInt >= 0 && customInt <= 9)
                {
                    switch (customInt)
                    {
                        case 0:
                            EmoteModMain.Settings.emote0 = emote;
                            break;
                        case 1:
                            EmoteModMain.Settings.emote1 = emote;
                            break;
                        case 2:
                            EmoteModMain.Settings.emote2 = emote;
                            break;
                        case 3:
                            EmoteModMain.Settings.emote3 = emote;
                            break;
                        case 4:
                            EmoteModMain.Settings.emote4 = emote;
                            break;
                        case 5:
                            EmoteModMain.Settings.emote5 = emote;
                            break;
                        case 6:
                            EmoteModMain.Settings.emote6 = emote;
                            break;
                        case 7:
                            EmoteModMain.Settings.emote7 = emote;
                            break;
                        case 8:
                            EmoteModMain.Settings.emote8 = emote;
                            break;
                        case 9:
                            EmoteModMain.Settings.emote9 = emote;
                            break;
                    }
                    EmoteModMain.echo($"assigned {emote} to numpad {customInt}");
                    EmoteModMain.Instance.SaveSettings();
                    EmoteModMain.Instance.LoadSettings();
                }
                else if (custom == "x" && int.TryParse(emote, out emoteInt))
                {
                    EmoteModMain.player.Sprite.Scale.X = emoteInt;
                }
                else if (custom == "y" && int.TryParse(emote, out emoteInt))
                {
                    EmoteModMain.player.Sprite.Scale.Y = emoteInt;
                }
                else
                {
                    EmoteModMain.echo($"failed to execute e {custom} {emote}. check your spelling");
                }
            }
        }
    }
}
