using Monocle;
using System.Collections.Generic;
using System;
using System.Numerics;
using System.Reflection;
using MonoMod.Cil;
using EmoteMod.Utility;
using EmoteMod.Features;
using Celeste;


namespace EmoteMod.Module
{
    public static class DebugCommands
    {
        public static string ass = "";
        [Command("e", "[subcommand] [arg] (might crash the game i warned you)")]
        public static void E(string custom, string emote)
        {
            if (!string.IsNullOrWhiteSpace(emote))
            {
                int customInt;
                int.TryParse(custom, out customInt);
                float emoteFloat; bool isFloat;
                isFloat = float.TryParse(emote, out emoteFloat);
                int emoteInt;
                int.TryParse(emote, out emoteInt);

                // this is where emotes should be
                if (custom == "c" || custom == "custom") // custom emotes
                {
                    // Emote.DoEmote(emote, true, PlayerHelper.GetPlayer()); // TODO
                }
                else if (custom == "toggle" || custom == "t") // toggles
                {
                    // toggle gravity
                    if (emote == "gravity" || emote == "g")
                    {
                        EmoteModModule.Settings.CancelGravity = !EmoteModModule.Settings.CancelGravity;
                        EmoteModModule.echo($"toggled gravity");
                        EmoteModModule.Instance.SaveSettings();
                        EmoteModModule.Instance.LoadSettings();
                    }
                    // print current animation
                    else if (emote == "i")
                    {
                        Engine.Commands.Log($"current animation: {PlayerHelper.GetPlayer().Sprite.CurrentAnimationID}");
                    }
                    // print current state
                    else if (emote == "s")
                    {
                        Engine.Commands.Log(PlayerHelper.GetPlayer().StateMachine.State);
                    }
                    // dump animations of current sprite mode to log file
                    else if (emote == "dump")
                    {
                        string temp = "animations: ";
                        foreach (KeyValuePair<string, Sprite.Animation> animation in PlayerHelper.GetPlayer().Sprite.Animations)
                        {
                            temp += animation.Key + ", ";
                        }
                        EmoteModModule.echo(temp);
                    }
                    // tobble backpack
                    else if (emote == "bp")
                    {
                        BackpackChanger.ScrollBackpack();
                    }

                    // haha funny
                    else if (emote == "funnycommand" || emote == "fc")
                    {
                        BackpackChanger.EnterSickoMode();
                    }
                }
                // binding emotes with console
                else if (int.TryParse(custom, out customInt) && customInt >= 0 && customInt <= 9)
                {
                    switch (customInt)
                    {
                        case 0:
                            EmoteModModule.Settings.emote0 = emote;
                            break;
                        case 1:
                            EmoteModModule.Settings.emote1 = emote;
                            break;
                        case 2:
                            EmoteModModule.Settings.emote2 = emote;
                            break;
                        case 3:
                            EmoteModModule.Settings.emote3 = emote;
                            break;
                        case 4:
                            EmoteModModule.Settings.emote4 = emote;
                            break;
                        case 5:
                            EmoteModModule.Settings.emote5 = emote;
                            break;
                        case 6:
                            EmoteModModule.Settings.emote6 = emote;
                            break;
                        case 7:
                            EmoteModModule.Settings.emote7 = emote;
                            break;
                        case 8:
                            EmoteModModule.Settings.emote8 = emote;
                            break;
                        case 9:
                            EmoteModModule.Settings.emote9 = emote;
                            break;
                    }
                    EmoteModModule.echo($"assigned {emote} to numpad {customInt}");
                    EmoteModModule.Instance.SaveSettings();
                    EmoteModModule.Instance.LoadSettings();
                }
                else if (custom == "d")
                {
                    try
                    {
                        Dictionary<string, SpriteData> spr = GFX.SpriteBank.SpriteData;
                        string anims = "";

                        if (emote == "modes")
                            foreach (KeyValuePair<string, SpriteData> anim in spr)
                            {
                                anims += anim.Key + " ";
                            }
                        else if (emote == "avatars")
                        {

                            if (Engine.Scene is Level)
                            {
                                Level level = (Level)Engine.Scene;

                                foreach (Entity e in level.Entities)
                                {
                                    if (e is Celeste.Mod.CelesteNet.Client.Entities.Ghost) // this gets all ghists in the level
                                    {
                                        Celeste.Mod.CelesteNet.Client.Entities.Ghost ghost = (Celeste.Mod.CelesteNet.Client.Entities.Ghost)e;

                                        EmoteModModule.echo($"{ghost.NameTag.Name}, ");
                                    }
                                }
                            }

                        }
                        else
                            foreach (KeyValuePair<string, Sprite.Animation> anim in spr[emote].Sprite.Animations)
                            {
                                anims += anim.Key + " ";
                            }

                        foreach (EmoteEntry e in EmoteModModule.Settings.Emotes)
                            e.RefreshInfo();

                        EmoteModModule.echo(anims);

                    }
                    catch
                    {
                        EmoteModModule.echo("something went wrong");
                    }
                }

                // the stretches

                else if (custom == "x")
                {
                    Stretcher.stretch_x(emoteFloat);
                }
                else if (custom == "y")
                {
                    Stretcher.stretch_y(emoteFloat);
                }
                else if (custom == "xy")
                {
                    if (emote == "lock" || emote == "l")
                        Stretcher.lock_stretch();
                    else
                    {
                        Stretcher.stretch_x(emoteFloat);
                        Stretcher.stretch_y(emoteFloat);
                    }
                }
                else if (custom == "test")
                {
                    // EmoteModModule.echo(CNetHelper.test);
                    string test = "";

                    try
                    {
                        // test += "properties:\n";
                        // foreach (PropertyInfo p in CNetHelper.CNMainComponent?.GetType().GetProperties())
                        // {
                        //     test += $"{p.Name} ::: {p.GetType()}\n";
                        // }
                        // test += "fields:\n";
                        // foreach (FieldInfo p in CNetHelper.CNMainComponent?.GetType().GetFields())
                        // {
                        //     test += $"{p.Name} ::: {p.GetType()}\n";
                        // }
                        // CNetHelper.CNInstance.ToString();
                        // test += "we got instance\n";
                        // CNetHelper.CNContext.ToString();
                        // test += "we got context\n";
                        // CNetHelper.CNMainComponent.ToString();
                        // test += "we got main\n";
                        // test += "no errors?!??!\n";
                        //
                        // CNetHelper.CNMainComponent?.GetType().GetField("StateUpdated")?.SetValue(CNetHelper.CNMainComponent, true);
                        // test += "HOLY SHIT??";
                        //
                        // EmoteModModule.echo(Speed.test);

                        // EmoteModModule.echo("fields: " + typeof(Player).GetFields().Aggregate((i, j) => i.ToString() + ", " + j.ToString()));
                        // string text = "Fields: ";
                        // foreach (FieldInfo f in typeof(PlayerSprite).GetFields())
                        // {
                        //     text += f.Name + ", ";
                        // }
                        // text += "\n\n";
                        // foreach (PropertyInfo f in typeof(PlayerSprite).GetProperties())
                        // {
                        //     text += f.Name + ", ";
                        // }
                        // EmoteModModule.echo(text);
                        // EmoteModModule.echo(GFX.SpriteBank.SpriteData.ContainsKey(emote).ToString());
                        // PlayerSprite plr = PlayerHelper.GetPlayer().Sprite;
                        // EmoteModModule.echo($"w:{plr.Width} h:{plr.Height}");

                        // foreach (KeyValuePair<String, SpriteData> spr in GFX.SpriteBank.SpriteData)
                        // {
                        //     foreach (string anim in spr.Value.Sprite.Animations.Keys)
                        //     {
                        //         if (anim.Contains(":"))
                        //         {
                        //             test += $"(name:'{anim}', spritebank:'{spr.Key}'), ";
                        //         }
                        //     }
                        //
                        // }
                        // EmoteModModule.echo($"FIUCK YOU: {test}");
                        //
                        // ButtonBinding butt = EmoteModModule.Settings.button0;
                        //
                        // EmoteModModule.echo($"keys: {butt.Keys}, joined: {String.Join(",", butt.Keys)}, length: {butt.Keys.Count}, first: {butt.Keys[0]}, button: {butt.Button}, bindex: {butt.Button.GamepadIndex}");
                        //
                        // // EmoteModModule.echo($"atlases: {String.Join(", ", GFX.SpriteBank.Atlas.Textures.Keys)}");
                        test = "";
                        // string searchterm = "core/01";
                        // foreach (string key in GFX.SpriteBank.Atlas.Textures.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        // foreach (string key in GFX.GuiSpriteBank.Atlas.Textures.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        // EmoteModModule.echo("this is from spritebank");
                        // foreach (string key in GFX.SpriteBank.SpriteData.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        //
                        // EmoteModModule.echo("this is not");
                        // foreach (string key in GFX.Gui.Textures.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        //
                        // foreach (string key in GFX.Misc.Textures.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        //
                        // foreach (string key in GFX.Game.Textures.Keys)
                        // {
                        //     if (key.Contains(searchterm))
                        //     {
                        //         test += key + ", ";
                        //     }
                        // }
                        //
                        // foreach (EmoteEntry e in EmoteModModule.Settings.Emotes)
                        // {
                        //     EmoteModModule.echo(e.animation);
                        //     e.RefreshInfo();
                        // }
                        //
                        // EmoteModModule.echo("sex dialog::");
                        //
                        // foreach (string k in Dialog.Language.Dialog.Keys)
                        // {
                        //     try
                        //     {
                        //         string g = Dialog.Clean(k);
                        //         string l = Dialog.Get(k);
                        //         if (g.ToLower().Contains("to clear buttons"))
                        //             EmoteModModule.echo($"key:'{k}', clean:'{g}', get:'{l}");
                        //     }
                        //     catch (Exception e)
                        //     {
                        //         EmoteModModule.echo(k + " errored out");
                        //     }
                        // }
                        // EmoteModModule.echo((GFX.SpriteBank == null).ToString());
                        //
                        // MTexture frame = GFX.SpriteBank.SpriteData["player"].Sprite.Animations["idle"].Frames[0];
                        //
                        // EmoteModModule.echo($"player:idle {frame.Width}x{frame.Height}");

                        // MTexture card => GFX.SpriteBank.Atlas.Textures;


                        // typeof(PlayerSprite).GetField("Scale");

                        MethodInfo[] mi = typeof(Player).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (MethodInfo m in mi)
                        {
                            if (m.Name.Contains("orig") || m.Name.Contains("Update") || m.Name.Contains("Render"))
                                test += m.Name + ", ";

                            if (m.GetMethodBody()?.ToString()?.Contains("Scale") ?? false)
                            {
                                EmoteModModule.echo(m.Name);
                                string[] lines = m.GetMethodBody().ToString().Split("\n");
                                foreach (string s in lines)
                                    if (s.Contains("stfld") && s.Contains("Scale"))
                                        EmoteModModule.echo(s);
                            }

                        }

                        EmoteModModule.echo(ass);

                        // EmoteModModule.echo(typeof(Player).GetMethod("UpdateSprite").GetMethodBody().ToString());





                    }
                    catch (Exception e)
                    {
                        test += $"EXCEPTION!??!? '{e}'";
                    }

                    EmoteModModule.echo(test);
                    // throw new Exception("lmao");

                }

                else
                {
                    EmoteModModule.echo($"failed to execute e {custom} {emote}. check your spelling");
                }
            }
        }


    }
}
