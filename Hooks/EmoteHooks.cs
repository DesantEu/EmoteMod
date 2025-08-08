using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.CelesteNet.Client.Entities;
using Celeste.Mod.CelesteNet.DataTypes;
using EmoteMod.Features;
using EmoteMod.Module;
using MonoMod.RuntimeDetour;

using static EmoteMod.Features.Emote;

namespace EmoteMod.Hooks
{

    public static class EmoteHooks
    {
        private static Hook celestenetUpdateGraphicsHook;
        private static int defaultAnimationsCount;

        internal static void Load()
        {

            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Level.LoadLevel += Level_LoadLevel;
            celestenetUpdateGraphicsHook = new Hook(typeof(Ghost).GetMethod("UpdateGraphics"), typeof(EmoteHooks).GetMethod("celestenetUpdateGraphics"));
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
