using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using System.Linq;
using Celeste;
using EmoteMod.Utility;
using Celeste.Mod;
using EmoteMod.Features;
using EmoteMod.Module;

namespace EmoteMod.UI
{
    public enum Stages
    {
        SpriteBank,
        Emote,
        Done
    }

    public class SpriteGrid : Entity
    {
        IEnumerator coro;
        Stages stage;
        EmoteCard parent;

        List<string> customsForRemoval;
        List<SpriteGridCell> cells;
        List<SpriteGridTitle> titles;
        int cursor_at;
        public float scroll_offset = 0;

        // BG stuff
        float bg_rotation;
        float bg_alpha = 0f;
        Vector2 bg_origin;
        static float bg_thickness = Celeste.Celeste.TargetWidth;

        // Spacing stuff
        public static float spacing = 70;
        float x_offset = Celeste.Celeste.TargetWidth / 2 - 2.5f * SpriteGridCell.default_size * SpriteGridCell.upscale - spacing * 2;
        float y_offset = 90;

        bool Focused;

        float fadeIn => Math.Min(0.125f + cells.Count * 0.02f, 0.375f); // TODO: move to SpriteGridElement constructor probably idk



        public override void Update()
        {
            base.Update();

            if (coro != null)
                coro.MoveNext();


            if (Focused)
            {
                #region Navigation
                if (Input.MenuRight.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at++;

                    if (cursor_at >= cells.Count)
                        cursor_at = 0;

                    cells[cursor_at].Select();

                    coro = Refocus();
                }
                else if (Input.MenuLeft.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at--;

                    if (cursor_at < 0)
                        cursor_at = cells.Count - 1;

                    cells[cursor_at].Select();

                    coro = Refocus();
                }
                else if (Input.MenuUp.Pressed)
                {
                    int cursor_past = cursor_at;
                    cells[cursor_at].Deselect();

                    SpriteGridCell next_cell = cursor_at >= 5 //              if we have something above
                        ? cells.Where(c => //                                 find
                            c.Position.Y < cells[cursor_at].Position.Y //     something above
                            && c.Position.X == cells[cursor_at].Position.X // on the same row
                            ).Last() //                                       and pick the bottom one
                        : cells.Where(c => c.Position.X == cells[cursor_at].Position.X).Last(); // or rollover to lowest

                    cursor_at = cells.IndexOf(next_cell);

                    cells[cursor_at].Select();
                    coro = Refocus();
                }
                else if (Input.MenuDown.Pressed)
                {
                    cells[cursor_at].Deselect();

                    SpriteGridCell next_cell = cursor_at + 5 < cells.Count // if we have something below
                        ? cells.Where(c => //                                 find
                            c.Position.Y > cells[cursor_at].Position.Y //     something below
                            && c.Position.X == cells[cursor_at].Position.X // on the same row
                            ).First() //                                      and pick the top one
                        : cells.Where(c => c.Position.X == cells[cursor_at].Position.X).First(); // or rollover to highest

                    cursor_at = cells.IndexOf(next_cell);

                    cells[cursor_at].Select();
                    coro = Refocus();
                }

                #endregion
                #region Other buttons

                else if (Input.MenuConfirm.Pressed)
                {
                    if (stage == Stages.SpriteBank)
                    {
                        cells[cursor_at].Deselect();
                        coro = SpriteBankToEmote();
                    }
                    else if (stage == Stages.Emote)
                    {
                        SpriteGridCell selected = cells[cursor_at];

                        string animation = selected.info.animation;
                        string sb = titles.First().text;

                        parent.emote = new(sb == "Default" ? animation : $"{sb}:{animation}", parent.emote.bind);
                        parent.emote.RefreshInfo();
                        parent.RefreshSprite();

                        parent.changesMade = true;
                        Focused = false;
                        Audio.Play("event:/new_content/ui/rename_entry_accept_locked");
                        coro = EmoteExit();
                    }
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (stage == Stages.SpriteBank)
                        coro = SpriteBankExit();
                    else if (stage == Stages.Emote)
                        coro = EmoteToSpritebank();
                }
                #endregion
            }
        }

