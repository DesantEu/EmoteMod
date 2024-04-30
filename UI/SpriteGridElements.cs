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
    public abstract class SpriteGridElement : Entity
    {
        internal IEnumerator coro;
        internal float fade_in_delay;
        internal float alpha;
        internal Vector2 Pos;
        internal SpriteGrid parent;
        internal float scroll_shift => parent.scroll_offset;

        private float visible_margin = 500;

        public IEnumerator FadeIn()
        {
            while (fade_in_delay > 0)
            {
                fade_in_delay -= Engine.DeltaTime;
                yield return null;
            }
            for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
            {
                alpha = d;
                yield return null;
            }
            alpha = 1;

            yield return null;
        }

        internal void RecalculatePosition()
        {
            Pos = Position - new Vector2(0, parent.scroll_offset);
        }

        public override void Update()
        {
            base.Update();

            if (coro != null)
                coro.MoveNext();

            RecalculatePosition();

            if (Pos.X < -visible_margin
                    || Pos.X > Celeste.TargetWidth
                    || Pos.Y < -visible_margin
                    || Pos.Y > Celeste.TargetHeight)
            {
                Visible = false;
            }
            else
            {
                Visible = true;
            }
        }

        private IEnumerator fadeOut(string direction)
        {
            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
            {
                alpha = d;
                yield return null;
            }
            alpha = 0;
        }

        public void FadeOut(string direction = "none")
        {
            coro = fadeOut(direction);
        }
    }

    public class SpriteGridTitle : SpriteGridElement
    {
        string text;
        public static Vector2 text_scale = Vector2.One;
        public float text_height;
        public static float line_offset = 15;
        public float total_height => text_height + line_offset + 5 + line_offset;
        // string bottom_text = "";

        public override void Render()
        {
            base.Render();

            ActiveFont.DrawOutline(text, Pos, Vector2.Zero, text_scale, Color.Snow * alpha, 3, Color.Black * alpha);
            Draw.Line(new Vector2(Pos.X, Pos.Y + text_height + line_offset)
                    , new Vector2(Celeste.TargetWidth - Pos.X, Pos.Y + text_height + line_offset)
                    , Color.Snow * alpha, 5);
            // Draw.Line(SpriteGridCell.)
        }

        void Init(Vector2 pos, string text, SpriteGrid parent, float fadeInDelay)
        {
            this.Position = pos;
            this.parent = parent;
            RecalculatePosition();
            this.text = text;
            this.fade_in_delay = fadeInDelay;
            parent.Scene.Add(this);
            this.text_height = (ActiveFont.Measure(text) * text_scale).Y;
            Tag = Tags.HUD;
            coro = FadeIn();
        }

        public SpriteGridTitle(string text, Vector2 pos, SpriteGrid parent, float fadeInDelay = 0)
        {
            Init(pos, text, parent, fadeInDelay);
        }
        // public SpriteGridTitle(Vector2 pos, string text, string subtext, SpriteGrid parent)
        // {
        //     Init(pos, text, parent);
        //
        // }
    }

    public class SpriteGridCell : SpriteGridElement
    {
        PlayerSprite sprite;
        public string text;
        Wiggler wiggler;
        bool isSelected;
        float downscale;
        Color flashing_color => !Settings.Instance.DisableFlashes && !this.Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;


        public static float default_size = 32;
        public static float upscale = 6;

        public void Select()
        {
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public override void Update()
        {
            base.Update();

            // if (coro != null)
            //     coro.MoveNext();
            //
            // sprite.Color = Color.White * alpha;
            //
            // sprite.Position = Pos;
            sprite.Update();
        }

        public override void Render()
        {
            base.Render();
            HudRenderer.EndRender();
            HudRenderer.BeginRender(null, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);

            sprite.Position = Pos;
            sprite.Color = Color.White * alpha;

            sprite.Render();
            float rect_side = default_size * upscale;
            // Draw.HollowRect(Pos, rect_side, rect_side, Color.Snow * alpha);

            ActiveFont.DrawOutline(text, Pos + new Vector2(rect_side / 2, rect_side),
                    new Vector2(0.5f, 0), Vector2.One * 0.5f,
                    isSelected ? flashing_color : Color.Snow * alpha, 3, Color.Black * alpha);

            HudRenderer.EndRender();
            HudRenderer.BeginRender();


        }

        public SpriteGridCell(EmoteInfo emote, string text, Vector2 pos, SpriteGrid parent, float fadeInDelay = 0f)
        {
            Tag = Tags.HUD;
            this.Position = pos;
            this.parent = parent;
            RecalculatePosition();
            this.text = text;
            fade_in_delay = fadeInDelay;
            alpha = 0;
            coro = FadeIn();

            this.sprite = new(emote.spritemode);

            string anim_name = AnimationHelper.global_emotes.ContainsKey(emote.spritebank)
                ? emote.animation
                : emote.isCustom ? $"{emote.spritebank}:{emote.animation}" : emote.animation;
            MTexture first_frame = sprite.Animations[anim_name].Frames[0];

            float max_side = Math.Max(first_frame.Width, first_frame.Height);
            downscale = default_size / max_side;
            sprite.Scale = Vector2.One * downscale * upscale;
            sprite.JustifyOrigin(0, 0);

            sprite.Play(anim_name);
            parent.Scene.Add(this);
        }
    }



}

