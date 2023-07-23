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

        /// <summary>
        /// Length of a DDS file's image header.
        /// </summary>
        const int DDS_HEADER_SIZE = 128;

        /// <summary>
        /// Loads an image from the specified filepath.
        /// </summary>
        /// <remarks>If the specified path doesn't exist, this function will fail after printing a log message.</remarks>
        /// <param name="texture">Ref of a <see cref="Texture2D"/> object to store the loaded image in.</param>
        /// <param name="path">The path to the target image file, including extension.</param>
        /// <returns><list type="table">
        /// <item><term>true</term><description>Successfully loaded the image file specified by <paramref name="path"/>, and <paramref name="texture"/> now contains its contents.</description></item>
        /// <item><term>false</term><description>The image file specified by <paramref name="path"/> doesn't exist, and <paramref name="texture"/> wasn't modified.</description></item>
        /// </list></returns>
        static bool LoadImageFromFile(ref Texture2D texture, string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Log.error($"! You're missing a required image file:  '{path}'");
                Log.error("^ To fix this issue, reinstall the mod specified by the above path.");
                return false; //< short circuit
            }

            if (path.EndsWith("dds", StringComparison.OrdinalIgnoreCase))
            {   // Handle DDS image formats:
                byte[] bytes = System.IO.File.ReadAllBytes(path);
                TextureFormat texFmt = TextureFormat.Alpha8;

                // Validate the DDS header
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(new System.IO.MemoryStream(bytes)))
                {
                    // DDS FORMAT SRC: https://docs.microsoft.com/en-us/windows/win32/direct3ddds/dx-graphics-dds-pguide#common-dds-file-resource-formats-and-associated-header-content
                    uint magic = br.ReadUInt32();
                    if (magic != DDSValues.uintMagic) // (542327876u)
                    {
                        Log.error($"! Invalid DDS Image:  '{path}'  (Magic number is '{magic}', expected '{DDSValues.uintMagic}')");
                        Log.error("^ To fix this issue, reinstall the mod specified by the above path, and/or contact the mod author.");
                        return false;
                    }

                    switch (new DDSHeader(br).ddspf.dwFourCC)
                    {
                    case 827611204u: //< "DXT1" as an unsigned integral
                        texFmt = TextureFormat.DXT1;
                        break;
                    case 894720068u: //< "DXT5" as an unsigned integral
                        texFmt = TextureFormat.DXT5;
                        break;
                    default:
                        Log.error($"! Invalid DDS Image:  '{path}'  (Invalid File Header)");
                        Log.error("^ To fix this issue, reinstall the mod specified by the above path, and/or contact the mod author.");
                        return false;
                    }

                    if (bytes[4] != 124)
                    {
                        Log.error($"! Invalid DDS Image:  '{path}'  (Byte[4] is '{bytes[4]}', expected '124')");
                        Log.error("^ To fix this issue, reinstall the mod specified by the above path, and/or contact the mod author.");
                        return false;
                    }
                } // if we reached this point, the DDS image is probably valid.

                try
                {
                    // calculate the image dimensions
                    int height = bytes[13] * 256 + bytes[12];
                    int width = bytes[17] * 256 + bytes[16];

                    // retrieve the image bytes
                    byte[] dxtBytes = new byte[bytes.Length - DDS_HEADER_SIZE];
                    Buffer.BlockCopy(bytes, DDS_HEADER_SIZE, dxtBytes, 0, bytes.Length - DDS_HEADER_SIZE);

                    // load the texture
                    texture = new Texture2D(width, height, texFmt, false);
                    texture.LoadRawTextureData(dxtBytes);
                    texture.Apply();
                }
                catch (Exception ex)
                {
                    Log.error($"! Invalid DDS Image:  '{path}'  (Exception was thrown during read operation: '{ex.Message}')");
                    Log.error("^ To fix this issue, reinstall the mod specified by the above path, and/or contact the mod author.");
                }
            }
            else texture.LoadImage(System.IO.File.ReadAllBytes(path));

            return true;
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
            return System.IO.File.Exists(fileNamePath); //< we aren't modifying the texture path here, so no need to resolve the path - see TexPathname
        }

        static String rootPath;
        static public String RootPath { get { return rootPath; } }


        internal static void InitRootPath()
        {
            string s = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            rootPath = s.Substring(0, s.IndexOf("GameData"));
        }

        static readonly string[] recognizedImgFileExtensions = new string[] { "png", "jpg", "gif", "dds" };

        /// <summary>
        /// Attempts to resolve the given filepath by searching for files with similar extensions.
        /// </summary>
        /// <param name="fullPath">The input filepath to resolve. If the file specified by the path already exists, it is returned unmodified; Case-sensitive filesystem handling is subject to the implementation of <see cref="System.IO.File.Exists(string)"/>.
        /// <br/>The immediate parent directory of this file is the root directory during the search.</param>
        /// <param name="sCompareType">The <see cref="StringComparison"/> type to use when matching filenames.</param>
        /// <param name="fSearchType">Determines the search method used when calling <see cref="System.IO.Directory.EnumerateFiles(string, string, SearchOption)"/></param>
        /// <returns>If the search was successful, this is the resolved path. If the search was unsuccessful, this is <paramref name="fullPath"/>.</returns>
        internal static string ResolveTexPathname(string fullPath, StringComparison sCompareType = StringComparison.OrdinalIgnoreCase, SearchOption fSearchType = SearchOption.TopDirectoryOnly)
        {
            if (fullPath.Length < 1 || System.IO.File.Exists(fullPath))
                return fullPath; //< short circuit

            int sepPos;
            string ext;
            for (var enumerator = System.IO.Directory.EnumerateFiles(Path.GetDirectoryName(fullPath), Path.GetFileNameWithoutExtension(fullPath) + ".*", fSearchType).GetEnumerator(); enumerator.MoveNext();)
            {
                sepPos = enumerator.Current.LastIndexOf('.');
                ext = sepPos == -1 ? "" : enumerator.Current.Substring(sepPos + 1);
                if (ext.Length > 0 && recognizedImgFileExtensions.Any(recognizedExt => recognizedExt.Equals(ext, sCompareType)))
                {
                    return enumerator.Current;
                }
            }

            Log.warn($"[! MISSING FILE !]:  \"{fullPath}\"");
            return fullPath;
        }

        internal static string TexPathname(string path, bool resolvePath = true)
        {
            //Debug.Log("TexPathname, GetExecutingAssembly: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            //Debug.Log("TexPathname, ApplicationRootPath: " + KSPUtil.ApplicationRootPath);
            //return  KSPUtil.ApplicationRootPath + "GameData/" + path;

            Log.info("TexPathname, path: " + path);
            if (path == "")
                Log.info("TexPathname StackTrace: " + Environment.StackTrace);

            string fullpath = RootPath + "GameData/" + path;

            if (resolvePath) // fallback to checking each extension type
                return ResolveTexPathname(fullpath, StringComparison.OrdinalIgnoreCase, SearchOption.TopDirectoryOnly);

            return fullpath;
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
