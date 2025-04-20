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
        internal Vector2 render_pos;
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
            render_pos = Position - new Vector2(0, parent.scroll_offset);
        }

        public override void Update()
        {
            base.Update();

            if (coro != null)
                coro.MoveNext();

            RecalculatePosition();

            if (render_pos.X < -visible_margin
                    || render_pos.X > Celeste.TargetWidth
                    || render_pos.Y < -visible_margin
                    || render_pos.Y > Celeste.TargetHeight)
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
        public string text;
        public static Vector2 text_scale = Vector2.One;
        /// <summary>
        /// Text height. Use `total_height` for height with padding
        /// </summary>
        public float text_height;
        public static float line_offset = 15;
        /// <summary>
        /// Height with padding
        /// </summary>
        public float total_height => text_height + line_offset + 5 + line_offset;
        // string bottom_text = "";

        public override void Render()
        {
            base.Render();

            // text
            ActiveFont.DrawOutline(text, render_pos, Vector2.Zero, text_scale, Color.Snow * alpha, 3, Color.Black * alpha);

            // line
            Vector2 line_start = new Vector2(render_pos.X, render_pos.Y + text_height + line_offset);
            Vector2 line_end = new Vector2(Celeste.TargetWidth - render_pos.X, render_pos.Y + text_height + line_offset);
            Draw.Line(line_start, line_end, Color.Snow * alpha, 5);
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
            Tag = parent.Tag;
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
        Wiggler wiggler = Wiggler.Create(0.4f, 3f);
        bool isSelected;
        float downscale;
        public EmoteInfo info;
        public float total_height = 0;
        private String anim_name;
        Color flashing_color => !Settings.Instance.DisableFlashes && !this.Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;


        public static float default_size = 32;
        public static float upscale = 6;
        /// <summary>
        /// Upscaled sprite size
        /// </summary>
        public static float sprite_size = default_size * upscale;

        public void Select()
        {
            wiggler.Start();
            sprite.Play(anim_name);
            isSelected = true;
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public override void Update()
        {
            base.Update();
            wiggler.Update();

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

            sprite.Position = render_pos - new Vector2(0, SpriteGrid.spacing * 0.25f * wiggler.Value);
            sprite.Color = Color.White * alpha;

            sprite.Render();

            ActiveFont.DrawOutline(
                text
                , render_pos + new Vector2(sprite_size / 2, sprite_size) // at bottom center
                , new Vector2(0.5f, 0) // justify top center
                , Vector2.One * 0.5f // scale // TODO: calculate instead
                , isSelected ? flashing_color : Color.Snow * alpha, 3, Color.Black * alpha); // color

            HudRenderer.EndRender();
            HudRenderer.BeginRender();


        }

        public SpriteGridCell(EmoteInfo emote, string text, Vector2 pos, SpriteGrid parent, float fadeInDelay = 0f)
        {
            Tag = parent.Tag;
            this.Position = pos;
            this.parent = parent;
            RecalculatePosition();
            this.text = text;
            this.info = emote;
            fade_in_delay = fadeInDelay;
            alpha = 0;
            coro = FadeIn();
            this.total_height = sprite_size + ActiveFont.LineHeight * 0.5f;

            this.sprite = new(emote.spritemode);

            anim_name = AnimationHelper.GlobalEmotes.ContainsKey(emote.spritebank)
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

