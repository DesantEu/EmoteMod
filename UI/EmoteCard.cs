using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System;

namespace Celeste.Mod.EmoteMod
{
    public enum Butt
    {
        None, Animation, Spritebank, Keys, Save, Cancel, Delete
    }
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
        Butt atButton = Butt.None;
        Wiggler wiggler = Wiggler.Create(0.25f, 3f);

        float edit_scale, anim_name_scale, spritebank_scale, save_scale, cancel_scale, delete_scale;

        public OuiEmoteConfigMenu parent;
        Color flashing_color => !Settings.Instance.DisableFlashes && !this.Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;

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


            if (!drawTicketOnTop)
                renderTicket();
            renderCard();
            if (drawTicketOnTop)
                renderTicket();


            // EmoteModModule.echo($"lol rendering at {X}:{Y}");
            HudRenderer.EndRender();
            HudRenderer.BeginRender();

        }

        void renderTicket()
        {
            ticket.DrawCentered(Position + ticket_shift);

            // edit button
            ActiveFont.DrawOutline("Edit", Position + ticket_shift + new Vector2(card.Width / 4, ActiveFont.LineHeight / 2),
                    Vector2.One, new Vector2(1, Math.Clamp(edit_scale, 0, 1)),
                    Color.White, 2f, Color.Black);
            // save cancel delete

            ActiveFont.DrawOutline("Save", Position + ticket_shift + new Vector2(atButton == Butt.Save ? wiggler.Value * 8f : 0, -70),
                    Vector2.One * 0.5f, new Vector2(1, save_scale),
                    atButton == Butt.Save ? flashing_color : Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline("Cancel", Position + ticket_shift + new Vector2(atButton == Butt.Cancel ? wiggler.Value * 8f : 0, 0),
                    Vector2.One * 0.5f, new Vector2(1, cancel_scale),
                    atButton == Butt.Cancel ? flashing_color : Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline("Delete", Position + ticket_shift + new Vector2(atButton == Butt.Delete ? wiggler.Value * 8f : 0, 70),
                    Vector2.One * 0.5f, new Vector2(1, delete_scale),
                    atButton == Butt.Delete ? flashing_color : Color.White, 2f, Color.Black);
        }

        void renderCard()
        {
            card.DrawCentered(Position + card_shift);

            ActiveFont.DrawOutline(emote.animation, Position + card_shift + new Vector2(card.Width / 6, -30) + new Vector2(atButton == Butt.Animation ? wiggler.Value * 8f : 0, 0)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(anim_name_scale, 0)),
                    atButton == Butt.Animation ? flashing_color : Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline(emote.animation, Position + card_shift + new Vector2(card.Width / 6, 10) + new Vector2(atButton == Butt.Spritebank ? wiggler.Value * 8f : 0, 0)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(spritebank_scale, 0)) * 0.8f,
                    atButton == Butt.Spritebank ? flashing_color : Color.White, 2f, Color.Black);

            ActiveFont.Draw(emote.animation, Position + card_shift + new Vector2(card.Width / 6, -30)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(-anim_name_scale, 0)), Color.Black * 0.8f);
            ActiveFont.Draw(emote.animation, Position + card_shift + new Vector2(card.Width / 6, 10)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(-spritebank_scale, 0)) * 0.8f, Color.Black * 0.6f);


            Vector2 keys_center = new(card.Width / 6, 50);
            if (emote.bind.Keys.Count == 1)
            {
                MTexture tex = GFX.Gui[$"controls/keyboard/{emote.bind.Keys[0]}"];
                tex.DrawOutlineCentered(Position + card_shift + keys_center + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)
                        , atButton == Butt.Keys ? flashing_color : Color.White);
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
                    t.DrawOutlineCentered(Position + card_shift + keys_center + new Vector2(half - totalwidth - t.Width / 2, 0) + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)

                            , atButton == Butt.Keys ? flashing_color : Color.White);
                }
            }
            else
            {
                ActiveFont.Draw("Add keys", Position + card_shift + keys_center + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)

                        , Vector2.One, new Vector2(0.5f, 0.5f), atButton == Butt.Keys ? flashing_color : Color.White);
            }
            sprite.Render();
        }

        public override void Update()
        {
            base.Update();
            sprite.Position = Position + new Vector2(-card.Width / 4, card.Height / 4) + card_shift;
            sprite.Update();

            wiggler.Update();

            if (coro != null)
                coro.MoveNext();

            if (Focused)
            {
                if (Input.MenuCancel.Pressed)
                {
                    coro = OnClose();
                }
                else if (Input.MenuDown.Pressed)
                {
                    if (atButton == Butt.Delete)
                        atButton = Butt.None;

                    atButton++;
                    wiggler.Start();
                }
                else if (Input.MenuUp.Pressed)
                {
                    atButton--;
                    if (atButton == Butt.None)
                        atButton = Butt.Delete;
                    wiggler.Start();

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

                edit_scale = 1f - d;

                anim_name_scale = Math.Clamp(-1 + 4 * d, -1, 1);
                spritebank_scale = Math.Clamp(-1 + 4 * (d - 0.5f), -1, 1);

                card_shift = new Vector2(l - (1f - easingo) * (l - cs.X), 0);
                ticket_shift = new Vector2(r - (1f - easingo) * (r - ts.X), 0);
                yield return null;

            }
            edit_scale = 0;
            anim_name_scale = spritebank_scale = 1f;
            // drawTicketOnTop = true;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                float easing = Math.Clamp(Ease.CubeInOut(d), 0, 1);

                save_scale = Math.Clamp(d * 3, 0, 1);
                cancel_scale = Math.Clamp((d - 0.33f) * 3, 0, 1);
                delete_scale = Math.Clamp((d - 0.66f) * 3, 0, 1);

                card_shift = new Vector2(l * (1f - easing), 0);
                ticket_shift = new Vector2(r * (1f - easing),
                        b * easing);
                yield return null;
            }
            save_scale = cancel_scale = delete_scale = 1f;
            Focused = true;
            atButton = Butt.Animation;
            wiggler.Start();
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
            atButton = Butt.None;
            // parent.Focused = true;
            //
            // card_shift = Vector2.Zero;
            // ticket_shift = Vector2.Zero;


            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {

                float easingo = Math.Clamp(Ease.CubeOut(d), 0f, 1f);
                delete_scale = 1f - Math.Clamp(d * 3, 0, 1);
                cancel_scale = 1f - Math.Clamp((d - 0.33f) * 3, 0, 1);
                save_scale = 1f - Math.Clamp((d - 0.66f) * 3, 0, 1);

                spritebank_scale = -Math.Clamp(-1 + 4 * d, -1, 1);
                anim_name_scale = -Math.Clamp(-1 + 4 * (d - 0.5f), -1, 1);

                card_shift = new Vector2(l - (1f - easingo) * (l - cs.X), 0);
                ticket_shift = new Vector2(r - (1f - easingo) * (r - ts.X), b * (1f - easingo));

                yield return null;
            }

            parent.releaseCards = true;

            save_scale = cancel_scale = delete_scale = 0f;
            anim_name_scale = spritebank_scale = -1f;
            drawTicketOnTop = false;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                edit_scale = d;

                card_shift.X = -card.Width / 4 + (-card.Width / 4) * (1f - Ease.CubeInOut(d));
                ticket_shift.X = card.Width / 4 + (card.Width / 4) * (1f - Ease.CubeInOut(d));

                yield return null;
            }
            edit_scale = 1f;

            parent.Focused = true;

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

            edit_scale = 1;
            anim_name_scale = spritebank_scale = -1f;
            save_scale = cancel_scale = delete_scale = 0f;

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
