using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;

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

        // int width, height;

        Vector2 ticket_shift, card_shift;
        public IEnumerator coro;

        public override void Render()
        {
            base.Render();

            if (!Visible)
                return;

            HudRenderer.EndRender();
            HudRenderer.BeginRender(null, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);

            ticket.DrawCentered(Position + ticket_shift);
            card.DrawCentered(Position + card_shift);
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

            Visible = X > -card.Width && X < Celeste.TargetWidth + card.Width
                && Y > -card.Width && Y < Celeste.TargetHeight + card.Height;

            // EmoteModModule.echo($"lol updating at {X}:{Y}");
        }

        public void Select()
        {
            coro = OnSelect();
        }

        public void Deselect()
        {
            coro = OnDeselect();
        }

        #region animations

        public IEnumerator OnSelect()
        {
            Vector2 cs = card_shift;
            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                EmoteModModule.echo($"moving {cs.X}");
                ticket_shift.X = Ease.CubeInOut(d) * card.Width / 4;
                card_shift.X = -Ease.CubeInOut(d) * card.Width / 4;
                yield return null;
            }
            Focused = true;
            yield return null;

        }

        public IEnumerator OnDeselect()
        {
            Vector2 ts = ticket_shift;
            Vector2 cs = card_shift;

            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                // card_shift.X = Position
                card_shift = cs * Ease.CubeInOut(1f - d);
                ticket_shift = ts * Ease.CubeInOut(1f - d);

                yield return null;

            }
            Focused = false;
            yield return null;

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
