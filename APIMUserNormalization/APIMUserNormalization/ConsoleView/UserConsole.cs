﻿using System;
using System.Collections.Generic;
using System.Text;
using APIMUserNormalization.Services;
using System.Threading;
using System.Threading.Tasks;
using APIMUserNormalization.Models;
using System.Collections;

namespace APIMUserNormalization.ConsoleView
{
    public class UserConsole
    {

        MigrationService migrationService;
        bool userSetup = false;

        public UserConsole()
        {
            migrationService = new MigrationService();
        }


        public async Task<bool> PrintMainMenu()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine("API Management and AD B2C Environments list:");
            Console.WriteLine("====================");

            await migrationService.ServiceBootstraping();
            userSetup = true;

            Console.WriteLine("Command  Description");
            Console.WriteLine("====================");


            Console.ForegroundColor = userSetup ? Console.ForegroundColor = ConsoleColor.Gray : Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[A]      Create List of Users");
            if (userSetup)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[B]      Manage Users");
                Console.WriteLine("[C]      Normalize all Users");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("[D]      Backup/Restore APIM");
            Console.WriteLine("[E]      Print Config Attributes");
            Console.WriteLine("[X]      Exit Program Console");
            Console.WriteLine("-------------------------");
            string option = Console.ReadLine();
            bool leaveLoop = false;
            option = option.ToUpper();

            switch (option)
            {
                case "A":
                    Console.WriteLine("This process will take some time, please wait...");
                    await migrationService.SetupUserCollections();
                    userSetup = true;
                    Console.WriteLine("Done...");
                    break;
                case "B":
                    await ManageUsersAsync();
                    Console.WriteLine("Done...");
                    break;
                case "C":
                    await migrationService.NormalizeAllUsersAsync();
                    Console.WriteLine("Done...");
                    break;
                case "D":
                    await BackupRestoreAPIM();
                    Console.WriteLine("Done...");
                    break;
                case "E":
                    PrintConfig();
                    Console.WriteLine("Done...");
                    break;
                case "X":
                    leaveLoop = true;
                    break;
                default:
                    Console.WriteLine("Option not recognized");
                    break;
            }
            Thread.Sleep(2000);
            if (!leaveLoop)
            {
                Console.Write("Press any key to continue...");
                Console.ReadLine();
            }

            return leaveLoop;

        }

        private async Task ManageUsersAsync()
        {
            Console.WriteLine("");
            Console.WriteLine("====================");
            Console.WriteLine("Choose your user #");
            string idx = Console.ReadLine().ToUpper();
            int userIdx = int.Parse(idx);
            ArrayList userNormalizationList = migrationService.getUserNormalizationList();
            var un = (UserNormalization) userNormalizationList[userIdx - 1];
            Console.WriteLine("User Email: " + un.Email);

            Console.Write("    Is user normilized?: ");
            if (!un.IsNormilized)
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(un.IsNormilized);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("    Found in " + un.ApimsFound + " APIM Services");
            Console.WriteLine("    User with " + un.UniqueIdS + " unique Object IdS");
            Console.WriteLine();
            Console.WriteLine("Normalization Plan:");

            string s4 = string.Empty;
            string s5 = string.Empty;
            string s6 = string.Empty;
            foreach (UserNormalizationStatus uns in un.UsersStatus)
            {
                Console.WriteLine("    APIM: " + uns.APIMName + ":");

                if (!uns.ExistsInAPIM) Console.WriteLine("        [APIM] - Create User");
                if (!uns.IsFoundInADB2C) Console.WriteLine("        [ADB2C]- Create User");
                if (!uns.HasADB2C) Console.WriteLine("        [APIM] - Add ADB2C Identity");
                if (!uns.IsEmailFoundInADB2C) Console.WriteLine("        [ADB2C] - Update Properties");
            }

            Console.WriteLine();
            Console.WriteLine("Normalize User? (Y/N)");
            string yn = Console.ReadLine().ToUpper();
            if (yn.ToUpper().Equals("Y"))
            {
                await migrationService.NormalizeUserAsync(un);
            }

        }
        
        private async Task BackupRestoreAPIM()
        {
            Console.WriteLine("[1]      Backup APIM");
            Console.WriteLine("[2]      Restore APIM");
            Console.WriteLine("[3]      Go Back");
            string option = Console.ReadLine();
            option = option.ToUpper();

            switch (option)
            {
                case "1":
                    await migrationService.BackupAPIM();
                    userSetup = true;
                    Console.WriteLine("Done...");
                    break;
                case "2":
                    Console.WriteLine("What is the backup name:");
                    string backupName = Console.ReadLine();
                    await migrationService.RestoreAPIM(backupName);
                    Console.WriteLine("Done...");
                    break;
                case "3":
                    PrintMainMenu();
                    break;
                default:
                    PrintMainMenu();
                    break;
            }
        }



        public void PrintConfig()
        {
            migrationService.PrintConfig();
        }

    }

}




