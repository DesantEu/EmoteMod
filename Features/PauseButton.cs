using System;
using Celeste;
using Celeste.Mod;
using Celeste.Mod.UI;
using EmoteMod.Module;
using EmoteMod.UI;
using Monocle;

namespace EmoteMod.Features

{
    public class PauseButton
    {
        public static void Load()
        {


            Everest.Events.Level.OnCreatePauseMenuButtons += OnCreatePauseMenuButtons;

        }

        private static void OnCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
        {
            EmoteModModule.echo("finding buttons");
            // find retry button
            int index = menu.Items.FindIndex(item =>
                    item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("menu_pause_retry"));
            EmoteModModule.echo($"retry: {index}");
            // or skip cutscene
            if (index == -1)
                index = menu.Items.FindIndex(item =>
                        item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("menu_pause_skip_cutscene"));
            EmoteModModule.echo($"skip: {index}");

            // skip if none found
            if (index != -1)
                menu.Insert(index + 1, BuildConfigButton(menu, true, null));
            else
            {
                foreach (TextMenu.Item item in menu.Items)
                {
                    if (item.GetType() == typeof(TextMenu.Button))
                        EmoteModModule.echo(((TextMenu.Button)item).Label);
                }
            }
        }

        // mostly stolen from ex variants
        public static TextMenu.Button BuildConfigButton(TextMenu parentMenu, bool inGame, Action backToParentMenu)
        {
            if (inGame)
            {
                return (TextMenu.Button)new TextMenu.Button("Emotes COnfig").Pressed(() =>
                {

                    Level level = Engine.Scene as Level;
                    bool fromPause = level.PauseMainMenuOpen;
                    level.PauseMainMenuOpen = false;
                    OuiEmoteConfigMenu menu = null;

                    if (OuiModOptions.Instance?.Overworld == null)
                    {
                        menu = (OuiEmoteConfigMenu)Activator.CreateInstance(typeof(OuiEmoteConfigMenu));
                        EmoteModModule.echo("had to create instance");
                    }
                    else
                    {
                        menu = OuiModOptions.Instance.Overworld.GetUI<OuiEmoteConfigMenu>();
                        EmoteModModule.echo("found");
                    }

                    menu.EnterFromPause();
                    menu.parentMenu = parentMenu;

                    parentMenu.RemoveSelf();
                    level.Add(menu);
                });
            }
            else
            {
                return (TextMenu.Button)new TextMenu.Button("Emotes Config").Pressed(() =>
                {
                    Audio.Play(SFX.ui_main_savefile_rename_start);
                    OuiModOptions.Instance.Overworld.Goto<OuiEmoteConfigMenu>();

                });
            }
        }
    }
}
