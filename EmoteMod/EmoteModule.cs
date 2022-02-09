using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteModule
    {
        // private static Player player;

        // any sprite mode
        private static List<string> true_neutral_emotes = new List<string>() { "b", "bounce", "idle", "idleA", "idleB", "idleC", "lookUp", "runSlow", "runStumble", "dreamDashIn", "dreamDashOut", "fallSlow", "fallFast", "climbLookBackStart", "faint", "flip", "walk", "push", "runFast", "dash", "dreamDashLoop", "slide", "jumpSlow", "jumpFast", "tired", "wallslide", "climbLookBack", "climbup", "duck", "edge", "fainted", "skid", "dangling", "spin" };
        // can be used with both bp and no bp
        private static List<string> neutral_emotes = new List<string>() { "runSlow_carry", "fallSlow_carry", "pickUp", "throw", "tiredStill", "climbPush", "climbPull", "fallPose", "deadside", "deadup", "deaddown", "startStarFly", "fall", "bigFallRecover", "bagdown", "asleep", "wakeUp", "halfWakeUp", "starMorph", "carryTheoCollapse", "tentacle_grab", "sitDown", "launchRecover", "idle_carry", "jumpSlow_carry", "runWind", "dash", "edgeBack", "swimIdle", "swimUp", "swimDown", "starFly", "bubble", "shaking", "hug", "starMorphIdle", "carryTheoWalk", "tentacle_grabbed", "tentacle_pull", "tentacle_dangling" };
        // no bp only
        private static List<string> maddy_emotes_no_bp = new List<string>() { "sleep", "bigFall", "launch", "roll", "rollGetUp", "downed" };
        // only badeline
        private static List<string> baddy_emotes = new List<string>() { "laugh", "spawn", "angry", "pretenddead", "boost" };

        public static bool bounced = false;
        public static bool playback = false;

        // public static bool changedSprite;

        public static void Emote(string animation, bool by_command, Player player)
        {
            if (EmoteModMain.anim_by_game != 2) // if the game is not playing a cutscene
            {
                try // anticrash3000
                {
                    player.StateMachine.State = Player.StDummy; // make player not able to move
                    player.DummyAutoAnimate = false; // make player not to auto animations
                    player.Speed = Vector2.Zero; // stop the player

                    GravityModule.playerY = player.Y; // record player y for the gravity switch

                    // if we were not doing an emote before, save default interactions state
                    if (EmoteModMain.anim_by_game == 0)
                    {
                        EmoteCancelModule.interactDefault = EmoteModMain.celestenetSettings.Interactions; // (because if we record it during an emote its just going to be false)
                        EmoteCancelModule.invincibilityDefault = SaveData.Instance.Assists.Invincible;
                        SaveData.Instance.Assists.Invincible = true;
                        // make playback emotes work
                        if (player.Sprite.Mode == PlayerSpriteMode.Playback)
                            playback = true;
                    }

                    // new sprite changes
                    if (!player.Sprite.Animations.ContainsKey(animation))
                    {
                        Dictionary<string, Sprite.Animation>.KeyCollection madeline_bp = GFX.SpriteBank.SpriteData["player"].Sprite.Animations.Keys;
                        Dictionary<string, Sprite.Animation>.KeyCollection madeline_no_bp = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations.Keys;
                        Dictionary<string, Sprite.Animation>.KeyCollection madeline_badeline = GFX.SpriteBank.SpriteData["player_badeline"].Sprite.Animations.Keys;
                        Dictionary<string, Sprite.Animation>.KeyCollection badeline = GFX.SpriteBank.SpriteData["badeline"].Sprite.Animations.Keys;
                        Dictionary<string, Sprite.Animation>.KeyCollection madeline_playback = GFX.SpriteBank.SpriteData["player_playback"].Sprite.Animations.Keys;

                        // change sprite if animation not found
                        if (madeline_no_bp.Contains(animation, StringComparer.OrdinalIgnoreCase))
                        {
                            player.ResetSprite(PlayerSpriteMode.MadelineNoBackpack);
                        }
                        else if (badeline.Contains(animation, StringComparer.OrdinalIgnoreCase))
                        {
                            player.ResetSprite(PlayerSpriteMode.Badeline);
                        }
                        else if (madeline_bp.Contains(animation, StringComparer.OrdinalIgnoreCase))
                        {
                            player.ResetSprite(PlayerSpriteMode.Madeline);
                        }
                    }
                    // bounc e
                    if (animation == "bounce" || animation == "b")
                    {
                        if (!bounced)
                            GravityModule.playerY -= 1;
                        player.Sprite.Play("spin");
                        SpeedModule.currentDelay = player.Sprite.Animations["spin"].Delay;
                        bounced = true;
                    }
                    else
                    {
                        player.Sprite.Play(animation); // do emote
                        SpeedModule.currentDelay = player.Sprite.Animations[animation].Delay;
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



        internal static void Load()
        {
            On.Celeste.Player.Update += Player_Update;

            //changedSprite = false; // uh
        }
        internal static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
        }

        public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (EmoteModMain.anim_by_game == 1)
            {
                if (Input.MoveX == 1)
                    self.Facing = Facings.Right;
                if (Input.MoveX == -1)
                    self.Facing = Facings.Left;
            }

            if (EmoteModMain.Settings.button0.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button0.Keys[0]) || EmoteModMain.Settings.button0.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button0.Buttons[0]))
                Emote(EmoteModMain.Settings.emote0, false, self);
            else if (EmoteModMain.Settings.button1.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button1.Keys[0]) || EmoteModMain.Settings.button1.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button1.Buttons[0]))
                Emote(EmoteModMain.Settings.emote1, false, self);
            else if (EmoteModMain.Settings.button2.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button2.Keys[0]) || EmoteModMain.Settings.button2.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button2.Buttons[0]))
                Emote(EmoteModMain.Settings.emote2, false, self);
            else if (EmoteModMain.Settings.button3.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button3.Keys[0]) || EmoteModMain.Settings.button3.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button3.Buttons[0]))
                Emote(EmoteModMain.Settings.emote3, false, self);
            else if (EmoteModMain.Settings.button4.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button4.Keys[0]) || EmoteModMain.Settings.button4.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button4.Buttons[0]))
                Emote(EmoteModMain.Settings.emote4, false, self);
            else if (EmoteModMain.Settings.button5.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button5.Keys[0]) || EmoteModMain.Settings.button5.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button5.Buttons[0]))
                Emote(EmoteModMain.Settings.emote5, false, self);
            else if (EmoteModMain.Settings.button6.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button6.Keys[0]) || EmoteModMain.Settings.button6.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button6.Buttons[0]))
                Emote(EmoteModMain.Settings.emote6, false, self);
            else if (EmoteModMain.Settings.button7.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button7.Keys[0]) || EmoteModMain.Settings.button7.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button7.Buttons[0]))
                Emote(EmoteModMain.Settings.emote7, false, self);
            else if (EmoteModMain.Settings.button8.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button8.Keys[0]) || EmoteModMain.Settings.button8.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button8.Buttons[0]))
                Emote(EmoteModMain.Settings.emote8, false, self);
            else if (EmoteModMain.Settings.button9.Keys.Count != 0 && MInput.Keyboard.Pressed(EmoteModMain.Settings.button9.Keys[0]) || EmoteModMain.Settings.button9.Buttons.Count != 0 && MInput.GamePads[0].Pressed(EmoteModMain.Settings.button9.Buttons[0]))
                Emote(EmoteModMain.Settings.emote9, false, self);


        }
    }
}
