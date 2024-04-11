using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.UI;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Celeste.Mod.EmoteMod
{
    public class EmoteInfo
    {
        public string animation;
        public string spritebank;
        public bool isCustom;
        public bool changeSpriteMode;

        public PlayerSpriteMode spritemode;

    }
    public static class AnimationHelper
    {
        static Dictionary<string, PlayerSpriteMode> global_emotes;
        // TODO: remove probably
        public static void Init()
        {
            global_emotes = new() {
            {"player_no_backpack", PlayerSpriteMode.MadelineNoBackpack},
            {"badeline", PlayerSpriteMode.Badeline},
            {"player", PlayerSpriteMode.Madeline},
            {"player_playback", PlayerSpriteMode.Playback},
            // {"player_badeline", PlayerSpriteMode.MadelineAsBadeline},
        };

        }
        // public static string GetSpritebank(string animation)
        // {
        //     return "";
        // }
        public static EmoteInfo GetInfo(string anim)
        {
            EmoteInfo info = new();

            // TODO: finish

            if (anim.Contains(":"))
            {
                string[] parts = anim.Split(":", 2);
                info.isCustom = true;

                // new spritemode-specific ones
                if (global_emotes.Keys.Contains(parts[0]))
                {
                    if (GFX.SpriteBank.SpriteData[parts[0]].Sprite.Animations.Keys.Contains(parts[1]))
                    {
                        info.spritebank = parts[0];
                        info.spritemode = global_emotes[parts[0]];
                        info.animation = parts[1];
                        return info;
                    }

                }
                // customs
                else if (GFX.SpriteBank.SpriteData.Keys.Contains(parts[0]))
                {
                    if (GFX.SpriteBank.SpriteData[parts[0]].Sprite.Animations.Keys.Contains(parts[1]))
                    {

                        info.spritebank = parts[0];
                        info.spritebank = parts[0];
                        info.spritemode = global_emotes["player"];
                        info.animation = parts[1];
                        return info;
                    }
                }


            }
            else
            {
                foreach (string sb in global_emotes.Keys)
                {
                    if (!GFX.SpriteBank.SpriteData.ContainsKey(sb))
                    {
                        return null;
                    }

                    if (GFX.SpriteBank.SpriteData[sb].Sprite.Animations.ContainsKey(anim))
                    {
                        info.changeSpriteMode = false;
                        info.animation = anim;
                        info.spritebank = sb;
                        info.isCustom = false;
                        info.spritemode = global_emotes[sb];
                        return info;

                    }


                }

            }
            return null;
        }
    }
}
