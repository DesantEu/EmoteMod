using Monocle;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Celeste.Mod.UI;
using EmoteMod.Module;
using EmoteMod.UI;
using Celeste;
using EmoteMod.Utility;

using static EmoteMod.Features.Backpack;

namespace EmoteMod.Hooks
{
    public static class BackpackHooks
    {
        public static void Load()
        {
            On.Celeste.PlayerSprite.ctor += PlayerSprite;
            On.Celeste.LevelLoader.ctor += onLevelLoader;

            // TODO: figure out what this is for
            // if (Engine.Scene is Level)
            // {
            //     initializeRollBackpackSprites();
            // }
        }


        public static void Unload()
        {
            On.Celeste.PlayerSprite.ctor -= PlayerSprite;
            On.Celeste.LevelLoader.ctor -= onLevelLoader;
        }
        internal static void PlayerSprite(On.Celeste.PlayerSprite.orig_ctor orig, PlayerSprite self, PlayerSpriteMode mode)
        {
            // code stolen from max (extended variant mode)
            if (EmoteModModule.anim_by_game != 1 &&
                    !(OuiModOptions.Instance?.Overworld.GetUI<OuiEmoteConfigMenu>().Visible ?? false))
                if (mode == PlayerSpriteMode.Madeline || mode == PlayerSpriteMode.MadelineNoBackpack)
                {
                    mode = GetMode(EmoteModModule.Settings.Backpack, mode);
                }

            orig(self, mode);
        }

        // load missing animatons
        private static void onLevelLoader(On.Celeste.LevelLoader.orig_ctor orig, LevelLoader self, Session session, Vector2? startPosition)
        {
            orig(self, session, startPosition);

            // TODO: figure out what this is for (this is also in load)
            // initializeRollBackpackSprites();
        }

    }
}
