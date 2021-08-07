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
using System.IO;

using System.Reflection;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DDSHeaders;


namespace Toolbar
{
    internal static class Utils
    {
        internal static Vector2 getMousePosition()
        {
            Vector3 mousePos = Input.mousePosition;
            return new Vector2(mousePos.x, Screen.height - mousePos.y).clampToScreen();
        }

        internal static bool isPauseMenuOpen()
        {
            // PauseMenu.isOpen may throw NullReferenceException on occasion, even if HighLogic.LoadedScene==GameScenes.FLIGHT
            try
            {
                return (HighLogic.LoadedScene == GameScenes.FLIGHT) && PauseMenu.isOpen;
            }
            catch
            {
                return false;
            }
        }


        //
        // The following function was initially copied from @JPLRepo's AmpYear mod, which is covered by the GPL, as is this mod
        //
        // This function will attempt to load either a PNG or a JPG from the specified path.  
        // It first checks to see if the actual file is there, if not, it then looks for either a PNG or a JPG
        //
        // easier to specify different cases than to change case to lower.  This will fail on MacOS and Linux
        // if a suffix has mixed case
        static string[] imgSuffixes = new string[] { ".png", ".jpg", ".gif", ".PNG", ".JPG", ".GIF", ".dds", ".DDS" };
        static Boolean LoadImageFromFile(ref Texture2D tex, String fileNamePath)
        {

            Boolean blnReturn = false;
            bool isDDS = false;
            try
            {
                string path = fileNamePath;
                if (!System.IO.File.Exists(fileNamePath))
                {
                    // Look for the file with an appended suffix.
                    for (int i = 0; i < imgSuffixes.Length; i++)

                        if (System.IO.File.Exists(fileNamePath + imgSuffixes[i]))
                        {
                            path = fileNamePath + imgSuffixes[i];
                            isDDS = imgSuffixes[i] == ".dds" || imgSuffixes[i] == ".DDS";
                            break;
                        }
                }

                //File Exists check
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        if (isDDS)
                        {
                            byte[] bytes = System.IO.File.ReadAllBytes(path);

                            BinaryReader binaryReader = new BinaryReader(new MemoryStream(bytes));
                            uint num = binaryReader.ReadUInt32();

                            if (num != DDSValues.uintMagic)
                            {
                                UnityEngine.Debug.LogError("DDS: File is not a DDS format file!");
                                return false;
                            }
                            DDSHeader ddSHeader = new DDSHeader(binaryReader);

                            TextureFormat tf = TextureFormat.Alpha8;
                            if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT1)
                                tf = TextureFormat.DXT1;
                            if (ddSHeader.ddspf.dwFourCC == DDSValues.uintDXT5)
                                tf = TextureFormat.DXT5;
                            if (tf == TextureFormat.Alpha8)
                                return false;


                            tex = LoadTextureDXT(bytes, tf);
                        }
                        else
                            tex.LoadImage(System.IO.File.ReadAllBytes(path));
                        blnReturn = true;
                    }
                    catch (Exception ex)
                    {
                        Log.error("Failed to load the texture:" + path);
                        Log.error(ex.Message);
                    }
                }
                else
                {
                    Log.error("Cannot find texture to load:" + fileNamePath);
                }
            }
            catch (Exception ex)
            {
                Log.error("Failed to load (are you missing a file):" + fileNamePath);
                Log.error(ex.Message);
            }
            return blnReturn;
        }
        public static Texture2D LoadTextureDXT(byte[] ddsBytes, TextureFormat textureFormat)
        {
            if (textureFormat != TextureFormat.DXT1 && textureFormat != TextureFormat.DXT5)
                throw new Exception("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
                throw new Exception("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            Texture2D texture = new Texture2D(width, height, textureFormat, false);
            texture.LoadRawTextureData(dxtBytes);
            texture.Apply();

            return (texture);
        }

        internal static Texture2D GetTexture(string texturePath)
        {
            Texture2D tmptexture = null;
            string filePath = TexPathname(texturePath);
            if (!Utils.TextureFileExists(filePath))
            {
                //Debug.Log("GetTexture, filePath: [" + filePath + "] not found, trying game database");
                if (GameDatabase.Instance.ExistsTexture(texturePath))
                {
                    tmptexture = GameDatabase.Instance.GetTexture(texturePath, false);
                    if (tmptexture == null)
                        Debug.Log("GetTexture, tmptexture is null after checking GameDatabase: [" + texturePath + "]");
                }
                else
                    Log.info("GetTexture, texture not found in GameDatabase: [" + texturePath + "]");
            }
            else
            {
                tmptexture = GetTextureFromFile(texturePath, false);

                if (tmptexture == null)
                    Log.info("GetTexture, texture not found after check for file, texturePath: " + texturePath);
            }
            return tmptexture;
        }

        internal static bool TextureFileExists(string fileNamePath)
        {
            if (!System.IO.File.Exists(fileNamePath))
            {
                // Look for the file with an appended suffix.
                for (int i = 0; i < imgSuffixes.Length; i++)

                    if (System.IO.File.Exists(fileNamePath + imgSuffixes[i]))
                        return true;

            }
            return false;
        }

        static String rootPath;
        static public String RootPath {  get { return rootPath; } }


        internal static void InitRootPath()
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            rootPath = s.Substring(0, s.IndexOf("GameData"));
        }


        internal static string TexPathname(string path)
        {
            //Debug.Log("TexPathname, GetExecutingAssembly: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            //Debug.Log("TexPathname, ApplicationRootPath: " + KSPUtil.ApplicationRootPath);
            //return  KSPUtil.ApplicationRootPath + "GameData/" + path;

            return RootPath + "GameData/" + path;
        }

        internal static Texture2D GetTextureFromFile(string path, bool b)
        {

            Texture2D tex = new Texture2D(16, 16, TextureFormat.ARGB32, false);

            if (LoadImageFromFile(ref tex, TexPathname(path)))
                return tex;
            Log.error("GetTextureFromFile, error loading: " + TexPathname(path));
            return null;
        }
    }
}
