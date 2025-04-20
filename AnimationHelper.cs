using System.Collections.Generic;
using System;
using System.Linq;
namespace Celeste.Mod.EmoteMod
{
    /// <summary>
    /// <paramref name="animation"/> asdfaf
    /// </summary>
    public class EmoteInfo
    {

        /// <summary>
        /// ONLY the animation name (no spritebank).
        /// </summary>
        public string animation;

        /// <summary>
        /// ONLY the spritebank name (no animation).
        /// </summary>
        public string spritebank;

        /// <summary>
        /// Identifies a 'custom' emote. Meaning the sprite data is ripped from some other sprite and pasted onto player.
        /// </summary>
        public bool isCustom;

        /// <summary>
        /// Signifies that the emote has a "sb:anim".
        /// </summary>
        public bool changeSpriteMode;

        /// <summary>
        /// Desired spritemode.
        /// </summary>
        public PlayerSpriteMode spritemode;

    }
    public static class AnimationHelper
    {
        public static Dictionary<string, PlayerSpriteMode> GlobalEmotes;
        // TODO: remove probably
        public static void Init()
        {
            GlobalEmotes = new() {
                {"player_no_backpack", PlayerSpriteMode.MadelineNoBackpack},
                {"badeline", PlayerSpriteMode.Badeline},
                {"player", PlayerSpriteMode.Madeline},
                {"player_playback", PlayerSpriteMode.Playback},
                {"player_badeline", PlayerSpriteMode.MadelineAsBadeline},
            };
        }

        public static EmoteInfo GetInfo(string anim)
        {
            EmoteInfo info = new();

            // TODO: finish

            if (anim.Contains(':'))
            {
                string[] parts = anim.Split(":", 2);

                // new spritemode-specific ones
                if (GlobalEmotes.Keys.Contains(parts[0]))
                {
                    if (GFX.SpriteBank.SpriteData[parts[0]].Sprite.Animations.Keys.Contains(parts[1]))
                    {
                        info.spritebank = parts[0];
                        info.spritemode = GlobalEmotes[parts[0]];
                        info.animation = parts[1];
                        info.isCustom = false;
                        info.changeSpriteMode = true;
                        return info;
                    }
                }
                // customs
                else if (GFX.SpriteBank.SpriteData.Keys.Contains(parts[0]))
                {
                    if (GFX.SpriteBank.SpriteData[parts[0]].Sprite.Animations.Keys.Contains(parts[1]))
                    {

                        info.spritebank = parts[0];
                        info.spritemode = GlobalEmotes["player"];
                        info.animation = parts[1];
                        info.isCustom = true;
                        info.changeSpriteMode = true;
                        return info;
                    }
                }
            }
            else
            {
                foreach (string sb in GlobalEmotes.Keys)
                {
                    if (!GFX.SpriteBank.SpriteData.ContainsKey(sb))
                    {
                        // return null;
                        continue;
                    }

                    if (GFX.SpriteBank.SpriteData[sb].Sprite.Animations.ContainsKey(anim))
                    {
                        info.animation = anim;
                        info.spritebank = sb;
                        info.isCustom = false;
                        info.spritemode = GlobalEmotes[sb];
                        return info;

                    }
                }
            }
            return null;
        }
    }
}
