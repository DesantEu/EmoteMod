using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.UI;
using System;
using Celeste;
using EmoteMod.Module;

namespace EmoteMod.UI
{
    public class OuiEmoteConfigMenu : Oui, OuiModOptions.ISubmenu
    {

        TextMenu menu;
        public List<EmoteCard> cards;

        int atCard;
        IEnumerator coro;
        float cards_shift = 0f;

        public bool releaseCards = false;
        public string debug_text = "#Debug";
        private bool from_pause = false;
        public TextMenu parentMenu;

        public SpriteGrid gallery;


        public override IEnumerator Enter(Oui from)
        {
            from_pause = false;
            Tag = Tags.HUD;
            yield return OnEnter();
        }

        public void EnterFromPause()
        {
            from_pause = true;
            Tag = Tags.HUD | Tags.PauseUpdate;
            // AddTag(Tags.HUD);
            coro = OnEnter();
        }

        private IEnumerator OnEnter()
        {
            Visible = true;
            cards = new();
            cards_shift = Celeste.Celeste.TargetHeight / 2 - 310;
            atCard = 0;

            // make cards
            // TODO: maybe change to for and remove index
            foreach (EmoteEntry emote in EmoteModModule.Settings.Emotes)
            {
                int index = cards.Count;
                emote.RefreshInfo();

                cards.Add(new EmoteCard(emote)
                {
                    X = Celeste.Celeste.TargetWidth / 2,
                    Y = cards_shift + index * 310,
                    parent = this,
                    Tag = this.Tag,
                });
                Scene.Add(cards[index]);
            }

            // Scene.Add(gallery);
            Audio.Play("event:/ui/main/whoosh_list_in");

            int centerw = Celeste.Celeste.TargetWidth / 2;
            int offscreenw = Celeste.Celeste.TargetWidth + 300;

            // make cool animation for cards
            for (float d = 0f; d < 1f; d += Engine.DeltaTime * 2f)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    // TODO: this feels like a lot of numbers
                    float shift = (offscreenw * 2) * (1f - Ease.CubeOut(d)) - 300 * Math.Min(4, 4 - i);
                    cards[i].X = centerw + Math.Max(0, shift);
                }
                yield return null;
            }

            Audio.Play("event:/ui/main/whoosh_savefile_in");
            cards[atCard].Select();
            Focused = true;
        }


        public override void Render()
        {
            base.Render();
        }

        public override void Update()
        {
            if (coro != null)
                coro.MoveNext();

            if (Focused)
            {
                if (Input.MenuCancel.Pressed)
                {
                    if (!from_pause)
                        Overworld.Goto<OuiModOptions>();
                    else
                    {
                        coro = OnLeave();
                    }
                }
                if (Input.MenuDown.Pressed)
                {
                    Audio.Play("event:/ui/main/rollover_down");
                    cards[atCard].Deselect();
                    atCard++;
                    if (atCard >= cards.Count)
                        atCard = 0;
                    cards[atCard].Select();
                    coro = Refocus();
                }
                if (Input.MenuUp.Pressed)
                {
                    Audio.Play("event:/ui/main/rollover_up");
                    cards[atCard].Deselect();
                    atCard--;
                    if (atCard < 0)
                        atCard = cards.Count - 1;
                    cards[atCard].Select();
                    coro = Refocus();
                }
                if (Input.MenuConfirm.Pressed)
                {
                    cards[atCard].Open();
                    coro = FocusCard();
                    Focused = false;
                }
            }

            base.Update();
        }

        IEnumerator Refocus()
        {
            float old_shift = cards_shift;
            int target = Math.Max(Math.Min(atCard, cards.Count - 2), 1);
            float new_shift = Celeste.Celeste.TargetHeight / 2 - 310 * target;

            for (float d = 1f; d > 0f; d -= Engine.DeltaTime * 4)
            {
                cards_shift = new_shift - (new_shift - old_shift) * Ease.CubeIn(d);
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Position = new Vector2(Celeste.Celeste.TargetWidth / 2,
                            cards_shift + i * 310f);
                }

                yield return null;
            }
        }

        IEnumerator FocusCard()
        {
            Vector2 focused_target_pos = new Vector2(Celeste.Celeste.TargetWidth / 2, Celeste.Celeste.TargetHeight / 2 - 150f);
            // move cards out of the way
            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    Vector2 default_pos = new(Celeste.Celeste.TargetWidth / 2,
                                cards_shift + i * 310f);
                    if (i < atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, -Celeste.Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else if (i > atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, Celeste.Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else
                    {
                        cards[i].Position = default_pos - (default_pos - focused_target_pos) * Ease.CubeInOut(d);
                    }
                }
                yield return null;
            }
            // wait for focus
            while (!releaseCards)
                yield return null;

            releaseCards = false;

            // put cards back
            for (float d = 1; d > 0; d -= Engine.DeltaTime * 4)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    Vector2 default_pos = new(Celeste.Celeste.TargetWidth / 2,
                                cards_shift + i * 310f);
                    if (i < atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, -Celeste.Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else if (i > atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, Celeste.Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else
                    {
                        cards[i].Position = default_pos - (default_pos - focused_target_pos) * Ease.CubeInOut(d);
                    }
                }
                yield return null;
            }
            yield return null;
        }

        public override IEnumerator Leave(Oui next)
        {
            yield return OnLeave();
        }

        private IEnumerator OnLeave()
        {
            Focused = false;
            Visible = false;

            int centerw = Celeste.Celeste.TargetWidth / 2;
            int offscreenw = Celeste.Celeste.TargetWidth + 300;
            int target = Math.Max(Math.Min(atCard, cards.Count - 2), 2);

            Audio.Play("event:/ui/main/whoosh_list_out");
            Audio.Play("event:/ui/main/button_back");

            for (float d = 0f; d < 1f; d += Engine.DeltaTime * 3f)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    float shift = (offscreenw * 2) * (Ease.SineIn(d)) - 600 * Math.Clamp(i - target + 2, 0, 4);
                    cards[i].X = centerw + Math.Max(0, shift);
                }
                yield return null;
            }
            foreach (EmoteCard card in cards)
            {
                card.RemoveSelf();
            }

            cards.Clear();

            if (from_pause)
            {
                Scene.Add(parentMenu);
                this.RemoveSelf();
            }

            yield return null;
        }
    }
}
