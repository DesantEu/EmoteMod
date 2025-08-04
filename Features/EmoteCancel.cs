using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteCancel
    {
        public static bool invincibilityDefault;
        public static List<string> customEmotes;

        public static void cancelEmote()
        {
            Player player = PlayerHelper.GetPlayer();

            if (player == null)
                return;

            Speed.ResetSpeed();

            player.DummyAutoAnimate = true; // auto animate
            player.StateMachine.State = Player.StNormal; // idk maybe its supposed to make player moveable or something i dont remember
            player.Speed = Vector2.Zero;

            CNetHelper.InteractionsAllowed = true;
            SaveData.Instance.Assists.Invincible = invincibilityDefault;

            // return player sprite mode
            if (Emote.playback)
            { //ex variants fix thing
                player.ResetSpriteNextFrame(PlayerSpriteMode.Playback);
                Emote.playback = false;
            }
            else if (SaveData.Instance.Assists.PlayAsBadeline)
            {
                player.ResetSprite(PlayerSpriteMode.MadelineAsBadeline);
            }
            else
            {
                player.ResetSprite(player.DefaultSpriteMode);
            }

            foreach (string e in customEmotes)
            {
                try
                {
                    Emote.madeline_bp.Remove(e);
                }
                catch { }
            }

            customEmotes.Clear();

            Emote.bounced = false;

            EmoteModModule.anim_by_game = 0; // tell yourself that no animation is playing
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
        }

        internal static void Load()
        {
            // customEmote = "";
            customEmotes = new();

            On.Celeste.LevelExit.ctor += LevelExit;
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Level.Update += Level_Update;
            On.Celeste.LevelExit.Begin += LevelExit_Begin;
            On.Celeste.Level.LoadLevel += LoadLevel;
        }

        internal static void Unload()
        {
            cancelEmote();

            On.Celeste.LevelExit.ctor -= LevelExit;
            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Level.Update -= Level_Update;
            On.Celeste.LevelExit.Begin -= LevelExit_Begin;
            On.Celeste.Level.LoadLevel -= LoadLevel;
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
