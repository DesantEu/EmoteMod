using System;
using System.Collections.Generic;
using System.Linq;
using Celeste;
using Celeste.Mod.CelesteNet.Client.Entities;
using Celeste.Mod.CelesteNet.DataTypes;
using EmoteMod.Features;
using EmoteMod.Module;
using EmoteMod.Utility;
using Monocle;
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
            customEmotes = new();

            On.Celeste.LevelExit.ctor += LevelExit;
            On.Celeste.Level.Update += Level_Update;
            On.Celeste.LevelExit.Begin += LevelExit_Begin;
            On.Celeste.Level.LoadLevel += LoadLevel; celestenetUpdateGraphicsHook = new Hook(typeof(Ghost).GetMethod("UpdateGraphics"), typeof(EmoteHooks).GetMethod("celestenetUpdateGraphics"));
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

            cancelEmote();

            On.Celeste.LevelExit.ctor -= LevelExit;
            On.Celeste.Level.Update -= Level_Update;
            On.Celeste.LevelExit.Begin -= LevelExit_Begin;
            On.Celeste.Level.LoadLevel -= LoadLevel;
            celestenetUpdateGraphicsHook.Dispose();
        }


        // cancel on level exit
        public static void LevelExit_Begin(On.Celeste.LevelExit.orig_Begin orig, LevelExit self)
        {
            if (EmoteModModule.anim_by_game == 1)
                cancelEmote();
            orig(self);
        }

        // cancel if not on level
        public static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);

            if (!(Engine.Scene is Level) && EmoteModModule.anim_by_game == 1)
                cancelEmote();
        }

        internal static void Player_Update(On.Celeste.Player.orig_Update orig, Player player)
        {
            if (EmoteModModule.anim_by_game == 1)
            {
                if (player.Sprite.CurrentAnimationID == "idle")
                    cancelEmote();
                // something
                if (player.StateMachine.State == 0)
                    cancelEmote();
                // cancel emote on press keys or if we die so that we dont respawn in a bad spot
                if (Input.Dash.Pressed || Input.Jump.Pressed || Input.MoveY == 1 || Input.Grab.Pressed || player.Dead)
                    cancelEmote();
            }
            // cancel emote if below level
            if (Engine.Scene is Level level && player.Y > level.Bounds.Bottom && EmoteModModule.anim_by_game == 1)
                cancelEmote();
            // cancel emote if not on level
            if (!(Engine.Scene is Level))
                cancelEmote();
            // check if cutscene started
            if (EmoteModModule.anim_by_game == 0)
                if (player.StateMachine.State == Player.StDummy || player.StateMachine.State == Player.StLaunch || player.StateMachine.State == Player.StFlingBird || player.StateMachine.State == Player.StSummitLaunch)
                    EmoteModModule.anim_by_game = 2;
            // check if cutscene over
            if (EmoteModModule.anim_by_game == 2)
                if (player.StateMachine.State != Player.StDummy && player.StateMachine.State != Player.StLaunch && player.StateMachine.State != Player.StFlingBird && player.StateMachine.State != Player.StSummitLaunch)
                    EmoteModModule.anim_by_game = 0;


            orig(player);

            // cancel
            if (EmoteModModule.anim_by_game == 1)
            {
                if (Input.MoveX == 1)
                    player.Facing = Facings.Right;
                if (Input.MoveX == -1)
                    player.Facing = Facings.Left;
            }

            foreach (EmoteEntry e in EmoteModModule.Settings.Emotes)
            {
                if (e.CheckPressed())
                {
                    if (e.GetInfo() == null)
                        e.RefreshInfo();

                    DoEmote(e.GetInfo(), false, player);
                }
            }

        }


        internal static void LevelExit(On.Celeste.LevelExit.orig_ctor orig, LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            if (EmoteModModule.anim_by_game == 1)
                cancelEmote();
            orig(self, mode, session, snow);
        }

        // cancel when changing rooms
        internal static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (PlayerHelper.GetPlayer() != null && EmoteModModule.anim_by_game == 1)
                cancelEmote();
            orig(self, playerIntro, isFromLoader);
        }
    }
}
