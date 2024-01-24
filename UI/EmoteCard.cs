using Monocle;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
using Celeste.Mod.Core;
using Celeste.Mod.Helpers;
using Mono.Cecil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    internal class EmoteCard : Entity
    {
        MTexture card = GFX.Gui["emotemod/card"];
        MTexture ticket = GFX.Gui["emotemod/ticket"];

        int index;
        float animation_scale = 10f;
        public EmoteEntry emote;

        PlayerSprite sprite;

        public override void Render()
        {
            base.Render();

            card.DrawCentered(Position);
            sprite.Position = Position + new Vector2(-50f, 0f);
            sprite.Render();

            EmoteModModule.echo($"lol rendering at {X}:{Y}");
        }

        public override void Update()
        {
            base.Update();
            sprite.Update();

            EmoteModModule.echo($"lol updating at {X}:{Y}");
        }

        public EmoteCard(EmoteEntry emote)
        {
            Tag = Tags.HUD;

            this.emote = emote;

            sprite = new(PlayerSpriteMode.Madeline);
            sprite.Scale = Vector2.One * animation_scale;

            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
        }


    }
}
