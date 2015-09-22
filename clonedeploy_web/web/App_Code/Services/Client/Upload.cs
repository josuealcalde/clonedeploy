﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web;
using Global;
using Models;
using Newtonsoft.Json;
using Partition;

namespace Services.Client
{
    public class Upload
    {
        public string AddNewImageSpecs(string imagename, string imagesize)
        {
            /*
            var image = new Image {Name = imagename};
            image.Read();
            image.ClientSize = imagesize;
            image.Update();
           
          
            ImagePhysicalSpecs existingips;
            try
            {
                existingips = JsonConvert.DeserializeObject<ImagePhysicalSpecs>(image.ClientSize);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                return "false";
            }
            if (existingips.Hd == null)
            {
                return "false";
            }
            //Reset Custom Specs
            image.ClientSizeCustom = "";
            image.Update();
            return "true";
             * */
            return "fixme";
        }

        public string CreateDirectory(string imageName, string dirName)
        {
            /*
            var image = new Image {Name = imageName};
            image.Read();
            if (image.Protected != 1)
            {
                try
                {
                    if (Settings.ImageTransferMode == "udp+http")
                        Directory.CreateDirectory(Settings.ImageStorePath + imageName +
                                                  Path.DirectorySeparatorChar + "hd" + dirName);
                    else
                        Directory.CreateDirectory(Settings.ImageHoldPath + imageName +
                                                  Path.DirectorySeparatorChar + "hd" + dirName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return "Could Not Create Directory";
                }
                return "true";
            }
            return "Image Is Protected";
             * */
            return "fixme";
        }

        public string DeleteImage(string imageName)
        {
            /*
            var image = new Image {Name = imageName};
            image.Read();
            if (image.Protected != 1)
            {
                try
                {
                    if (Directory.Exists(Settings.ImageStorePath + imageName))
                        new FileOps().DeleteAllFiles(Settings.ImageStorePath + imageName);

                    if (Directory.Exists(Settings.ImageHoldPath + imageName))
                        new FileOps().DeleteAllFiles(Settings.ImageHoldPath + imageName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return "Could Not Delete Existing Image";
                }

                try
                {
                    if (Settings.ImageTransferMode == "udp+http")
                        Directory.CreateDirectory(Settings.ImageStorePath + imageName);
                    else
                        Directory.CreateDirectory(Settings.ImageHoldPath + imageName);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return "Could Not Delete Existing Image";
                }
            }
            else
                return "Image Is Protected";
            */
            return "true";
        }

        public string StartReceiver(string imagePath, string port)
        {
            var compAlgorithm = Settings.CompressionAlgorithm;
            imagePath = imagePath.Replace('/', Path.DirectorySeparatorChar);
            var compExt = compAlgorithm == "lz4" ? ".lz4" : ".gz";

            string shell;

            var appPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" +
                          Path.DirectorySeparatorChar + "apps" + Path.DirectorySeparatorChar;
            var logPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" +
                          Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar;
            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                string dist = null;
                var distInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = "uname",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(distInfo))
                {
                    if (process != null) dist = process.StandardOutput.ReadToEnd();
                }

                shell = dist != null && dist.ToLower().Contains("bsd") ? "/bin/csh" : "/bin/bash";
            }
            else
            {
                shell = "cmd.exe";
            }

            Process receiver = null;
            var receiverInfo = new ProcessStartInfo {FileName = (shell)};

            if (Environment.OSVersion.ToString().Contains("Unix"))
            {
                receiverInfo.Arguments = (" -c \"" + "udp-receiver" + " --portbase " + port + " " +
                                          Settings.ReceiverArgs + " --start-timeout 30 | " +
                                          " " + compAlgorithm + " -" + Settings.CompressionLevel +
                                          " > " + Settings.ImageStorePath + imagePath + compExt +
                                          "\"");
            }
            else
            {
                receiverInfo.Arguments = (" /c " + appPath + "udp-receiver.exe" + " --portbase " + port + " " +
                                          Settings.ReceiverArgs + " --start-timeout 30 | " +
                                          appPath + compAlgorithm + " -" + Settings.CompressionLevel +
                                          " > " + Settings.ImageStorePath + imagePath + compExt);
            }

            var log = ("\r\n" + DateTime.Now.ToString("MM.dd.yy hh:mm") + " Starting Unicast Upload " + imagePath +
                       " With The Following Command:\r\n\r\n" + receiverInfo.FileName + receiverInfo.Arguments +
                       "\r\n\r\n");
            File.AppendAllText(logPath + "unicast.log", log);

            var result = "true";
            try
            {
                receiver = Process.Start(receiverInfo);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
                result = "Could Not Start Upload .  Check The Exception Log For More Info";
                File.AppendAllText(logPath + "unicast.log",
                    "Could Not Start Session " + imagePath + "Try Pasting The Command Into A Command Prompt");
            }

            Thread.Sleep(2000);

            if (receiver != null && receiver.HasExited)
            {
                result = "Could Not Start Upload .  Check The Exception Log For More Info";
                File.AppendAllText(logPath + "multicast.log",
                    "Session " + imagePath +
                    @" Started And Was Forced To Quit, Try Pasting The Command Into A Command Prompt");
            }
            return result;
        }

        public string UploadFile(string fileName, string imagePath, string fileType, HttpFileCollection files)
        {
            try
            {
                string fullPath;

                switch (fileType)
                {
                    case "mbr":
                        imagePath = imagePath.Replace('/', Path.DirectorySeparatorChar);
                        if (Settings.ImageTransferMode == "udp+http")
                            fullPath = Settings.ImageStorePath + imagePath + Path.DirectorySeparatorChar +
                                       fileName;
                        else
                            fullPath = Settings.ImageHoldPath + imagePath + Path.DirectorySeparatorChar +
                                       fileName;
                        break;
                    case "log":
                        if (imagePath == "host")
                            fullPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" +
                                       Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + "hosts" +
                                       Path.DirectorySeparatorChar + fileName;
                        else
                            fullPath = HttpContext.Current.Server.MapPath("~") + Path.DirectorySeparatorChar + "data" +
                                       Path.DirectorySeparatorChar + "logs" + Path.DirectorySeparatorChar + fileName;
                        break;
                    default:
                        return "File Type Not Specified";
                }

                if (files.Count == 1 && files[0].ContentLength > 0 && !string.IsNullOrEmpty(fileName))
                {
                    var binaryWriteArray = new
                        byte[files[0].InputStream.Length];
                    files[0].InputStream.Read(binaryWriteArray, 0, (int) files[0].InputStream.Length);
                    var objfilestream = new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite);
                    objfilestream.Write(binaryWriteArray, 0, binaryWriteArray.Length);
                    objfilestream.Close();
                    if (File.Exists(fullPath))
                    {
                        var file = new FileInfo(fullPath);
                        return file.Length > 0 ? "true" : "File Has No Size";
                    }
                    return "File Could Not Be Created";
                }
                return "File Was Not Posted Successfully";
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return "Check The Exception Log For More Info";
            }
        }
    }
}