﻿using BloonTowerMaker.Data;
using BloonTowerMaker.Properties;
using System.Drawing;
using System.IO;

namespace BloonTowerMaker.Logic
{
    class SelectImage
    {

        //Load Image from name (Resource folder)
        public static Image LoadImage(string name)
        {
            //Skip no image
            if (string.IsNullOrWhiteSpace(name)) return null;

            //Add missing .png
            if (!name.Contains(".png")) name += ".png";

            //Get image file path
            var imagePath = Path.Combine(Project.instance.projectPath, Resources.ProjectResourcesFolder,name);

            //Check if the image exist
            if (!File.Exists(imagePath)) return null;//throw new Exception($"Image {name} not found!");


            //Read image
            try
            {
                return Image.FromFile(imagePath);
            }
            catch
            {
                return null;
            }
        }

        public static void SaveImage(string path)
        {
            var fileName = Path.GetFileName(path);
            try
            {
                File.Copy(path, Path.Combine(Project.instance.projectPath, Resources.ProjectResourcesFolder, fileName));
            }
            catch
            {
                //throw new Exception("Cant copy image from path " + path);
                //Image already in folder
            }
        }
    }
}