        #region Render
        public override void Render()
        {
            base.Render();

            Draw.LineAngle(bg_origin,
                    bg_rotation, Celeste.Celeste.TargetWidth * 2,
                    Color.Black * bg_alpha, bg_thickness);
        }

        #endregion
        #region Transitions

        private IEnumerator SpriteBankEnter()
        {
            Visible = true;
            parent.stopVisibilityChecks = true;

            fillWithSpriteBanks(gradualFadeIn: true);

            // lmaoo no rotating rectangles?
            // too bad
            // metallica - fade to black
            Audio.Play("event:/ui/main/button_select");
            Audio.Play("event:/ui/main/whoosh_large_in");

            // reset bg pos
            bg_origin = new Vector2(-bg_thickness / 2, Celeste.Celeste.TargetHeight);
            bg_alpha = 0xff;

            // rotate the bg in
            for (float d = 0; d < 1; d += Engine.DeltaTime * 4)
            {
                bg_rotation = ((float)Math.PI) / 2 * (d - 1);
                bg_origin = new Vector2(
                        ((float)Math.Sin(bg_rotation)) * bg_thickness / 2,
                        Celeste.Celeste.TargetHeight - bg_thickness / 2 * ((float)Math.Cos(bg_rotation))
                        );

                yield return null;
            }

            bg_origin = new Vector2(0, 5 + Celeste.Celeste.TargetHeight - bg_thickness / 2);
            bg_rotation = 0;

            parent.Visible = false;

            // obligatory wait
            for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
                yield return null;

            cursor_at = cells.FindIndex(c => c.info.spritebank == parent.info.spritebank);
            if (cursor_at < 0)
                cursor_at = 0;

            cells[cursor_at].Select();


            Focused = true;

            yield return null;

            if (cursor_at != 0)
                coro = Refocus();

        }

        private IEnumerator SpriteBankToEmote()
        {
            // get info
            string spritebank = cells[cursor_at].text;
            List<string> anims;

            if (spritebank == "Default")
            {
                List<string> sbs = AnimationHelper.GlobalEmotes.Keys.ToList();
                anims = GFX.SpriteBank.SpriteData[sbs[0]].Sprite.Animations.Keys.ToList();
                for (int i = 1; i < sbs.Count(); i++)
                {
                    anims = anims.Intersect(GFX.SpriteBank.SpriteData[sbs[i]].Sprite.Animations.Keys.ToList()).ToList();
                }
            }
            else
            {
                anims = GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations.Keys.ToList();
            }

            Focused = false;

            // fade out
            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            Audio.Play("event:/ui/main/button_select");
            Audio.Play("event:/ui/main/whoosh_large_in");

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            Clear();
            cursor_at = 0;
            scroll_offset = 0;

            // display text and emotes
            titles.Add(new(spritebank, new(x_offset, y_offset), this));
            float cells_offset = y_offset + titles.First().total_height;

            foreach (string a in anims)
            {
                EmoteInfo info = AnimationHelper.GetInfo(spritebank == "Default" ? a : $"{spritebank}:{a}");

                if (info == null)
                {
                    Logger.Log(LogLevel.Info, "EmoteMod", $"could not get info for '{a}'");
                    continue;
                }
                else if (info.isCustom)
                {
                    Emote.addCustomEmote($"{spritebank}:{a}");
                    customsForRemoval.Add($"{spritebank}:{a}");
                }

                cells.Add(new(info, a, new_cell_pos(cells.Count), this, cells.Count * 0.02f));
            }

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            stage = Stages.Emote;

            cells[cursor_at].Select();
            Focused = true;

            yield return null;

        }

