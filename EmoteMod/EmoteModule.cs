using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteModule
    {
        private static Player player;

        // any sprite mode
        private static string[] true_neutral_emotes = { "idle", "idleA", "idleB", "idleC", "lookUp", "runSlow", "runStumble", "dreamDashIn", "dreamDashOut", "fallSlow", "fallFast", "climbLookBackStart", "faint", "flip", "walk", "push", "runFast", "dash", "dreamDashLoop", "slide", "jumpSlow", "jumpFast", "tired", "wallslide", "climbLookBack", "climbup", "duck", "edge", "fainted", "skid", "dangling", "spin" };
        // can be used with both bp and no bp
        private static string[] neutral_emotes = { "runSlow_carry", "fallSlow_carry", "pickUp", "throw", "tiredStill", "climbPush", "climbPull", "fallPose", "deadside", "deadup", "deaddown", "startStarFly", "fall", "bigFallRecover", "bagdown", "asleep", "wakeUp", "halfWakeUp", "starMorph", "carryTheoCollapse", "tentacle_grab", "sitDown", "launchRecover", "idle_carry", "jumpSlow_carry", "runWind", "dash", "edgeBack", "swimIdle", "swimUp", "swimDown", "starFly", "bubble", "shaking", "hug", "starMorphIdle", "carryTheoWalk", "tentacle_grabbed", "tentacle_pull", "tentacle_dangling" };
        // no bp only
        private static string[] maddy_emotes_no_bp = { "sleep", "bigFall", "launch", "roll", "rollGetUp", "downed" };
        // only badeline
        private static string[] baddy_emotes = { "laugh", "spawn", "angry", "pretenddead", "boost" };

        public static void Emote(string animation, bool by_command)
        {
            // another way to toggle debug so that you can bind it to numpad
            if (animation == "log")
                EmoteModMain.debug = !EmoteModMain.debug;
            else if (EmoteModMain.anim_by_game != 2) // if the game is not playing a cutscene
            {
                try // anticrash3000
                {
                    player.StateMachine.State = Player.StDummy; // make player not able to move
                    player.DummyAutoAnimate = false; // make player not to auto animations
                    player.Speed = Vector2.Zero; // stop the player
                    EmoteModMain.playerY = player.Y; // record player y for the gravity switch
                    if (EmoteModMain.anim_by_game == 0) // if we were not doing an emote before, save default interactions state
                        EmoteModMain.interactDefault = EmoteModMain.celestenetSettings.Interactions; // (because if we record it during an emote its just going to be false)
                    EmoteModMain.celestenetSettings.Interactions = false; // disable interations for everyone's sake
                    if (EmoteModMain.anim_by_game == 0)
                        EmoteModMain.invincibilityDefault = SaveData.Instance.Assists.Invincible;
                    SaveData.Instance.Assists.Invincible = true;

                    #region sprite changes

                    do
                    {
                        bool break_ = false;

                        // make whiteline have spin and yea
                        if (player.Sprite.Mode == PlayerSpriteMode.Playback)
                            if (animation.ToLower() == "sitdown" || animation.ToLower() == "launch")
                                break;
                        // if we play bounce skip the sprite thingy
                        if (animation == "bounce" || animation == "b")
                            break;
                        // dont do anything if true neutral
                        foreach (string s in true_neutral_emotes)
                        {
                            if (s.ToLower() == animation.ToLower())
                            {
                                // return to whiteline
                                if (EmoteModMain.Settings.Backpack == 3)
                                {
                                    player.ResetSprite(PlayerSpriteMode.Playback);
                                }

                                // baddy seems to not have idleA-C so we do this
                                if (player.Sprite.Mode == PlayerSpriteMode.Badeline)
                                {
                                    if (animation.ToLower() == "idlea" || animation.ToLower() == "idleb" || animation.ToLower() == "idlec")
                                    {
                                        player.ResetSprite(SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                                        EmoteModMain.changedSprite = false;
                                    }
                                }
                                break_ = true;
                                break;
                            }
                        }
                        if (break_)
                            break;

                        // if need to turn off bp
                        if (!SaveData.Instance.Assists.PlayAsBadeline)
                            foreach (string s in maddy_emotes_no_bp)
                            {
                                if (animation.ToLower() == s.ToLower())
                                {
                                    player.ResetSprite(PlayerSpriteMode.MadelineNoBackpack);
                                    EmoteModMain.changedSprite = true;
                                    break_ = true;
                                    break;
                                }
                            }
                        if (break_)
                            break;

                        // if baddy
                        foreach (string s in baddy_emotes)
                        {
                            if (animation.ToLower() == s.ToLower())
                            {
                                player.ResetSprite(PlayerSpriteMode.Badeline);
                                EmoteModMain.changedSprite = true;
                                break_ = true;
                                break;
                            }
                        }
                        if (break_)
                            break;
                        // else switch to default
                        player.ResetSprite(SaveData.Instance.Assists.PlayAsBadeline ? PlayerSpriteMode.MadelineAsBadeline : player.DefaultSpriteMode);
                        EmoteModMain.changedSprite = false;

                    } while (false);

                    #endregion
                        
                    // bounc e
                    if (animation == "bounce" || animation == "b")
                    {
                        EmoteModMain.playerY -= 1;
                        player.Sprite.Play("spin");
                        EmoteModMain.currentDelay = player.Sprite.Animations["spin"].Delay;
                    }
                    else
                    {
                        player.Sprite.Play(animation); // do emote
                        EmoteModMain.currentDelay = player.Sprite.Animations[animation].Delay;
                    }

                    if (by_command) // command reply only if done by command
                        EmoteModMain.echo($"playing {animation}");

                    EmoteModMain.anim_by_game = 1; // acknowledge that emote is playing
                }
                catch (Exception e)
                {
                    Logger.Log("EmoteMod EXCEPTION", e.ToString()); // burh
                    EmoteModMain.echo($"failed to play {animation}");
                    EmoteCancelModule.cancelEmote();
                }
            }
        }

        public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
            player = self;

            if (EmoteModMain.Settings.button0.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button0.Keys[0]) || EmoteModMain.Settings.button0.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button0.Buttons[0]))
                Emote(EmoteModMain.Settings.emote0, false);
            else if (EmoteModMain.Settings.button1.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button1.Keys[0]) || EmoteModMain.Settings.button1.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button1.Buttons[0]))
                Emote(EmoteModMain.Settings.emote1, false);
            else if (EmoteModMain.Settings.button2.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button2.Keys[0]) || EmoteModMain.Settings.button2.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button2.Buttons[0]))
                Emote(EmoteModMain.Settings.emote2, false);
            else if (EmoteModMain.Settings.button3.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button3.Keys[0]) || EmoteModMain.Settings.button3.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button3.Buttons[0]))
                Emote(EmoteModMain.Settings.emote3, false);
            else if (EmoteModMain.Settings.button4.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button4.Keys[0]) || EmoteModMain.Settings.button4.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button4.Buttons[0]))
                Emote(EmoteModMain.Settings.emote4, false);
            else if (EmoteModMain.Settings.button5.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button5.Keys[0]) || EmoteModMain.Settings.button5.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button5.Buttons[0]))
                Emote(EmoteModMain.Settings.emote5, false);
            else if (EmoteModMain.Settings.button6.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button6.Keys[0]) || EmoteModMain.Settings.button6.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button6.Buttons[0]))
                Emote(EmoteModMain.Settings.emote6, false);
            else if (EmoteModMain.Settings.button7.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button7.Keys[0]) || EmoteModMain.Settings.button7.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button7.Buttons[0]))
                Emote(EmoteModMain.Settings.emote7, false);
            else if (EmoteModMain.Settings.button8.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button8.Keys[0]) || EmoteModMain.Settings.button8.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button8.Buttons[0]))
                Emote(EmoteModMain.Settings.emote8, false);
            else if (EmoteModMain.Settings.button9.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button9.Keys[0]) || EmoteModMain.Settings.button9.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button9.Buttons[0]))
                Emote(EmoteModMain.Settings.emote9, false);


        }
    }
}
