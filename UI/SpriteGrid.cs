using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System;
using System.Reflection;
using System.Linq;

namespace Celeste.Mod.EmoteMod
{
    public enum EmotePickerStage
    {
        SpriteBank,
        Emote,
        Done

    }
    public class SpriteGridCell : Entity
    {
        PlayerSprite sprite;
        string text;
        Vector2 Pos;
        Wiggler wiggler;
        bool isSelected;

        public override void Update()
        {
            base.Update();

            sprite.Update();
        }

        public override void Render()
        {
            base.Render();

        }

        public SpriteGridCell(EmoteInfo emote, string text, Vector2 pos)
        {
            this.Pos = pos;
            this.text = text;

            this.sprite = new(emote.spritemode);
            sprite.Play(emote.isCustom ? $"{emote.spritebank}:{emote.animation}" : emote.animation);
        }
    }
    public class SpriteGrid : Entity
    {
        IEnumerator coro;
        float bg_alpha = 0f;
        EmotePickerStage stage;
        EmoteCard parent;

        bool Focused;

        // MTexture bg = Celeste.


        public override void Update()
        {
            base.Update();
            // EmoteModModule.echo("gallery update");

            if (coro != null)
                coro.MoveNext();

            if (Focused)
            {
                if (Input.MenuCancel.Pressed)
                {
                    coro = SpriteBankExit();
                }
            }
        }
        public override void Render()
        {
            base.Render();
            Draw.Rect(-10, -10, Celeste.TargetWidth + 20, Celeste.TargetHeight + 20, Color.Black * bg_alpha);

        }

        private IEnumerator SpriteBankEnter()
        {
            Visible = true;
            parent.stopVisibilityChecks = true;
            // metallica - fade to black
            for (float d = 0; d < 1; d += Engine.DeltaTime * 12)
            {
                bg_alpha = 0xff * d;
                // EmoteModModule.echo($"alpha is {bg_alpha}");
                yield return null;

            }
            parent.Visible = false;
            bg_alpha = 0xff;


            Focused = true;

            yield return null;

        }

        private IEnumerator SpriteBankExit()
        {
            Focused = false;
            // metallica - fade to black
            for (float d = 1; d > 0; d -= Engine.DeltaTime * 12)
            {
                bg_alpha = 0xff * d;
                yield return null;

            }
            bg_alpha = 0;

            yield return null;
            parent.Focused = true;
            parent.atButton = Butt.Spritebank;
            parent.stopVisibilityChecks = false;
            parent.Visible = true;
            RemoveSelf();

        }

        void Init()
        {
            Tag = Tags.HUD;
        }


        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
        /// <paramref name="emote"/> - the emote currently targeted
		/// this is for emote only
		/// </summary>
        public SpriteGrid(string spritebank, string emote, EmoteCard parent)
        {
        }
        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
		/// this is for spritebank + emote
		/// </summary>
        public SpriteGrid(string spritebank, EmoteCard parent)
        {
            Init();
            stage = EmotePickerStage.SpriteBank;
            coro = SpriteBankEnter();
            Visible = true;
            this.parent = parent;
            parent.parent.Scene.Add(this);

            // parent.Scene.Add(this);
        }
    }
}
