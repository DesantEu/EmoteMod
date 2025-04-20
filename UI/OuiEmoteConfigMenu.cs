using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.UI;
using Celeste.Mod.CelesteNet.Client;
using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System;
namespace Celeste.Mod.EmoteMod
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
            cards_shift = Celeste.TargetHeight / 2 - 310;
            atCard = 0;

            // make cards
            // TODO: maybe change to for and remove index
            foreach (EmoteEntry emote in EmoteModModule.Settings.Emotes)
            {
                int index = cards.Count;
                emote.RefreshInfo();

                cards.Add(new EmoteCard(emote)
                {
                    X = Celeste.TargetWidth / 2,
                    Y = cards_shift + index * 310,
                    parent = this,
                    Tag = this.Tag,
                });
                Scene.Add(cards[index]);
            }

            // Scene.Add(gallery);
            Audio.Play("event:/ui/main/whoosh_list_in");

            int centerw = Celeste.TargetWidth / 2;
            int offscreenw = Celeste.TargetWidth + 300;

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

#if DEBUG
            ActiveFont.Draw(debug_text, Vector2.Zero, Vector2.Zero, Vector2.One, Color.White);
#endif
            // ActiveFont.Draw(Dialog.Clean("BTN_CONFIG_INFO"), new Vector2(960f, 50f), Vector2.One, Vector2.One, Color.White);
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
                    // Audio.Play("event:/ui/main/savefile_rename_start");
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
            float new_shift = Celeste.TargetHeight / 2 - 310 * target;

            for (float d = 1f; d > 0f; d -= Engine.DeltaTime * 4)
            {
                cards_shift = new_shift - (new_shift - old_shift) * Ease.CubeIn(d);
                for (int i = 0; i < cards.Count; i++)
                {
                    cards[i].Position = new Vector2(Celeste.TargetWidth / 2,
                            cards_shift + i * 310f);
                }

                yield return null;
            }
        }

        IEnumerator FocusCard()
        {
            Vector2 focused_target_pos = new Vector2(Celeste.TargetWidth / 2, Celeste.TargetHeight / 2 - 150f);
            // move cards out of the way
            for (float d = 0; d < 1f; d += Engine.DeltaTime * 4)
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    Vector2 default_pos = new(Celeste.TargetWidth / 2,
                                cards_shift + i * 310f);
                    if (i < atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, -Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else if (i > atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, Celeste.TargetHeight * Ease.CubeIn(d));
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
                    Vector2 default_pos = new(Celeste.TargetWidth / 2,
                                cards_shift + i * 310f);
                    if (i < atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, -Celeste.TargetHeight * Ease.CubeIn(d));
                    }
                    else if (i > atCard)
                    {
                        cards[i].Position = default_pos + new Vector2(0, Celeste.TargetHeight * Ease.CubeIn(d));
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
            // throw new NotImplementedException();
            Focused = false;
            Visible = false;

            int centerw = Celeste.TargetWidth / 2;
            int offscreenw = Celeste.TargetWidth + 300;
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

        // public static void Goto<T>(Action<Overworld> backToParentMenu, params object[] parameters) where T : OuiGenericMenu
        // {
        //     // get the instance for the menu we want to go to (all Oui's are singletons)
        //     Overworld overworld = OuiModOptions.Instance.Overworld;
        //     OuiGenericMenu menuInstance = overworld.GetUI<T>();
        //
        //     // set up the menu instance
        //     menuInstance.backToParentMenu = backToParentMenu;
        //     menuInstance.parameters = parameters;
        //
        //     // then navigate to it
        //     overworld.Goto<T>();
        // }
    }

    // public class EmoteConfigMenu : OuiGenericMenu, OuiModOptions.ISubmenu
    // {
    //     public override string MenuName => "Emote Config";
    //
    //     public override void Update()
    //     {
    //         if (menu != null)
    //         {
    //             menu.ItemSpacing = 50;
    //
    //             foreach (TextMenu.Item item in menu.Items)
    //             {
    //                 item.
    //             }
    //         }
    //         base.Update();
    //     }
    //
    //     public override void Render()
    //     {
    //         if (menu != null)
    //             foreach (TextMenu.Item item in menu.Items)
    //             {
    //                 Sprite.Animation anim = PlayerHelper.getAnimationByName("granny:laugh");
    //
    //             }
    //
    //         base.Render();
    //     }
    //
    //     protected override void addOptionsToMenu(TextMenu menu)
    //     {
    //         menu.Add(new TextMenu.Button("granny:laugh") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //         menu.Add(new TextMenu.Button("player:spin") { }.Pressed(() => { EmoteModModule.echo("haha"); }));
    //     }
    //
    //
    // }
    // public class ConfigEmote : OuiGenericMenu, OuiModOptions.ISubmenu
    // {
    //
    //     public override void Update()
    //     {
    //         base.Update();
    //     }
    //     protected override void addOptionsToMenu(TextMenu menu) // submenu
    //     {
    //     }
    //
    // }
    // public static class ConfigEmote
    // {
    //     public class OuiConfigEmote : Oui
    //     {
    //         private TextMenu menu;
    //
    //         protected Action<Overworld> backToParentMenu;
    //
    //         protected bool canGoBack = true;
    //
    //         private float alpha = 0f;
    //
    //
    //         public override void Update()
    //         {
    //             if (menu != null && menu.Focused && Selected && canGoBack && Input.MenuCancel.Pressed)
    //             {
    //                 // Back was pressed
    //                 Audio.Play(SFX.ui_main_button_back);
    //                 backToParentMenu(Overworld);
    //             }
    //
    //             base.Update();
    //         }
    //         public override void Render()
    //         {
    //             if (alpha > 0f)
    //             {
    //                 Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * alpha * 0.4f);
    //             }
    //             base.Render();
    //         }
    //
    //         public override IEnumerator Enter(Oui from)
    //         {
    //             throw new NotImplementedException();
    //         }
    //
    //         public override IEnumerator Leave(Oui next)
    //         {
    //             yield return null;
    //
    //         }
    //
    //         public static void Goto<T>(Action<Overworld> backToParentMenu, params object[] parameters) where T : OuiGenericMenu
    //         {
    //             // get the instance for the menu we want to go to (all Oui's are singletons)
    //             Overworld overworld = OuiModOptions.Instance.Overworld;
    //             OuiGenericMenu menuInstance = overworld.GetUI<T>();
    //
    //             // set up the menu instance
    //             menuInstance.backToParentMenu = backToParentMenu;
    //      menuInstance.parameters = parameters;
    //
    //             // then navigate to it
    //             overworld.Goto<T>();
    //         }
    //     }
    // }

    // public class ConfigEmote : Overlay, IDisposable
    // {
    //
    //     private TextMenu menu;
    //
    //     private IEnumerator Routine()
    //     {
    //         yield return FadeIn();
    //
    //         menu = new TextMenu() { AutoScroll = true };
    //
    //         menu.Add(new TextMenu.Button("text123") { }.Pressed(() => { EmoteModModule.echo("button pressed!"); }));
    //
    //         ConfigureMenu();
    //     }
    //
    //     public override void Update()
    //     {
    //         menu?.Update();
    //
    //         base.Update();
    //     }
    //
    //     public override void Render()
    //     {
    //         RenderFade();
    //
    //         if (menu != null)
    //         {
    //             menu.Alpha = Fade;
    //             menu.Render();
    //         }
    //
    //         Vector2 textPos = new Vector2(Celeste.TargetWidth * 0.3f, Celeste.TargetHeight * 0.35f);
    //         ActiveFont.Draw("title wtf?!?", textPos, new Vector2(0, 1), new Vector2(3), Color.White * Fade);
    //
    //         base.Render();
    //     }
    //
    //     private void ConfigureMenu()
    //     {
    //         menu.ItemSpacing = 4;
    //         menu.RecalculateSize();
    //
    //         menu.Position = new Vector2(Celeste.TargetWidth * 0.15f, Celeste.TargetHeight * 0.6f);
    //         menu.Justify = new Vector2(0.5f, 0.5f);
    //     }
    //
    //     public void Dispose()
    //     {
    //
    //     }
    //     // public static Player player;
    // }
}
