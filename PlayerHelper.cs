using Monocle;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Celeste.Mod.EmoteMod
{
    internal class PlayerHelper
    {

        internal bool ForceInvincibility = false;

        public void Load()
        {
            // TODO: hook death or something
        }

        public static Player GetPlayer()
        {
            return Engine.Scene?.Tracker?.GetEntity<Player>();
        }

        public static Sprite.Animation getAnimationByName(string animation)
        {
            // b
            if (animation == "b" || animation == "bounce")
            {
                return getAnimationByName("spin");
            }

            // this sucks but idk
            if (Emote.madeline_no_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
            {
                return Emote.madeline_no_bp[animation];
            }
            else if (Emote.madeline_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
            {
                return Emote.madeline_bp[animation];
            }
            else if (Emote.badeline.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
            {
                return Emote.badeline[animation];
            }

            else if (findCustomEmote(animation) != null)
            {
                return findCustomEmote(animation);
            }

            else
            {
                EmoteModModule.echo($"EMOTEMOD ERROR: Could not find '{animation}'");

                return getAnimationByName("player:idle");
            }
        }

        private static Sprite.Animation findCustomEmote(string name)
        {
            char split = ':';

            if (!name.Contains(split))
                return null;

            string sdata_name = name.Split(split)[0];
            string anim_name = name.Split(split, 2)[1];

            if (!GFX.SpriteBank.SpriteData.ContainsKey(sdata_name))
                return null;


            Dictionary<string, Sprite.Animation> anims = GFX.SpriteBank.SpriteData[sdata_name].Sprite.Animations;

            if (!anims.ContainsKey(anim_name))
                return null;


            return anims[anim_name];
        }

        public void Unload()
        {
        }
    }
}
