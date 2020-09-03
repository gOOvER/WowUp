﻿using Serilog;
using System;
using System.IO;
using System.Threading;

namespace WowUp.Utilities
{
    public static class FileUtilities
    {
        private static readonly string LocalAppDataPath = Environment
            .GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public static readonly string AppDataPath = Path.Combine(LocalAppDataPath, "WowUp");
        public static readonly string AppLogsPath = Path.Combine(AppDataPath, "Logs");

        public static void TryMove(string sourcePath, string destinationPath, bool overwrite)
        {
            var retryMax = 10;
            var retryDelay = 1000;

            for (int i = 1; i <= retryMax; ++i)
            {
                try
                {
                    File.Move(sourcePath, destinationPath, overwrite);
                    break;
                }
                catch (IOException e) when (i <= retryMax)
                {
                    Log.Error(e, "Move failed");
                    Thread.Sleep(retryDelay);
                }
            }
        }

        public static void Move(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            MoveAll(diSource, diTarget);
        }

        public static void MoveAll(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            // Copy each file into the new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Log.Information($"Moving file {target.FullName} => {fi.Name}");
                fi.MoveTo(Path.Combine(target.FullName, fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                MoveAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void MoveContents(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                var newDirectoryPath = dirPath.Replace(sourcePath, destinationPath);
                Log.Information($"Creating directory {newDirectoryPath}");

                Directory.CreateDirectory(newDirectoryPath);
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                var newDestinationPath = newPath.Replace(sourcePath, destinationPath);
                Log.Information($"Moving file {newPath} => {newDestinationPath}");

                File.Move(newPath, newDestinationPath, true);
            }
        }
    }
}
