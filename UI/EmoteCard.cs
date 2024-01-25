using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System;

namespace Celeste.Mod.EmoteMod
{
    internal class EmoteCard : Entity
    {
        MTexture card = GFX.Gui["emotemod/card"];
        MTexture ticket = GFX.Gui["emotemod/ticket"];

        float animation_scale = 10f;
        public EmoteEntry emote;

        PlayerSprite sprite;
        public bool Focused;
        public bool Selected;
        public bool Opened;

        public OuiEmoteConfigMenu parent;

        // int width, height;

        Vector2 ticket_shift, card_shift;
        bool drawTicketOnTop = false;

        public IEnumerator coro;

        public override void Render()
        {
            base.Render();

            if (!Visible)
                return;

            HudRenderer.EndRender();
            HudRenderer.BeginRender(null, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);

            ticket.DrawCentered(Position + ticket_shift);
            ActiveFont.DrawOutline("Edit", Position + ticket_shift + new Vector2(card.Width / 4, ActiveFont.LineHeight / 2),
                    Vector2.One, Vector2.One, Color.White, 1f, Color.Black);

            card.DrawCentered(Position + card_shift);

            ActiveFont.Draw(emote.animation, Position + card_shift + new Vector2(card.Width / 6, -30)
                    , new Vector2(0.5f, 1), Vector2.One, Color.Black * 0.8f);
            ActiveFont.Draw(emote.animation, Position + card_shift + new Vector2(card.Width / 6, 10)
                    , new Vector2(0.5f, 1), Vector2.One * 0.8f, Color.Black * 0.6f);

            Vector2 keys_center = new(card.Width / 6, 50);
            if (emote.bind.Keys.Count == 1)
            {
                MTexture tex = GFX.Gui[$"controls/keyboard/{emote.bind.Keys[0]}"];
                tex.DrawOutlineCentered(Position + card_shift + keys_center);
            }
            else if (emote.bind.Keys.Count > 1)
            {
                List<MTexture> keytextures = new();
                float totalwidth = 0;

                foreach (Keys k in emote.bind.Keys)
                {
                    MTexture tex = GFX.Gui[$"controls/keyboard/{k}"];
                    keytextures.Add(tex);
                    totalwidth += tex.Width;
                    // tex.DrawOutlineCentered(Position + card_shift + new Vector2(card.Width / 6, 50));
                }
                float half = totalwidth / 2;
                foreach (MTexture t in keytextures)
                {
                    totalwidth -= t.Width;
                    t.DrawOutlineCentered(Position + card_shift + keys_center + new Vector2(half - totalwidth - t.Width / 2, 0));
                }
            }
            else
            {
                ActiveFont.Draw("Add keys", Position + card_shift + keys_center, Vector2.One, new Vector2(0.5f, 0.5f), Color.White);
            }
            sprite.Render();

            // EmoteModModule.echo($"lol rendering at {X}:{Y}");
            HudRenderer.EndRender();
            HudRenderer.BeginRender();

        }

        public override void Update()
        {
            base.Update();
            sprite.Position = Position + new Vector2(-card.Width / 4, card.Height / 4) + card_shift;
            sprite.Update();

            if (coro != null)
                coro.MoveNext();

            if (Focused)
            {
                if (Input.MenuCancel.Pressed)
                {
                    coro = OnClose();
                }
            }

            Visible = X > -card.Width && X < Celeste.TargetWidth + card.Width
                && Y > -card.Width && Y < Celeste.TargetHeight + card.Height;

            // EmoteModModule.echo($"lol updating at {X}:{Y}");
        }

        public void Select()
        {
            coro = OnSelect();
        }

        public void Open()
        {
            coro = OnOpen();
        }

        public void Deselect()
        {
            coro = OnDeselect();
        }

        #region animations
        // TODO remake to work from any position (add cs/ts)
        public IEnumerator OnSelect()
        {
            Vector2 cs = card_shift;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                EmoteModModule.echo($"moving {cs.X}");
                ticket_shift.X = Ease.CubeInOut(d) * card.Width / 4;
                card_shift.X = -Ease.CubeInOut(d) * card.Width / 4;
                yield return null;
            }
            Selected = true;
            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
            yield return null;

        }

        public IEnumerator OnOpen()
        {
            Vector2 ts = ticket_shift;
            Vector2 cs = card_shift;
            float l = -card.Width / 2;
            float r = card.Width / 2;
            float b = card.Height;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                float easingo = Math.Clamp(Ease.CubeOut(d), 0f, 1f);
                float easingio = Math.Clamp(Ease.CubeInOut(d), 0f, 1f);

                card_shift = new Vector2(l - (1f - easingo) * (l - cs.X), 0);
                ticket_shift = new Vector2(r - (1f - easingo) * (r - ts.X), 0);
                yield return null;

            }

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                float easing = Math.Clamp(Ease.CubeInOut(d), 0, 1);

                card_shift = new Vector2(l * (1f - easing), 0);
                ticket_shift = new Vector2(r * (1f - easing),
                        b * easing);
                yield return null;
            }
            Focused = true;
            yield return null;

        }
        public IEnumerator OnClose()
        {
            Vector2 ts = ticket_shift;
            Vector2 cs = card_shift;
            float l = -card.Width / 2;
            float r = card.Width / 2;
            float b = card.Height;

            Focused = false;
            // parent.Focused = true;
            //
            // card_shift = Vector2.Zero;
            // ticket_shift = Vector2.Zero;


            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {

                float easingo = Math.Clamp(Ease.CubeOut(d), 0f, 1f);

                card_shift = new Vector2(l - (1f - easingo) * (l - cs.X), 0);
                ticket_shift = new Vector2(r - (1f - easingo) * (r - ts.X), b * (1f - easingo));

                yield return null;

            }
            parent.Focused = true;
            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                card_shift.X = -card.Width / 4 + (-card.Width / 4) * (1f - Ease.CubeInOut(d));
                ticket_shift.X = card.Width / 4 + (card.Width / 4) * (1f - Ease.CubeInOut(d));

                yield return null;

            }

            // coro = OnSelect();

            yield return null;

        }
        IEnumerator OnDeselect()
        {
            Vector2 ts = ticket_shift;
            Vector2 cs = card_shift;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                card_shift = cs * (1f - Ease.CubeInOut(d));
                ticket_shift = ts * (1f - Ease.CubeInOut(d));

                yield return null;
            }
        }



        #endregion

        public EmoteCard(EmoteEntry emote)
        {
            Tag = Tags.HUD;

            this.emote = emote;

            this.ticket_shift = Vector2.Zero;
            this.card_shift = Vector2.Zero;
            this.Focused = false;

            sprite = new(PlayerSpriteMode.Madeline);
            sprite.Scale = Vector2.One * animation_scale;

            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
        }


    }
}
