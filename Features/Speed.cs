using Celeste;
using EmoteMod.Module;
using EmoteMod.Utility;
using Monocle;
using MonoMod.Utils;
using System;
using System.Collections.Generic;

namespace EmoteMod.Features
{
    public class Speed
    {
        private static Player player;

        // speed multipliers
        public static float[] speeds = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 2, 4, 5, 6, 10, 20, 30, 40, 50, 60, 70, 100 };
        internal static string changedAnimaitonID = null;
        internal static float delayBeforeChange;
        internal static string spritebank;

        //speed formatter
        public static Func<int, string> speedFormatter = arg =>
        {
            return $"{speeds[arg]}x";
        };

        internal static void SetSpeed()
        {
            if (PlayerHelper.GetPlayer() == null)
            {
                return;
            }
            Sprite sp = PlayerHelper.GetPlayer().Sprite;
            int sm = ((int)PlayerHelper.GetPlayer().Sprite.Mode);
            DynData<Sprite> data = new DynData<Sprite>(sp);
            spritebank = new List<string>() { "player", "player_no_backpack", "badeline", "player_badeline", "player_playback" }[sm];
            EmoteModModule.echo($"spritebank: {spritebank}");

            ResetSpeed();

            delayBeforeChange = sp.Animations[sp.CurrentAnimationID].Delay;
            changedAnimaitonID = sp.CurrentAnimationID;

            float delay = delayBeforeChange / speeds[EmoteModModule.Settings.AnimationSpeed];

            (data["currentAnimation"] as Sprite.Animation).Delay = delay;
            EmoteModModule.echo($"changed sprite speed on {sp.CurrentAnimationID}: {delayBeforeChange} -> {delay}");

            if (Emote.madeline_bp.ContainsKey(changedAnimaitonID))
            {
                EmoteModModule.echo($"delay in sprite data: {Emote.madeline_bp[changedAnimaitonID].Delay}");
            }
        }

        internal static void ResetSpeed()
        {
            if (changedAnimaitonID != null)
            {
                if (!GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations.ContainsKey(changedAnimaitonID))
                    return;

                Sprite sp = PlayerHelper.GetPlayer().Sprite;

                EmoteModModule.echo($"resetting delay on {sp.CurrentAnimationID}: {sp.Animations[changedAnimaitonID].Delay} -> {delayBeforeChange}");

                GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations[changedAnimaitonID].Delay = delayBeforeChange;

                changedAnimaitonID = null;
            }
        }
    }
}
