using Monocle;
using System;
using System.Collections.Generic;
using Celeste;

namespace Celeste.Mod.EmoteMod
{
    public class BackpackModule
    {
        public static Player player;
        public enum MadelineBackpackModes { Default, NoBackpack, Backpack, Playback };

        internal static void PlayerSprite(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode)
        {
            // code stolen from max (extended variant mode)
            if (EmoteModMain.Settings.Backpack != 0)
                if (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineNoBackpack)
                {
                    if (EmoteModMain.Settings.Backpack == (int)PlayerSpriteMode.Madeline)
                    {
                        mode = PlayerSpriteMode.Madeline;
                    }
                    else if (EmoteModMain.Settings.Backpack == (int) MadelineBackpackModes.NoBackpack)
                    {
                        mode = PlayerSpriteMode.MadelineNoBackpack;
                    }
                    else if (EmoteModMain.Settings.Backpack == (int)PlayerSpriteMode.Playback)
                    {
                        mode = PlayerSpriteMode.Playback;
                    }
                }

            orig(self, mode);
        }

        private static void initializeRollBackpackSprites()
        {
            Dictionary<string, Sprite.Animation> player = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
            Dictionary<string, Sprite.Animation> playerNoBackpack = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;

            // copy the roll animations from player_no_backpack to player, to prevent crashes in Farewell if the backpack is forced.
            if (!player.ContainsKey("roll"))
            {
                player.Add("roll", playerNoBackpack["roll"]);
            }
            if (!player.ContainsKey("rollGetUp"))
            {
                player.Add("rollGetUp", playerNoBackpack["rollGetUp"]);
            }
            if (!player.ContainsKey("downed"))
            {
                player.Add("downed", playerNoBackpack["downed"]);
            }
        }

        public static void Load()
        {
            if (Engine.Scene is Level)
            {
                initializeRollBackpackSprites();
            }
        }


        // old

        //internal static void Player_ResetSprite(On.Celeste.Player.orig_Update orig, Player self)
        ////internal static void Player_ResetSprite(On.Celeste.Player.orig_ResetSprite orig, Player self, PlayerSpriteMode mode)
        //{
        //    orig(self);
        //    // so that it doent mess with emotes
        //    if (Engine.Scene is Level && EmoteModMain.anim_by_game == 0)

        //        // force backpack
        //        if (EmoteModMain.Settings.Backpack == 1 && self.Sprite.Mode != PlayerSpriteMode.Madeline)
        //            self.ResetSprite(PlayerSpriteMode.Madeline);
        //        // force no backpack
        //        else if (EmoteModMain.Settings.Backpack == 2 && self.Sprite.Mode != PlayerSpriteMode.MadelineNoBackpack)
        //            self.ResetSprite(PlayerSpriteMode.MadelineNoBackpack);
        //        // playback sprite mode
        //        else if (EmoteModMain.Settings.Backpack == 3 && self.Sprite.Mode != PlayerSpriteMode.Playback)
        //            self.ResetSprite(PlayerSpriteMode.Playback);
        //        // else apply either default skin
        //        else if (EmoteModMain.Settings.Backpack == 0 && !SaveData.Instance.Assists.PlayAsBadeline && self.Sprite.Mode != self.DefaultSpriteMode) // default
        //            self.ResetSprite(self.DefaultSpriteMode);
        //        // or badeline
        //        else if (EmoteModMain.Settings.Backpack == 0 && SaveData.Instance.Assists.PlayAsBadeline && self.Sprite.Mode != PlayerSpriteMode.MadelineAsBadeline) // default
        //            self.ResetSprite(PlayerSpriteMode.MadelineAsBadeline);
        //        else

        //            player = self;

        //}


    }
}