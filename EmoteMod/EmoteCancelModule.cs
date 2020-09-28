using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteCancelModule
    {
        public static Player player;

        public static void cancelEmote()
        {
            player.DummyAutoAnimate = true; // auto animate
            player.StateMachine.State = Player.StNormal; // idk maybe its supposed to make player moveable or something i dont remember
            EmoteModMain.celestenetSettings.Interactions = EmoteModMain.interactDefault; // return interactions do their default value
            SaveData.Instance.Assists.Invincible = EmoteModMain.invincibilityDefault;

            // if changed sprite get it back
            if (EmoteModMain.changedSprite)
            {
                player.ResetSprite(player.DefaultSpriteMode);
                EmoteModMain.changedSprite = false; // idk why its here twice but im afraid to remove it
                EmoteModMain.changedSprite = false;
            }

            EmoteModMain.anim_by_game = 0; // tell yourself that no animation is playing
        }
        // cancel on level exit
        public static void LevelExit_Begin(On.Celeste.LevelExit.orig_Begin orig, LevelExit self)
        {
            if (EmoteModMain.anim_by_game == 1)
                cancelEmote();
            orig(self);
        }
        // cancel if not on level
        public static void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);

            if (!(Engine.Scene is Level) && EmoteModMain.anim_by_game == 1)
                cancelEmote();
        }
        internal static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            player = self;

            // if idle after emote cancel emote
            if (EmoteModMain.anim_by_game == 1 && player.Sprite.CurrentAnimationID == "idle")
                cancelEmote();
            // something
            if (EmoteModMain.anim_by_game == 1 && player.StateMachine.State == 0)
                cancelEmote();
            // cancel emote on press keys or if we die so that we dont respawn in a bad spot
            if (Input.Dash.Pressed || Input.Jump.Pressed || Input.MoveY == 1 || Input.Grab.Pressed || player.Dead)
                if (EmoteModMain.anim_by_game == 1)
                    cancelEmote();
            // cancel emote if below level
            if (Engine.Scene is Level level && player.Y > level.Bounds.Bottom && EmoteModMain.anim_by_game == 1)
                cancelEmote();
            // cancel emote if not on level
            if (!(Engine.Scene is Level))
                cancelEmote();
            // check if cutscene started
            if (EmoteModMain.anim_by_game == 0)
                if (player.StateMachine.State == Player.StDummy || player.StateMachine.State == Player.StLaunch || player.StateMachine.State == Player.StFlingBird || player.StateMachine.State == Player.StSummitLaunch)
                    EmoteModMain.anim_by_game = 2;
            // check if cutscene over
            if (EmoteModMain.anim_by_game == 2)
                if (player.StateMachine.State != Player.StDummy && player.StateMachine.State != Player.StLaunch && player.StateMachine.State != Player.StFlingBird && player.StateMachine.State != Player.StSummitLaunch)
                    EmoteModMain.anim_by_game = 0;


            orig(self);
        }

        internal static void LevelExit(On.Celeste.LevelExit.orig_ctor orig, LevelExit self, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            if (EmoteModMain.anim_by_game == 1)
                cancelEmote();
            orig(self, mode, session, snow);
        }

        // cancel when changing rooms
        internal static void LoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            if (player != null && EmoteModMain.anim_by_game == 1)
                cancelEmote();
            orig(self, playerIntro, isFromLoader);
        }
    }
}