        private IEnumerator EmoteToSpritebank()
        {
            string spritebank = titles.First().text;
            Focused = false;

            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            Clear();

            fillWithSpriteBanks();

            cursor_at = cells.FindIndex(c => c.info.spritebank == spritebank);
            if (cursor_at < 0)
                cursor_at = 0;

            scroll_offset = get_new_shift();

            // obligatory wait
            for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
                yield return null;

            stage = Stages.SpriteBank;
            cells[cursor_at].Select();
            Focused = true;
            yield return null;
        }

        private IEnumerator EmoteExit()
        {
            Focused = false;

            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 16)
                yield return null;

            parent.Visible = true;

            // remove bg
            bg_origin = new Vector2(Celeste.Celeste.TargetWidth, Celeste.Celeste.TargetHeight - bg_thickness / 2);
            bg_alpha = 0xff;

            // rotate the bg in
            for (float d = 0; d < 1; d += Engine.DeltaTime * 4)
            {
                bg_rotation = ((float)Math.PI) * (1.5f - 0.5f * (1 - d));
                bg_origin = new Vector2(
                        Celeste.Celeste.TargetWidth - ((float)Math.Sin(bg_rotation)) * bg_thickness / 2,
                        Celeste.Celeste.TargetHeight + bg_thickness / 2 * ((float)Math.Cos(bg_rotation))
                        );

                EmoteModModule.echo(Math.Cos(bg_rotation).ToString());
                yield return null;
            }

            bg_origin = new Vector2(Celeste.Celeste.TargetWidth + bg_thickness / 2, Celeste.Celeste.TargetHeight);
            bg_rotation = ((float)Math.PI) * 2;
            bg_alpha = 0f;


            Clear();

            parent.Focused = true;
            parent.atButton = Butt.Animation;
            parent.stopVisibilityChecks = false;
            RemoveSelf();
            yield return null;
        }

        private IEnumerator SpriteBankExit()
        {
            Focused = false;
            // metallica - fade to black
            for (float d = 1; d > 0; d -= Engine.DeltaTime * 12)
            {
                bg_alpha = 0xff * d;
                yield return null;

            }
            bg_alpha = 0;

            yield return null;
            parent.Focused = true;
            parent.atButton = Butt.Spritebank;
            parent.stopVisibilityChecks = false;
            parent.Visible = true;
            Clear();
            RemoveSelf();

        }

        #endregion

        #region Helper methods
        /// <summary>
        /// Returns an absolute cell pos
        /// </summary>
        /// <param name="count_from_title">cell's index starting from the last title</param>
        Vector2 new_cell_pos(int count_from_title)
        {
            float length = SpriteGridCell.upscale * SpriteGridCell.default_size;
            float y_start = titles.Last().Y + titles.Last().total_height; // this assumes there is always a title before any sprites

            // relative pos
            float X = count_from_title % 5 * (length + spacing);
            float Y = count_from_title / 5 * (length + spacing); // TODO: add text spacing too

            // absolute pos
            Vector2 abs_offset = new(x_offset, y_start);
            return new Vector2(X, Y) + abs_offset;
        }

