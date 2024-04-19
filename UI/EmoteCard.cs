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
    public enum Butt
    {
        None, Animation, Spritebank, Keys, Save, Cancel, Delete
    }
    public class EmoteCard : Entity
    {
        MTexture card = MTN.FileSelect.Textures["card"];
        // MTexture card = GFX.Gui["emotemod/card"];
        // MTexture ticket = GFX.Gui["emotemod/ticket"];
        MTexture ticket = MTN.FileSelect.Textures["ticket"];

        // SpriteGrid gallery;

        float animation_scale = 10f;
        public EmoteEntry emote;
        private EmoteInfo placeholder_info;
        public EmoteInfo info => emote.GetInfo() == null ? placeholder_info : emote.GetInfo();
        public EmoteEntry old_emote;

        PlayerSprite sprite;
        PlayerHair hair;
        public bool Focused;
        public bool Selected;
        public bool Opened;
        bool changesMade;
        bool isReadingKey = false;
        public bool stopVisibilityChecks = false;
        public Butt atButton = Butt.None;
        Wiggler wiggler = Wiggler.Create(0.25f, 3f);
        Wiggler save_cancel_wiggler = Wiggler.Create(0.25f, 3f);

        float edit_scale, anim_name_scale, spritebank_scale, save_scale, cancel_scale, delete_scale;

        public OuiEmoteConfigMenu parent;
        Color flashing_color => !Settings.Instance.DisableFlashes && !this.Scene.BetweenInterval(0.1f) ? TextMenu.HighlightColorB : TextMenu.HighlightColorA;


        Vector2 ticket_shift, card_shift;
        bool drawTicketOnTop = false;

        public IEnumerator coro;
        List<MTexture> keytextures = new();
        float total_keys_width;

        public override void Render()
        {
            base.Render();

            if (!Visible)
                return;

            // bye bye blur
            HudRenderer.EndRender();
            HudRenderer.BeginRender(null, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);


            if (!drawTicketOnTop)
                renderTicket();
            renderCard();
            if (drawTicketOnTop)
                renderTicket();


            // hello blur :3
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
            // literally unreadable
            ActiveFont.DrawOutline("Save", Position + ticket_shift + new Vector2((atButton == Butt.Save ? wiggler.Value * 8f : 0) + save_cancel_wiggler.Value * 8f, -70),
                    Vector2.One * 0.5f, new Vector2(1, save_scale),
                    atButton == Butt.Save ? flashing_color : Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline("Cancel", Position + ticket_shift + new Vector2((atButton == Butt.Cancel ? wiggler.Value * 8f : 0) - save_cancel_wiggler.Value * 8f, 0),
                    Vector2.One * 0.5f, new Vector2(1, cancel_scale),
                    atButton == Butt.Cancel ? flashing_color : Color.White, 2f, Color.Black);
            ActiveFont.DrawOutline("Delete", Position + ticket_shift + new Vector2(atButton == Butt.Delete ? wiggler.Value * 8f : 0, 70),
                    Vector2.One * 0.5f, new Vector2(1, delete_scale),
                    atButton == Butt.Delete ? flashing_color : Color.White, 2f, Color.Black);
        }

        void renderCard()
        {
            card.DrawCentered(Position + card_shift);

            // emote name
            ActiveFont.DrawOutline(info.spritebank == "Unknown" ? emote.animation : info.animation
                    , Position + card_shift + new Vector2(card.Width / 6, -30) + new Vector2(atButton == Butt.Animation ? wiggler.Value * 8f : 0, 0)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(anim_name_scale, 0)),
                    atButton == Butt.Animation ? flashing_color : Color.White, 2f, Color.Black);
            // spritebank
            string sb = this.info.isCustom ? this.info.spritebank : "Default";
            ActiveFont.DrawOutline(sb, Position + card_shift + new Vector2(card.Width / 6, 10) + new Vector2(atButton == Butt.Spritebank ? wiggler.Value * 8f : 0, 0)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(spritebank_scale, 0)) * 0.8f,
                    atButton == Butt.Spritebank ? flashing_color : Color.White, 2f, Color.Black);

            // same but with outline
            ActiveFont.Draw(info.spritebank == "Unknown" ? emote.animation : info.animation,
                    Position + card_shift + new Vector2(card.Width / 6, -30)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(-anim_name_scale, 0)), Color.Black * 0.8f);
            ActiveFont.Draw(sb, Position + card_shift + new Vector2(card.Width / 6, 10)
                    , new Vector2(0.5f, 1), new Vector2(1, Math.Max(-spritebank_scale, 0)) * 0.8f, Color.Black * 0.6f);


            // keybinds
            Vector2 keys_center = new(card.Width / 6, 50);
            // draw one
            // if (emote.bind.Keys.Count == 1)
            // {
            //     MTexture tex = GFX.Gui[$"controls/keyboard/{emote.bind.Keys[0]}"];
            //     tex.DrawOutlineCentered(Position + card_shift + keys_center + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)
            //             , atButton == Butt.Keys ? flashing_color : Color.White);
            // }
            // draw an array centered
            if (keytextures.Count > 0)
            // if (emote.bind.Keys.Count > 0)
            {
                // List<MTexture> keytextures = new();
                // float totalwidth = 0;
                //
                // foreach (Keys k in emote.bind.Keys)
                // {
                //     MTexture tex = GFX.Gui[$"controls/keyboard/{k}"];
                //     keytextures.Add(tex);
                //     totalwidth += tex.Width;
                //     // tex.DrawOutlineCentered(Position + card_shift + new Vector2(card.Width / 6, 50));
                // }
                float half = total_keys_width / 2;
                float used = 0;
                foreach (MTexture t in keytextures)
                {
                    used += t.Width;

                    t.DrawOutlineCentered(Position + card_shift + keys_center
                            + new Vector2(half - total_keys_width + used - t.Width / 2, 0)
                            + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)
                            , atButton == Butt.Keys ? flashing_color : Color.White);
                }
            }
            // or draw text if none
            else
            {
                ActiveFont.Draw("Add keys", Position + card_shift + keys_center + new Vector2(atButton == Butt.Keys ? wiggler.Value * 8f : 0, 0)

                        , new Vector2(0.5f, 0.5f), Vector2.One, atButton == Butt.Keys ? flashing_color : Color.White);
            }

            sprite.Position = Position + new Vector2(-card.Width / 4, card.Height / 4) + card_shift;
            sprite.Render();
        }

        public void RegenerateKeyTextures(bool controller = false)
        {
            keytextures = new();
            total_keys_width = 0;

            foreach (Keys k in emote.bind.Keys)
            {
                MTexture tex = GFX.Gui[$"controls/keyboard/{k}"];
                keytextures.Add(tex);
                total_keys_width += tex.Width;
                // tex.DrawOutlineCentered(Position + card_shift + new Vector2(card.Width / 6, 50));
            }
        }

        public override void Update()
        {
            // if (Focused)
            //     EmoteModModule.echo($"old keys:{old_emote.bind.Keys.Count}, new:{emote.bind.Keys.Count}");
            base.Update();
            // do we need to do this?
            // sprite.Position = Position + new Vector2(-card.Width / 4, card.Height / 4) + card_shift;
            sprite.Update();

            wiggler.Update();
            save_cancel_wiggler.Update();

            if (coro != null)
                coro.MoveNext();

            // inputs stuff
            if (Focused)
            {
                // try exit
                if (Input.MenuCancel.Pressed)
                {
                    if (changesMade)
                    {
                        // save_cancel_wiggle = true;
                        save_cancel_wiggler.Start();
                    }
                    else
                        coro = OnClose();
                }
                // down
                else if (Input.MenuDown.Pressed)
                {
                    if (atButton == Butt.Delete)
                        atButton = Butt.None;

                    atButton++;
                    wiggler.Start();
                }
                // up
                else if (Input.MenuUp.Pressed)
                {
                    atButton--;
                    if (atButton == Butt.None)
                        atButton = Butt.Delete;
                    wiggler.Start();

                }
                // clear keys
                else if (Input.MenuJournal.Pressed)
                {
                    if (atButton == Butt.Keys)
                    {
                        changesMade = true;
                        emote.bind.Keys.Clear();
                        RegenerateKeyTextures();
                    }
                }
                // confirm
                else if (Input.MenuConfirm.Pressed)
                {
                    // save
                    if (atButton == Butt.Save)
                    {
                        int index = EmoteModModule.Settings.Emotes.IndexOf(old_emote);
                        EmoteModModule.Settings.Emotes[index] = emote;
                        coro = OnClose();
                    }
                    // cancel
                    else if (atButton == Butt.Cancel)
                    {
                        changesMade = false;
                        emote = old_emote;
                        RegenerateKeyTextures();
                        coro = OnClose();
                    }
                    // add keys/buttons
                    else if (atButton == Butt.Keys)
                    {
                        coro = OnAddBind();
                    }
                    // change spritemode
                    else if (atButton == Butt.Spritebank)
                    {
                        Focused = false;
                        atButton = Butt.None;

                        // gallery = new SpriteGrid(emote.GetInfo().spritebank, this);
                        // parent.Scene.Add(gallery);
                        parent.gallery = new(info.spritebank, this);

                    }
                }
            }

            if (!stopVisibilityChecks)
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
        public IEnumerator OnAddBind()
        {
            // todo rewrite this pretty much
            // consider mouse and gamepad
            Focused = false;
            isReadingKey = true;
            // this feels dumb idk
            bool confirm_skip = true;

            MTexture new_button_placeholder = GFX.Gui["controls/keyboard/OemQuestion"];
            keytextures.Add(new_button_placeholder);
            total_keys_width += new_button_placeholder.Width;

            List<Keys> menu_confirm_keys = Settings.Instance.Confirm.Keyboard;
            List<Buttons> menu_confirm_buttons = Settings.Instance.Confirm.Controller;
            List<MInput.MouseData.MouseButtons> menu_confirm_mouse = Settings.Instance.Confirm.Mouse;

            for (float d = 0; d < 5f; d += Engine.DeltaTime)
            {
                // this feels very dumb but cheching input.menuconfirm doesnt work idk
                if (confirm_skip && menu_confirm_keys.Intersect(Monocle.MInput.Keyboard.CurrentState.GetPressedKeys()).Count() > 0)
                {
                    yield return null;
                }
                else
                {
                    confirm_skip = false;

                    if (MInput.Keyboard.HasAnyInput())
                    {
                        changesMade = true;
                        Keys pressed_key = MInput.Keyboard.CurrentState.GetPressedKeys()[0];

                        EmoteModModule.echo("looking up");
                        if (emote.bind.Keys.Contains(pressed_key))
                            emote.bind.Keys.Remove(pressed_key);
                        else
                            emote.bind.Keys.Add(pressed_key);
                        break;
                    }
                    // else if (MInput.GamePads[Input.Gamepad].HasAnyInput())
                    // {
                    //
                    // }
                    else
                        yield return null;
                }
            }

            // void all menu button presses
            foreach (FieldInfo f in typeof(Input).GetFields().Where(i => i.Name.Contains("Menu")))
                f.FieldType.GetMethod("ConsumePress").Invoke(f.GetValue(null), null);

            RegenerateKeyTextures();
            isReadingKey = false;
            Focused = true;
            yield return null;
        }
        // when floating over the card
        // TODO: remake to work from any position (add cs/ts)
        public IEnumerator OnSelect()
        {
            Vector2 cs = card_shift;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                ticket_shift.X = Ease.CubeInOut(d) * card.Width / 4;
                card_shift.X = -Ease.CubeInOut(d) * card.Width / 4;
                yield return null;
            }
            Selected = true;
            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
            yield return null;

        }

        // when MenuConfirm is pressed
        public IEnumerator OnOpen()
        {
            // whar
            old_emote = emote;
            emote = old_emote.Clone();


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

        // when menu back is pressed
        public IEnumerator OnClose()
        {
            Vector2 ts = ticket_shift;
            Vector2 cs = card_shift;
            float l = -card.Width / 2;
            float r = card.Width / 2;
            float b = card.Height;

            Focused = false;
            atButton = Butt.None;

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
            // this.info = emote.GetInfo();

            // if (this.info == null)
            {
                // this.info = new();
                placeholder_info = new();
                placeholder_info.animation = "faint";
                placeholder_info.spritebank = "Unknown";
                placeholder_info.isCustom = true;
                placeholder_info.spritemode = PlayerSpriteMode.Madeline;
            }

            if (info.isCustom)
            {
                Emote.addCustomEmote(emote.animation);
            }

            RegenerateKeyTextures();
            // On.Monocle.Engine.

            edit_scale = 1;
            anim_name_scale = spritebank_scale = -1f;
            save_scale = cancel_scale = delete_scale = 0f;

            this.ticket_shift = Vector2.Zero;
            this.card_shift = Vector2.Zero;
            this.Focused = false;

            sprite = new(this.info.spritemode);
            sprite.Scale = Vector2.One * animation_scale;

            hair = new(sprite) { SimulateMotion = true };
            hair.Visible = true;
            // hair.

            if (emote.GetInfo() != null)
                sprite.Play(emote.animation);
            else
                sprite.Play(info.animation);

            // if (sprite.Animations.ContainsKey(emote.animation))
            //     sprite.Play(emote.animation);

        }


    }
}
