﻿/*
Copyright (c) 2013-2016, Maik Schreiber
All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Toolbar
{
    internal class Button : IPopupMenuOption
    {
        private static readonly Vector2 UNSIZED = new Vector2(float.NaN, float.NaN);
        private const string TEXTURE_PATH_DROPDOWN = "000_Toolbar/toolbar-dropdown";
        private const int DROPDOWN_TEX_WIDTH = 10;
        private const int DROPDOWN_TEX_HEIGHT = 7;
        private const int PADDING = 4;

        internal static int MAX_TEX_WIDTH = 57;
        internal static int MAX_TEX_HEIGHT = 57;

        internal const int MAX_SMALL_TEX_WIDTH = 24;
        internal const int MAX_SMALL_TEX_HEIGHT = 24;

        internal string Namespace
        {
            get
            {
                return command.Namespace;
            }
        }

        internal string FullId
        {
            get
            {
                return command.FullId;
            }
        }

        internal bool IsInternal
        {
            get
            {
                return command.IsInternal;
            }
        }

        private Vector2? size_ = null;
        private int oldSavedScale = 24;
        internal Vector2 Size
        {
            get
            {
                if (size_ == null || (toolbar != null && oldSavedScale != toolbar.savedScale))
                {
                    if (toolbar != null)
                    {
                        oldSavedScale = toolbar.savedScale;
                        toolbar.scaleChanged = true;
                    }
                    if (toolbarDropdown)
                    {
                        size_ = new Vector2(DROPDOWN_TEX_WIDTH, DROPDOWN_TEX_HEIGHT);
                    }
                    else if (command.IsTextured)
                    {
                        if (toolbar != null)
                            size_ = new Vector2(toolbar.adjustedSavedScale + PADDING * 2, toolbar.adjustedSavedScale + PADDING * 2);
                        else
                            size_ = new Vector2(oldSavedScale, oldSavedScale);
                    }
                    else
                    {
                        var s = Style;
                        s.fontSize = Style.fontSize * oldSavedScale / 24;
                        var tmpsize_ = s.CalcSize(Content);
                        tmpsize_.x += s.padding.left + s.padding.right;
                        tmpsize_.y += s.padding.top + s.padding.bottom;
                        size_ = tmpsize_;
                    }
                }
                if (size_ != null)
                    return (Vector2)size_;
                else
                    return UNSIZED;
            }
        }

        private GUIContent content_;
        private GUIContent Content
        {
            get
            {
                if (content_ == null)
                {
                    content_ = command.IsTextured ? new GUIContent(Texture) : new GUIContent(command.Text ?? "???");
                }
                return content_;
            }
        }

        private Texture2D texture_;
        private Texture2D Texture
        {
            get
            {
                if ((texture_ == null) && (command.TexturePath != null || command.BigTexturePath != null))
                {
                    try
                    {
                        Texture2D tmptexture_ = null;

                        if (command.BigTexturePath != null)
                        {
                            string t = null;
                            if (command.TexturePath != null &&
                                ((toolbar != null && toolbar.adjustedSavedScale == 24) ||
                                toolbar == null))
                                t = command.TexturePath;
                            else
                            {
                                if (command.BigTexturePath != null)
                                    t = command.BigTexturePath;
                                else if (command.TexturePath != null)
                                    t = command.TexturePath;
                            }
                            //Debug.Log("Texture, t: [" + t + "], toolbar.adjustedSavedScale: " + toolbar.adjustedSavedScale);
                            //Debug.Log("command.BigTexturePath: " + command.BigTexturePath);
                            //Debug.Log("command.TexturePath: " + command.TexturePath);
                            tmptexture_ = Utils.GetTexture(t);
                        }
                        else
                        {
                            Log.info("Texture, command.TexturePath: [" + command.TexturePath + "]");
                            tmptexture_ = Utils.GetTexture(command.TexturePath);
                        }
                        if (tmptexture_ != null)
                        {
                            if ((tmptexture_.width > MAX_TEX_WIDTH) || (tmptexture_.height > MAX_TEX_HEIGHT))
                            {
                                Log.error("button texture exceeds {0}x{1} pixels, ignoring texture: {2}", MAX_TEX_WIDTH, MAX_TEX_HEIGHT, command.FullId);
                                tmptexture_ = BrokenButtonTexture;
                            }

                            if (toolbar != null)
                            {
                                // Make a copy here so we don't change what's in the game database
                                texture_ = UnityEngine.Object.Instantiate(tmptexture_) as Texture2D;
     
                                if (texture_ == null)
                                {
                                    Log.error("texture_ is null, unable to instantiate copy");
                                }
                                if (texture_.format == TextureFormat.DXT5)
                                {
                                    Texture2D newTexture2DInARGB32 = new Texture2D(texture_.width, texture_.height, TextureFormat.ARGB32, false);
                                    newTexture2DInARGB32.SetPixels(texture_.GetPixels());
                                    newTexture2DInARGB32.Apply();
                                    texture_ = newTexture2DInARGB32;
                                }
                                if (texture_.format == TextureFormat.DXT1)
                                {
                                    Texture2D newTexture2DInARGB32 = new Texture2D(texture_.width, texture_.height, TextureFormat.RGB24, false);
                                    newTexture2DInARGB32.SetPixels(texture_.GetPixels());
                                    newTexture2DInARGB32.Apply();
                                    texture_ = newTexture2DInARGB32;
                                }
                                LocalTextureScale.Point(texture_, (int)toolbar.adjustedSavedScale, (int)toolbar.adjustedSavedScale);
                            }
                            else
                            {
                                texture_ = tmptexture_;
                            }

                        }
                        else
                        {
                            Log.error("button texture not found: {0}", command.TexturePath);
                            Log.error("Current Dir: " + System.IO.Directory.GetCurrentDirectory());
                            string filePath = Utils.TexPathname(command.TexturePath);
                            string dir = Path.GetDirectoryName(filePath);
                            Log.error("dir: " + dir);
                            Log.error("filePath: " + filePath);
                            
                            var files = System.IO.Directory.GetFiles(dir);
                            foreach ( var file in files ) 
                            {
                                Log.error("file: " + file);
                            }


                            texture_ = BrokenButtonTexture;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.error(e, "error while loading button texture: {0}", command.TexturePath);
                        texture_ = BrokenButtonTexture;
                    }
                }
                return texture_;
            }
        }

        private GUIStyle style_;
        private GUIStyle Style
        {
            get
            {
                if (style_ == null)
                {
                    style_ = new GUIStyle(toolbarDropdown ? GUIStyle.none : GUI.skin.button);
                    style_.alignment = TextAnchor.MiddleCenter;
                    style_.normal.textColor = command.TextColor;
                    style_.onHover.textColor = command.TextColor;
                    style_.hover.textColor = command.TextColor;
                    style_.onActive.textColor = command.TextColor;
                    style_.active.textColor = command.TextColor;
                    style_.onFocused.textColor = command.TextColor;
                    style_.focused.textColor = command.TextColor;
                    if (command.IsTextured)
                    {
                        style_.padding = new RectOffset(0, 0, 0, 0);
                        style_.margin = new RectOffset(1, 1, 1, 1);
                    }
                }
                return style_;
            }

            set
            {
                style_ = value;
            }
        }

        private GUIStyle tooltipStyle_;
        private GUIStyle TooltipStyle
        {
            get
            {
                if (tooltipStyle_ == null)
                {
                    tooltipStyle_ = new GUIStyle(GUI.skin.box);
                    tooltipStyle_.wordWrap = false;
                }
                return tooltipStyle_;
            }
        }

        private GUIStyle menuOptionStyle_;
        private GUIStyle MenuOptionStyle
        {
            get
            {
                if (menuOptionStyle_ == null)
                {
                    Texture2D orangeBgTex = new Texture2D(1, 1);
                    orangeBgTex.SetPixel(0, 0, XKCDColors.DarkOrange);
                    orangeBgTex.Apply();

                    menuOptionStyle_ = new GUIStyle(GUI.skin.label);
                    menuOptionStyle_.hover.background = orangeBgTex;
                    menuOptionStyle_.hover.textColor = Color.white;
                    menuOptionStyle_.onHover.background = orangeBgTex;
                    menuOptionStyle_.onHover.textColor = Color.white;
                    menuOptionStyle_.wordWrap = false;
                    menuOptionStyle_.margin = new RectOffset(0, 0, 1, 1);
                    menuOptionStyle_.padding = new RectOffset(8, 8, 3, 3);
                }
                return menuOptionStyle_;
            }
        }

        public event ClickHandler OnClick
        {
            add
            {
                if (!destroyed)
                {
                    command.OnClick += value;
                }
            }
            remove
            {
                if (!destroyed)
                {
                    command.OnClick -= value;
                }
            }
        }

        private static Texture2D brokenButtonTexture_;
        private Texture2D BrokenButtonTexture
        {
            get
            {
                if (brokenButtonTexture_ == null)
                {
                    brokenButtonTexture_ = new Texture2D(MAX_TEX_WIDTH, MAX_TEX_HEIGHT);
                    Color[] colors = new Color[MAX_TEX_WIDTH * MAX_TEX_HEIGHT];
                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = XKCDColors.Purple;
                    }
                    brokenButtonTexture_.SetPixels(0, 0, MAX_TEX_WIDTH, MAX_TEX_HEIGHT, colors);
                    brokenButtonTexture_.Apply();
                }
                return brokenButtonTexture_;
            }
        }

        internal event Action OnMouseEnter;
        internal event Action OnMouseLeave;
        internal event DestroyHandler OnDestroy;

        internal readonly Command command;

        internal bool destroyed;

        private Toolbar toolbar;
        private bool toolbarDropdown;
        private bool showTooltip;

        internal Button(Command command, Toolbar toolbar = null)
        {
            this.command = command;
            this.toolbar = toolbar;

            OnMouseEnter += () =>
            {
                showTooltip = true;
                command.mouseEnter();
            };
            OnMouseLeave += () =>
            {
                showTooltip = false;
                command.mouseLeave();
            };

            command.OnChange += () => clearCaches();
            command.OnDestroy += () => Destroy();

            if (toolbar != null)
            {
                toolbar.OnSkinChange += clearCaches;
            }
        }

        private void clearCaches()
        {
            texture_ = null;
            content_ = null;
            style_ = null;
            size_ = null;
        }

        internal static Button createToolbarDropdown()
        {
            Command dropdownCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "dropdown");
            dropdownCommand.TexturePath = TEXTURE_PATH_DROPDOWN;
            Button button = new Button(dropdownCommand);
            button.toolbarDropdown = true;
            return button;
        }

        internal static Button createMenuOption(string text)
        {
            Command menuOptionCommand = new Command(ToolbarManager.NAMESPACE_INTERNAL, "menuOption");
            menuOptionCommand.Text = text;
            Button button = new Button(menuOptionCommand);
            return button;
        }

        internal void drawInToolbar(Rect rect, bool enabled)
        {
            if (!destroyed)
            {
                bool oldEnabled = GUI.enabled;
                GUI.enabled = enabled && command.Enabled;
                var s = Style;
                s.fontSize = Style.fontSize * oldSavedScale / 24;
                bool clicked = GUI.Button(rect, Content, s);

                GUI.enabled = oldEnabled;

                if (clicked)
                {
                    click();
                }
            }
        }

        internal void drawButton()
        {
            if (!destroyed)
            {
                //var s = Style;
                //s.fontSize = Style.fontSize * oldSavedScale / 24;
                if (GUILayout.Button(Content, Style, GUILayout.Width(Size.x), GUILayout.Height(Size.y)))
                {
                    click();
                }
            }
        }

        internal void drawPlain()
        {
            if (!destroyed)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                //style.fontSize = style.fontSize * oldSavedScale / 24;
                GUILayout.Label(Content, style, GUILayout.Width(Size.x), GUILayout.Height(Size.y));
            }
        }

        public void drawMenuOption()
        {
            if (!destroyed)
            {
                bool oldEnabled = GUI.enabled;
                GUI.enabled = command.Enabled;

                bool clicked = GUILayout.Button(command.Text, MenuOptionStyle, GUILayout.ExpandWidth(true));

                GUI.enabled = oldEnabled;

                if (clicked)
                {
                    click();
                }
            }
        }

        internal void drawToolTip()
        {
            if (!destroyed)
            {
                if (showTooltip && (command.ToolTip != null) && (command.ToolTip.Trim().Length > 0))
                {
                    Vector2 mousePos = Utils.getMousePosition();
                    Vector2 size = TooltipStyle.CalcSize(new GUIContent(command.ToolTip));
                    Rect rect = new Rect(mousePos.x, mousePos.y + 20, size.x, size.y);
                    float origY = rect.y;
                    rect = rect.clampToScreen();
                    // clamping moved the tooltip up -> reposition above mouse cursor
                    if (rect.y < origY)
                    {
                        rect.y = mousePos.y - size.y - 5;
                        rect = rect.clampToScreen();
                    }

                    int oldDepth = GUI.depth;
                    GUI.depth = -1000;
                    GUILayout.BeginArea(rect);
                    GUILayout.Label(command.ToolTip, TooltipStyle);
                    GUILayout.EndArea();
                    GUI.depth = oldDepth;
                }
            }
        }

        private void click()
        {
            command.click();
        }

        internal void mouseEnter()
        {
            if (!destroyed)
            {
                if (OnMouseEnter != null)
                {
                    OnMouseEnter();
                }
            }
        }

        internal void mouseLeave()
        {
            if (!destroyed)
            {
                if (OnMouseLeave != null)
                {
                    OnMouseLeave();
                }
            }
        }

        public void Destroy()
        {
            if (!destroyed)
            {
                destroyed = true;
                if (toolbar != null)
                {
                    toolbar.OnSkinChange -= clearCaches;
                }
                fireDestroy();
            }
        }

        private void fireDestroy()
        {
            if (OnDestroy != null)
            {
                OnDestroy(new DestroyEvent(this));
            }
        }
    }

}
