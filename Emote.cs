using System;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.CelesteNet.Client.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.CelesteNet.DataTypes;

using System.Text;


using MonoMod.RuntimeDetour;

namespace Celeste.Mod.EmoteMod
{
    public class Emote
    {
        public static bool bounced = false;
        public static bool playback = false;

        private static Hook celestenetUpdateGraphicsHook;

        private static int defaultAnimationsCount;

        internal static Dictionary<string, Sprite.Animation> madeline_bp => GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
        internal static Dictionary<string, Sprite.Animation> madeline_no_bp => GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;
        internal static Dictionary<string, Sprite.Animation> madeline_badeline => GFX.SpriteBank.SpriteData["player_badeline"].Sprite.Animations;
        internal static Dictionary<string, Sprite.Animation> badeline => GFX.SpriteBank.SpriteData["badeline"].Sprite.Animations;
        internal static Dictionary<string, Sprite.Animation> madeline_playback => GFX.SpriteBank.SpriteData["player_playback"].Sprite.Animations;

        // public static bool changedSprite;

        public static void DoEmote(EmoteInfo info, bool by_command, Player player)
        // public static void DoEmote(string animation, bool by_command, Player player)
        {
            if (EmoteModModule.anim_by_game != 2) // if the game is not playing a cutscene
            {
                string animation = info.isCustom ? $"{info.spritebank}:{info.animation}" : info.animation;

                try // anticrash3000
                {
                    player.StateMachine.State = Player.StDummy; // make player not able to move
                    player.DummyAutoAnimate = false; // make player not to auto animations
                    player.Speed = Vector2.Zero; // stop the player


                    Gravity.playerY = player.Y; // record player y for the gravity switch

                    // if we were not doing an emote before, save default interactions state
                    if (EmoteModModule.anim_by_game == 0)
                    {
                        CNetHelper.InteractionsAllowed = false;
                        EmoteCancel.invincibilityDefault = SaveData.Instance.Assists.Invincible;
                        SaveData.Instance.Assists.Invincible = true; // TODO: wanna get rid of changing settings at all
                        EmoteModModule.anim_by_game = 1; // acknowledge that emote is playing

                        // make playback emotes work
                        if (player.Sprite.Mode == PlayerSpriteMode.Playback) // TODO: no if
                            playback = true;
                    }


                    // new sprite changes
                    if (animation != "b" && animation != "bounce")
                        if (info.changeSpriteMode)
                        {
                            if (PlayerHelper.GetPlayer()?.Sprite.Mode != info.spritemode)
                                player.ResetSprite(info.spritemode);

                            if (info.isCustom)
                                addCustomEmote(animation);
                        }
                        else if (!player.Sprite.Animations.ContainsKey(animation))
                        {
                            // change sprite if animation not found
                            if (madeline_no_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
                            {
                                player.ResetSprite(PlayerSpriteMode.MadelineNoBackpack);
                            }
                            else if (badeline.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
                            {
                                player.ResetSprite(PlayerSpriteMode.Badeline);
                            }
                            else if (madeline_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
                            {
                                player.ResetSprite(PlayerSpriteMode.Madeline);
                            }
                        }


                    // bounc e
                    if (info.animation == "bounce" || info.animation == "b")
                    {
                        if (!bounced)
                            Gravity.playerY -= 1;
                        player.Sprite.Play("spin");
                        // Speed.currentDelay = player.Sprite.Animations["spin"].Delay;
                        Speed.SetSpeed();
                        bounced = true;
                    }
                    else
                    {
                        player.Sprite.Play(animation); // do emote
                                                       // Speed.currentDelay = player.Sprite.Animations[animation].Delay;
                        Speed.SetSpeed();
                    }

                    if (by_command) // command reply only if done by command
                        EmoteModModule.echo($"playing {animation}");

                }
                catch (Exception e)
                {
                    Logger.Log("EmoteMod EXCEPTION", e.ToString()); // burh
                    EmoteModModule.echo($"failed to play {animation}");
                    EmoteCancel.cancelEmote();
                }
            }
        }



        public static bool addCustomEmote(string name)
        {
            char split = ':';
            // EmoteModModule.echo($"a, '{name}'");
            string test = "";
            foreach (int i in Encoding.ASCII.GetBytes(name))
            {
                test += i;
            }
            // EmoteModModule.echo(test);

            if (madeline_bp.ContainsKey(name))
                return false;

            if (!name.Contains(split))
                return false;

            string sdata_name = name.Split(split)[0];
            string anim_name = name.Split(split, 2)[1];

            if (!GFX.SpriteBank.SpriteData.ContainsKey(sdata_name))
                return false;

            Dictionary<string, Sprite.Animation> anims = GFX.SpriteBank.SpriteData[sdata_name].Sprite.Animations;

            if (!anims.ContainsKey(anim_name))
                return false;

            KeyValuePair<string, Sprite.Animation> newAnim = new KeyValuePair<string, Sprite.Animation>(anim_name, anims[anim_name]);
            madeline_bp.Add(name, copyAnim(newAnim, name));

            EmoteCancel.customEmotes.Add(name);
            return true;
        }

        private static Sprite.Animation copyAnim(KeyValuePair<string, Sprite.Animation> anim, string name)
        {
            Sprite.Animation ae = new Sprite.Animation();
            ae.Frames = anim.Value.Frames;
            ae.Delay = anim.Value.Delay;
            ae.Goto = new Chooser<string>(name);

            return ae;
        }

        internal static void Load()
        {

            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            celestenetUpdateGraphicsHook = new Hook(typeof(Ghost).GetMethod("UpdateGraphics"), typeof(Emote).GetMethod("celestenetUpdateGraphics"));
        }

        private static void Level_LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // TODO: this was changed, see if works
            // the idea is you want to see the default mode animations count, no? idk the whole thing needs a second look
            // maybe see how often updateGraphics happens and compare the whole thing, no need to break it
            //
            // idea: compare for missing sprites in the players spritemode???
            // defaultAnimationsCount = PlayerHelper.GetPlayer().Sprite.Animations.Count();
            defaultAnimationsCount = madeline_bp.Count();
            orig(self, playerIntro, isFromLoader);
        }


        public static void celestenetUpdateGraphics(Action<Ghost, DataPlayerGraphics> orig, Ghost self, DataPlayerGraphics graphics) // ty max <3
        {
            try
            {
                if (graphics.SpriteAnimations.Count() > defaultAnimationsCount) // detect if there are any foreign animations
                {

                    List<string> lackin = graphics.SpriteAnimations.Where(x => !madeline_bp.ContainsKey(x)).ToList();

                    if (lackin.Count() > 0)
                    {
                        foreach (string i in lackin)
                        {
                            if (addCustomEmote(i) && !self.Sprite.Animations.ContainsKey(i)) // try and add them
                            {
                                self.Sprite.Animations.Add(i, madeline_bp[i]); // add them to the ghost cuz celestenet wont :\
                            }
                        }
                    }
                }



            }
            catch { }

            orig(self, graphics);
        }

        internal static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;

            celestenetUpdateGraphicsHook.Dispose();
        }


        public static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);

            if (EmoteModModule.anim_by_game == 1)
            {
                if (Input.MoveX == 1)
                    self.Facing = Facings.Right;
                if (Input.MoveX == -1)
                    self.Facing = Facings.Left;
            }

            foreach (EmoteEntry e in EmoteModModule.Settings.Emotes)
            {
                if (e.CheckPressed())
                {
                    if (e.GetInfo() == null)
                        e.RefreshInfo();

                    DoEmote(e.GetInfo(), false, self);
                }
            }
        }
    }
}
