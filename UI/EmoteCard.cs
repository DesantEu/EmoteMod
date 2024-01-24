using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.EmoteMod
{
    internal class EmoteCard : Entity
    {
        MTexture card = GFX.Gui["emotemod/card"];
        MTexture ticket = GFX.Gui["emotemod/ticket"];

        float animation_scale = 10f;
        public EmoteEntry emote;

        PlayerSprite sprite;

        int width, height;

        Vector2 ticket_shift, card_shift;

        public override void Render()
        {
            base.Render();

            HudRenderer.EndRender();
            HudRenderer.BeginRender(null, Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp);

            ticket.DrawCentered(Position);
            card.DrawCentered(Position);
            sprite.Render();

            EmoteModModule.echo($"lol rendering at {X}:{Y}");
            HudRenderer.EndRender();
            HudRenderer.BeginRender();
        }

        public override void Update()
        {
            base.Update();
            sprite.Position = Position + new Vector2(-width / 4, height / 4);
            sprite.Update();

            EmoteModModule.echo($"lol updating at {X}:{Y}");
        }

        public void Select()
        {

        }

        public EmoteCard(EmoteEntry emote)
        {
            Tag = Tags.HUD;

            this.emote = emote;
            this.width = card.Width;
            this.height = card.Height;

            this.ticket_shift = Vector2.Zero;
            this.card_shift = Vector2.Zero;

            sprite = new(PlayerSpriteMode.Madeline);
            sprite.Scale = Vector2.One * animation_scale;

            if (sprite.Animations.ContainsKey(emote.animation))
                sprite.Play(emote.animation);
        }


    }
}