        /// <summary>
        /// Fills the grid with CONTENT
        /// </summary>
        /// <param name="content">Pairs of { title : { display_text : animation } }</param>
        /// <param name="gradualFadeIn">Fade or no fade</param>
        void fillWithContent(Dictionary<string, Dictionary<string, string>> content, bool gradualFadeIn = false)
        {
            Vector2 abs_offset = new(x_offset, y_offset);
            int title_after_ind; // index of the cell before the last title
            Vector2 title_rel_pos = Vector2.Zero;

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp_title_spr in content)
            {
                string title = kvp_title_spr.Key;
                Dictionary<string, string> sprites = kvp_title_spr.Value;

                title_after_ind = cells.Count;


                titles.Add(new SpriteGridTitle(
                    title,
                    abs_offset + title_rel_pos,
                    this,
                    gradualFadeIn ? fadeIn : 0.125f
                ));


                foreach (KeyValuePair<string, string> kvp_text_anim in sprites)
                {
                    string text = kvp_text_anim.Key;
                    string animation = kvp_text_anim.Value;
                    cells.Add(new SpriteGridCell(
                        AnimationHelper.GetInfo(animation),
                        text,
                        new_cell_pos(cells.Count - title_after_ind),
                        this,
                        fadeIn
                    ));
                }

                title_rel_pos = new(0f, cells.Last().Y + cells.Last().total_height);
            }
        }

        /// <summary>
        /// generates and fills spritebank content
        /// </summary>
        /// <param name="gradualFadeIn"></param>
        void fillWithSpriteBanks(bool gradualFadeIn = false)
        {
            Audio.Play("event:/ui/main/button_select");
            Dictionary<string, Dictionary<string, string>> content = new();


            // add not-so-customs
            Dictionary<string, string> global = new();
            global["Default"] = "player:idle";

            foreach (string gsm in AnimationHelper.GlobalEmotes.Keys.ToList())
                global[gsm] = $"{gsm}:idle";

            content["Visible to everyone:"] = global;

            // add customs
            Dictionary<string, string> customs = new();

            List<string> sprites = GFX.SpriteBank.SpriteData.Keys // get all non-global sprites
                .Where(s => !AnimationHelper.GlobalEmotes.ContainsKey(s))
                .ToList();

            foreach (string sprite in sprites)
            {
                if (GFX.SpriteBank.SpriteData[sprite].Sprite.Animations.Count == 0) // skip if empty
                    continue;

                // choose idle or whatever
                List<string> anims = GFX.SpriteBank.SpriteData[sprite].Sprite.Animations.Keys.ToList();
                string anim_name = anims.Contains("idle") ? "idle" : anims.First();

                // add here and there
                string full_name = $"{sprite}:{anim_name}";
                customs[sprite] = full_name;
                Emote.addCustomEmote(full_name);
                customsForRemoval.Add(full_name);
            }

            content["Visible to Emotemod users"] = customs;

            fillWithContent(content, gradualFadeIn);
        }

        float get_new_shift()
        {
            // int row = cursor_at / 5 + even_ifier;

            float target;
            float new_shift;
            if (cursor_at < 5)
            {
                target = 0;
                new_shift = 0;
            }
            else
            {
                target = cells[cursor_at].Position.Y - Celeste.Celeste.TargetHeight / 2;
                new_shift = target + SpriteGridCell.default_size / 2 * SpriteGridCell.upscale;
            }
            return new_shift;
        }

        IEnumerator Refocus()
        {
            float old_shift = scroll_offset;

            float new_shift = get_new_shift();

            for (float d = 1f; d > 0f; d -= Engine.DeltaTime * 4)
            {
                scroll_offset = new_shift - (new_shift - old_shift) * Ease.CubeIn(d);

                yield return null;
            }
            scroll_offset = new_shift;
        }

        void Init()
        {
            customsForRemoval = new();
            cells = new();
            titles = new();
        }

        void Clear()
        {
            foreach (string e in customsForRemoval)
            {
                try
                {
                    Emote.madeline_bp.Remove(e);
                }
                catch { }
            }
            customsForRemoval.Clear();

            foreach (SpriteGridCell c in cells)
            {
                c.RemoveSelf();
            }
            cells.Clear();

            foreach (SpriteGridTitle t in titles)
            {
                t.RemoveSelf();
            }
            titles.Clear();
        }
        #endregion

        #region Constructors


        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
        /// <paramref name="emote"/> - the emote currently targeted
        /// this is for emote only
        /// </summary>
        public SpriteGrid(string spritebank, string emote, EmoteCard parent)
        {
        }
        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
        /// this is for spritebank + emote
        /// </summary>
        public SpriteGrid(string spritebank, EmoteCard parent)
        {
            Init();
            stage = Stages.SpriteBank;
            coro = SpriteBankEnter();
            Visible = true;
            this.parent = parent;
            Tag = parent.Tag;
            parent.parent.Scene.Add(this);
        }
        #endregion
    }
}
