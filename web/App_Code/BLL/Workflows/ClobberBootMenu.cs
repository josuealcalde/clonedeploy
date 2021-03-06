﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Helpers;

namespace BLL.Workflows
{
    public class ClobberBootMenu
    {
        private readonly bool _promptComputerName;
        private readonly Models.ImageProfile _imageProfile;

        public ClobberBootMenu(Models.ImageProfile imageProfile, bool promptComputerName)
        {
            _promptComputerName = promptComputerName;
            _imageProfile = imageProfile;
        }

        public bool CreatePxeBootFiles()
        {
            var webPath = Settings.WebPath;
            var globalComputerArgs = Settings.GlobalComputerArgs;
            string namePromptArg = "";
            if (_promptComputerName)
                namePromptArg = " name_prompt=true";

            var userToken = Settings.ClobberRequiresLogin == "No" ? Settings.UniversalToken : "";
            const string newLineChar = "\n";


            var ipxe = new StringBuilder();
            ipxe.Append("#!ipxe" + newLineChar);
            ipxe.Append("kernel " + webPath + "IpxeBoot?filename=" + _imageProfile.Kernel +
                        "&type=kernel" + " initrd=" + _imageProfile.BootImage +
                        " root=/dev/ram0 rw ramdisk_size=156000 task=clobber " + "imageProfileId=" + _imageProfile.Id + namePromptArg +
                        " consoleblank=0" + " web=" + webPath + " USER_TOKEN=" + userToken + " " + globalComputerArgs +
                        " " + _imageProfile.KernelArguments + newLineChar);
            ipxe.Append("imgfetch --name " + _imageProfile.BootImage + " " + webPath +
                        "IpxeBoot?filename=" + _imageProfile.BootImage + "&type=bootimage" + newLineChar);
            ipxe.Append("boot" + newLineChar);


            var sysLinux = new StringBuilder();
            sysLinux.Append("DEFAULT clonedeploy" + newLineChar);
            sysLinux.Append("LABEL clonedeploy" + newLineChar);
            sysLinux.Append("KERNEL kernels" + Path.DirectorySeparatorChar + _imageProfile.Kernel + newLineChar);
            sysLinux.Append("APPEND initrd=images" + Path.DirectorySeparatorChar + _imageProfile.BootImage +
                            " root=/dev/ram0 rw ramdisk_size=156000 task=clobber " + "imageProfileId=" + _imageProfile.Id + namePromptArg +
                            " consoleblank=0" + " web=" + webPath + " USER_TOKEN=" + userToken + " " + globalComputerArgs +
                            " " + _imageProfile.KernelArguments + newLineChar);


            var grub = new StringBuilder();
            grub.Append("set default=0" + newLineChar);
            grub.Append("set timeout=0" + newLineChar);
            grub.Append("menuentry CloneDeploy --unrestricted {" + newLineChar);
            grub.Append("echo Please Wait While The Boot Image Is Transferred.  This May Take A Few Minutes." +
                        newLineChar);
            grub.Append("linux /kernels/" + _imageProfile.Kernel +
                        " root=/dev/ram0 rw ramdisk_size=156000 task=clobber " + "imageProfileId=" + _imageProfile.Id + namePromptArg + " consoleblank=0" + " web=" + webPath + " USER_TOKEN=" +
                        userToken + " " +
                        globalComputerArgs + " " + _imageProfile.KernelArguments + newLineChar);
            grub.Append("initrd /images/" + _imageProfile.BootImage + newLineChar);
            grub.Append("}" + newLineChar);

            var list = new List<Tuple<string, string, string>>
            {
                Tuple.Create("bios", "", sysLinux.ToString()),
                Tuple.Create("bios", ".ipxe", ipxe.ToString()),
                Tuple.Create("efi32", "", sysLinux.ToString()),
                Tuple.Create("efi32", ".ipxe", ipxe.ToString()),
                Tuple.Create("efi64", "", sysLinux.ToString()),
                Tuple.Create("efi64", ".ipxe", ipxe.ToString()),
                Tuple.Create("efi64", ".cfg", grub.ToString())
            };

            //In proxy mode all boot files are created regardless of the pxe mode, this way computers can be customized
            //to use a specific boot file without affecting all others, using the proxydhcp reservations file.
            if (Settings.ProxyDhcp == "Yes")
            {
                string path = null;

                foreach (var bootMenu in list)
                {
                    switch (bootMenu.Item2)
                    {
                        case ".cfg":
                            path = Settings.TftpPath + "grub" + Path.DirectorySeparatorChar + "grub.cfg";
                            break;
                        case ".ipxe":
                            path = Settings.TftpPath + "proxy" + Path.DirectorySeparatorChar + bootMenu.Item1 +
                                   Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar +
                                   "default.ipxe";
                            break;
                        case "":
                            path = Settings.TftpPath + "proxy" + Path.DirectorySeparatorChar + bootMenu.Item1 +
                                   Path.DirectorySeparatorChar + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                            break;
                    }

                    if (!new FileOps().WritePath(path, bootMenu.Item3))
                        return false;
                }
            }
            //When not using proxy dhcp, only one boot file is created
            else
            {
                var mode = Settings.PxeMode;
                string path = null;
                string fileContents = null;
                if (mode == "pxelinux" || mode == "syslinux_32_efi" || mode == "syslinux_64_efi")
                {
                    path = Settings.TftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default";
                    fileContents = sysLinux.ToString();
                }

                else if (mode.Contains("ipxe"))
                {
                    path = Settings.TftpPath + "pxelinux.cfg" + Path.DirectorySeparatorChar + "default.ipxe";
                    fileContents = ipxe.ToString();
                }
                else if (mode.Contains("grub"))
                {
                    path = Settings.TftpPath + "grub" + Path.DirectorySeparatorChar + "grub.cfg";
                    fileContents = grub.ToString();
                }

                if (!new FileOps().WritePath(path, fileContents))
                    return false;
            }

            return true;
        }
    }
}