KSP Toolbar Plugin
==================

This is a plugin for [Kerbal Space Program] that adds a common API for a buttons toolbar.
Third-party plugin authors may use the API to add their buttons to the toolbar.

For more information, please visit the [Forums Feedback Thread].

[Kerbal Space Program]: http://www.kerbalspaceprogram.com
[Forums Feedback Thread]: http://forum.kerbalspaceprogram.com/threads/60066

=================================================

The UnBlur dependency has been removed

CKAN has been updated with this information

 

Dependencies

Click Through Blocker
 

With @blizzy78's blessing (request, actually), I'm continuing support for this mod.  Original thread is here: http://forum.kerbalspaceprogram.com/index.php?/topic/55420-120-toolbar-1713-common-api-for-draggableresizable-buttons-toolbar/

Hi, there are quite a few plugins now that add various GUI buttons to the game. Right now, every plugin author has to implement button behaviour themselves. Also, there is no consistent button style between different plugins' buttons.

I have written a plugin that is targeted towards those plugins. Specifically, it provides an API for third-party plugins to provide GUI buttons to a toolbar. Those buttons then can invoke an arbitrary action specified by the plugin's author, such as opening a new window. The ultimate goal of this plugin is the separation of specifying a button's contents (such as text or image) and its action from the actual display style and position of the button. The player should be free to position buttons anywhere they please.

Availability

Download: Download Toolbar Plugin
Source code: https://github.com/linuxgurugamer/ksp_toolbar
License:  BSD 2-clause license.
There is now a dependency to the ClickThroughBlocker (Minimum version of the ClickThroughBlocker needs to be 0.1.7)

Older video demonstrating some of the features:
https://youtu.be/5yRh8WwYKkY
 

 


Mod spotlight from HAWX Gaming:

 https://youtu.be/QPvMC4EXPbs

 


Feature List

 

A draggable and resizable toolbar holds all buttons
Buttons have a texture (icon)
Custom button order can be maintained
Unlimited number of toolbars per game scene
Custom button folders can be created to improve organization
Toolbar position is saved between KSP restarts
Toolbar will auto-clamp to screen area so that it cannot be dragged off-screen
Toolbar may auto-hide itself if positioned at screen edge
Clicking a button invokes an arbitrary action specified by plugin author
Every button looks the same, no need for plugin author to provide any styles
 

Please note that this is not a regular plugin in the usual sense. You can install it, but it won't actually do anything on its own unless being told to do so by other plugins.

However, if you're a player and would like your favorite plugin to use the Toolbar Plugin, please tell that plugin's author to look into it.

Are you a plugin author and would like to use the Toolbar Plugin? Please head over to the development thread which has all the info you need.

Frequently Asked Questions

Your download link is dead!?

It is not. It works just fine. Clear your browser's cache and try again.

How do I reposition the toolbar on my screen?

Click on the little triangle button on the toolbar, then select "Unlock Position and Size" from the menu. You can now drag the toolbar around. When done, click on the triangle button again, then select "Lock Position and Size."

How do I change the layout of the toolbar? I want there to be two rows of buttons, or two columns, or ...

Click on the little triangle button on the toolbar, then select "Unlock Position and Size" from the menu. You can now resize the toolbar by clicking and dragging in the lower-right corner of it. When done, click on the triangle button again, then select "Lock Position and Size."

How do I change the order of the buttons?

Click on the little triangle button on the toolbar, then select "Unlock Button Order" from the menu. You can now drag the buttons around. When done, click on the triangle button again, then select "Lock Button Order."

Does the toolbar remember position, layout, button order, and button visibility dependent on current game scene?

Yes, all those settings are saved dependent on the current game scene.

Is there a way to automatically hide the toolbar when I don't need it?

First, drag the toolbar to the screen edge. Then, click the triangle button and select "Activate Auto-Hide at Screen Edge" from the menu. The toolbar should now auto-hide itself when the mouse pointer is not hovering above it. To disable this behaviour, click the triangle button again, then select "Deactivate Auto-Hide at Screen Edge."

I've installed a new plugin, how do I make its buttons visible?

Click on the little triangle button on the toolbar, then select "Configure Visible Buttons" from the menu. Activate or deactivate buttons as desired. Click "Close" to close the configuration window.

How do I create a new button folder? How do I put buttons into a folder?

Click the triangle button, then select "Create New Folder" from the menu.

To put buttons into the folder, click the triangle button, then select "Unlock Button Order" from the menu. Then drag any regular button onto the folder button (not into the folder, should it be visible.) To exit the button drag mode, click the triangle button, then select "Lock Button Order."

How do I delete a button folder?

Right-click on the button folder, then select "Delete Folder" from the menu.

Folders are not enough, I need more toolbars. How do I add them?

Click the triangle button, then select "Create New Toolbar" to add a new toolbar.

How do I delete a toolbar?

Click the triangle button on the toolbar in question, then select "Delete Toolbar". Note that you cannot delete the last toolbar.

How do I change a folder button's tool tip text?

Right-click on the button folder, then select "Edit Folder Settings" from the menu.

I like the KSP GUI skin better than Unity's default, is there a way to make the buttons look like that?

Click the triangle button, then select "Use KSP Skin" from the menu. To revert back to Unity's default skin, click the triangle button again, then select "Use Unity 'Smoke' Skin."

Is there a way to hide the toolbar's border?

Click the triangle button, then select "Hide Border" from the menu. To show the border again, click the triangle button again, then select "Show Border."

My toolbar is all messed up, how do I reset it completely?

To reset to defaults completely, exit KSP, delete the GameData\toolbar-settings.dat file from your installation, then start KSP again.