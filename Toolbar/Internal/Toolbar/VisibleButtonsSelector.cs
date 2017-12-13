/*
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace Toolbar {
	internal class VisibleButtonsSelector : AbstractWindow {
		internal event Action OnButtonSelectionChanged;

		private HashSet<string> visibleButtonIds;
		private List<Button> buttons;
		private Vector2 scrollPos;

		internal VisibleButtonsSelector(HashSet<string> visibleButtonIds) : base() {
			this.visibleButtonIds = visibleButtonIds;

			Rect = new Rect(300, 300, 0, 0);
			// dictionary.cfg
            //
            // #TOOLBAR_UI_CONFIG_VISIBLE_TITLE = "Toolbar Button Visibility"
            // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON = "Configure which buttons should be visible in the current game scene."
            // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON_NOTE = "Note: Plugins may still decide to hide buttons from any game scene even if those buttons are active here."
            // #TOOLBAR_UI_CLOSE = "Close"
            //
            // eg : Localizer.Format("#ID")
            //
			Title = Localizer.Format("#TOOLBAR_UI_CONFIG_VISIBLE_TITLE");
			Dialog = true;

			List<Command> commands = new List<Command>(ToolbarManager.InternalInstance.Commands.Where(c => !c.IsInternal));
			commands.Sort((c1, c2) => c1.CompareTo(c2));
			buttons = new List<Button>();
			foreach (Command command in commands) {
				buttons.Add(new Button(command));
			}
		}

		internal override void drawContents() {
			GUILayout.BeginVertical();
                // dictionary.cfg
                //
                // #TOOLBAR_UI_CONFIG_VISIBLE_TITLE = "Toolbar Button Visibility"
                // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON = "Configure which buttons should be visible in the current game scene."
                // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON_NOTE = "Note: Plugins may still decide to hide buttons from any game scene even if those buttons are active here."
                // #TOOLBAR_UI_CLOSE = "Close"
                //
                // eg : Localizer.Format("#ID")
                //
				GUILayout.Label(Localizer.Format("#TOOLBAR_UI_CONFIG_VISIBLE_BUTTON"));
				GUILayout.Label(Localizer.Format("#TOOLBAR_UI_CONFIG_VISIBLE_BUTTON_NOTE"));

				GUILayout.Space(5);

				scrollPos = GUILayout.BeginScrollView(scrollPos,
					GUILayout.Width(Mathf.Max(Screen.width / 4, 350)), GUILayout.Height(Mathf.Max(Screen.height / 3, 350)));

				GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
				labelStyle.wordWrap = false;

				string lastNamespace = buttons.First().Namespace;
				foreach (Button button in buttons) {
					if (button.Namespace != lastNamespace) {
						Separator.Instance.drawMenuOption();
					}

					GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
					string id = button.FullId;
						bool visible = visibleButtonIds.Contains(id);
						bool selected = GUILayout.Toggle(visible, (string) null);
						if (selected != visible) {
							if (selected) {
								visibleButtonIds.Add(id);
							} else {
								visibleButtonIds.Remove(id);
							}
							fireButtonSelectionChanged();
						}
						button.drawPlain();
						GUILayout.Label(button.command.Text ?? button.command.ToolTip, labelStyle);
						GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();

					lastNamespace = button.Namespace;
				}

				GUILayout.EndScrollView();

				GUILayout.Space(15);

				GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					// dictionary.cfg
                    //
                    // #TOOLBAR_UI_CONFIG_VISIBLE_TITLE = "Toolbar Button Visibility"
                    // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON = "Configure which buttons should be visible in the current game scene."
                    // #TOOLBAR_UI_CONFIG_VISIBLE_BUTTON_NOTE = "Note: Plugins may still decide to hide buttons from any game scene even if those buttons are active here."
                    // #TOOLBAR_UI_CLOSE = "Close"
                    //
                    // eg : Localizer.Format("#ID")
                    //
					if (GUILayout.Button(Localizer.Format("#TOOLBAR_UI_CLOSE"))) {
						destroy();
					}
				GUILayout.EndHorizontal();

			GUILayout.EndVertical();
		}

		private void fireButtonSelectionChanged() {
			if (OnButtonSelectionChanged != null) {
				OnButtonSelectionChanged();
			}
		}
	}
}
