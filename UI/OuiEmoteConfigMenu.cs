using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

using Celeste.Mod.UI;
using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
using Celeste.Mod.Core;
using Celeste.Mod.Helpers;
using Mono.Cecil;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Threading;
using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    public class OuiEmoteConfigMenu : Oui
    {

        TextMenu menu;
        List<EmoteCard> cards;


        public override IEnumerator Enter(Oui from)
        {
            // TODO: figure out when to make it focused
            Visible = true;
            Focused = true;

            EmoteModModule.echo("on enter");

            cards = new();

            foreach (EmoteEntry emote in EmoteModModule.Settings.Emotes)
            {
                int index = cards.Count();

                cards.Add(new EmoteCard(emote)
                {
                    X = Celeste.TargetWidth / 2,
                    Y = 0 + (index + 1) * 300,
                    Visible = Y < Celeste.TargetHeight + 300 && Y > -300
                });
                Scene.Add(cards[index]);
            }

            // for (int i = 0; i < 5; i++)
            // {
            //     cards.Add(new EmoteCard()
            //     {
            //         Y = 0 + cards.Count * 300,
            //         X = Celeste.TargetWidth / 2,
            //         Active = true
            //     });
            //     Scene.Add(cards[cards.Count - 1]);
            //     yield return null;
            //
            // }
            // EmoteCard test = new();

            // Scene.Add(test);

            yield return null;

            // test.X = 960f;
            // test.Active = true;
            // test.Visible = true;
        }

        public override void Render()
        {
            base.Render();

            ActiveFont.Draw("text", new Vector2(960f, 50f), Vector2.One, Vector2.One, Color.White);
            // EmoteModModule.echo("menu render");
            //
            // foreach (EmoteCard card in cards)
            // {
            //     card.Render();
            // }
        }

        public override void Update()
        {
            if (Focused && Input.MenuCancel.Pressed)
            {
                Overworld.Goto<OuiModOptions>();
            }
            base.Update();
        }

        public override IEnumerator Leave(Oui next)
        {
            // throw new NotImplementedException();
            Focused = false;
            Visible = false;

            foreach (EmoteCard card in cards)
            {
                card.RemoveSelf();
            }

            cards.Clear();

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
    //             menuInstance.parameters = parameters;
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
