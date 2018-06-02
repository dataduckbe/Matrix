﻿using AutoUpdaterDotNET;
using Ionic.Zip;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Matrix.Lib;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace Matrix.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GetLatestNews();
            GetCredits();
            GetUserSettings();
            packages = packageService.Create(settings, versions, serverPath);
            AutoUpdater.Start("https://militaryaiworks.com/matrix/app/latestVersion.xml");
        }


        //FIELDS


        string tempPath = Path.GetTempPath();
        string serverPath = Properties.Settings.Default.ServerPath;
        string installPath;
        bool manual;
        Dictionary<string, bool> settings = new Dictionary<string, bool>();
        Dictionary<string, string> versions = new Dictionary<string, string>();
        PackageService packageService = new PackageService();
        List<Package> packages;
        WebClient client;
        ProgressDialogController progress;
        MessageDialogResult messageResult;
        MessageDialogResult licenseResult;
        Logger logger = LogManager.GetCurrentClassLogger();



        //METHODS


        //Html

        void GetLatestNews()
        {
            string fileName = "latestNews.html";
            string url = $"{serverPath}/html/{fileName}?refreshToken=" + Guid.NewGuid().ToString();

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Display page if okay.
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    webLatestNews.Source = new Uri(url);
                    webLatestNews.LoadCompleted += (s, e) => webLatestNews.Visibility = Visibility.Visible;
                }

                // Releases the resources of the response.
                response.Close();
                response.Dispose();
            }

            catch (WebException)
            {
                //Something went wrong. Keep the browser hidden.
                webLatestNews.Visibility = Visibility.Hidden;
                txbNoConnection.Visibility = Visibility.Visible;
            }
        }

        void GetCredits()
        {
            string fileName = "credits.html";
            string url = $"{serverPath}/html/{fileName}";

            try
            {
                // Creates an HttpWebRequest.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                // Sends the HttpWebRequest and waits for a response.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                //Display page if okay.
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    webCredits.Source = new Uri(url);
                    webCredits.LoadCompleted += (s, e) => webCredits.Visibility = Visibility.Visible;
                }

                // Releases the resources of the response.
                response.Close();
                response.Dispose();
            }

            catch (WebException)
            {
                //Something went wrong. Keep the browser hidden.
                webCredits.Visibility = Visibility.Hidden;
                txbNoCredits.Visibility = Visibility.Visible;
            }
        }


        //User settings

        void GetUserSettings()
        {
            //Check if new version
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }

            //Get installpath and manual setting
            installPath = Properties.Settings.Default.InstallPath;
            manual = Properties.Settings.Default.ManualInstallation;

            //Get the installation settings and store value in public Dictionary settings so they can be used by PackageService
            settings.Add("isInstalledGlobalLibraries", Properties.Settings.Default.IsInstalledGlobalLibraries);
            settings.Add("isInstalledRegionAfrica", Properties.Settings.Default.IsInstalledRegionAfrica);
            settings.Add("isInstalledRegionAsia", Properties.Settings.Default.IsInstalledRegionAsia);
            settings.Add("isInstalledRegionEurope", Properties.Settings.Default.IsInstalledRegionEurope);
            settings.Add("isInstalledRegionNA", Properties.Settings.Default.IsInstalledRegionNA);
            settings.Add("isInstalledRegionOceania", Properties.Settings.Default.IsInstalledRegionOceania);
            settings.Add("isInstalledRegionSA", Properties.Settings.Default.IsInstalledRegionSA);

            //Get the version settings and store value in public Dictionary versions so they can be used by PackageService
            versions.Add("versionGlobalLibraries", Properties.Settings.Default.versionGlobalLibraries);
            versions.Add("versionRegionAfrica", Properties.Settings.Default.versionRegionAfrica);
            versions.Add("versionRegionAsia", Properties.Settings.Default.versionRegionAsia);
            versions.Add("versionRegionEurope", Properties.Settings.Default.versionRegionEurope);
            versions.Add("versionRegionNA", Properties.Settings.Default.versionRegionNA);
            versions.Add("versionRegionOceania", Properties.Settings.Default.versionRegionOceania);
            versions.Add("versionRegionSA", Properties.Settings.Default.versionRegionSA);
        }

        void SaveUserSettings()
        {
            Properties.Settings.Default.IsInstalledGlobalLibraries = packages[0].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAfrica = packages[2].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionAsia = packages[3].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionEurope = packages[4].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionNA = packages[5].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionOceania = packages[6].IsInstalled;
            Properties.Settings.Default.IsInstalledRegionSA = packages[7].IsInstalled;

            Properties.Settings.Default.versionGlobalLibraries = packages[0].InstalledVersion;
            Properties.Settings.Default.versionRegionAfrica = packages[2].InstalledVersion;
            Properties.Settings.Default.versionRegionAsia = packages[3].InstalledVersion;
            Properties.Settings.Default.versionRegionEurope = packages[4].InstalledVersion;
            Properties.Settings.Default.versionRegionNA = packages[5].InstalledVersion;
            Properties.Settings.Default.versionRegionOceania = packages[6].InstalledVersion;
            Properties.Settings.Default.versionRegionSA = packages[7].InstalledVersion;

            Properties.Settings.Default.InstallPath = installPath;
            Properties.Settings.Default.ManualInstallation = manual;

            Properties.Settings.Default.Save();
        }


        //Button states

        void MarkAsInstalled(Button b)
        {
            b.Content = "UNINSTALL";
            b.BorderBrush = Brushes.Green;
        }

        void MarkAsUninstalled(Button b)
        {
            b.Content = "INSTALL";
            b.ClearValue(BorderBrushProperty);
        }

        void MarkAsUpdatesAvailable(Button u, Badged b, Package p)
        {
            u.IsEnabled = true;
            b.Badge = p.Updates.Count;
        }

        void MarkAsNoUpdatesAvailable(Button u, Badged b)
        {
            u.IsEnabled = false;
            b.BadgeBackground = null;
            b.BadgeForeground = null;
        }

        void SetInitialStates()
        {
            //Set install buttons
            if (packages[0].IsInstalled) MarkAsInstalled(btnInstallGlobalLibraries);
            if (packages[2].IsInstalled) MarkAsInstalled(btnInstallRegionAfrica);
            btnInstallRegionAsia.IsEnabled = false; //if (packages[3].IsInstalled) MarkAsInstalled(btnInstallRegionAsia);
            btnInstallRegionEurope.IsEnabled = false; //if (packages[4].IsInstalled) MarkAsInstalled(btnInstallRegionEurope);
            btnInstallRegionNA.IsEnabled = false; //if (packages[5].IsInstalled) MarkAsInstalled(btnInstallRegionNA);
            if (packages[6].IsInstalled) MarkAsInstalled(btnInstallRegionOceania);
            if (packages[7].IsInstalled) MarkAsInstalled(btnInstallRegionSA);

            //Set update buttons
            if (packages[0].IsInstalled && packages[0].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries, packages[0]); else MarkAsNoUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries);
            if (packages[2].IsInstalled && packages[2].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica, packages[2]); else MarkAsNoUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica);
            if (packages[3].IsInstalled && packages[3].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia,packages[3]); else MarkAsNoUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia);
            if (packages[4].IsInstalled && packages[4].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope, packages[4]); else MarkAsNoUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope);
            if (packages[5].IsInstalled && packages[5].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA, packages[5]); else MarkAsNoUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA);
            if (packages[6].IsInstalled && packages[6].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania, packages[6]); else MarkAsNoUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania);
            if (packages[7].IsInstalled && packages[7].Updates.Count != 0) MarkAsUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA, packages[7]); else MarkAsNoUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA);

            //Set toggle buttons
            if (manual) tglManual.IsChecked = true;

            //Set install path
            txtInstallationFolder.Text = installPath;
        }

      
        //Messages

        private async Task ShowMessageMissingLibs()
        {
            messageResult = await this.ShowMessageAsync("Global Libraries", "The MAIW Global Libraries are required for this region. They will be installed first.", MessageDialogStyle.AffirmativeAndNegative);
        }

        private async Task ShowMessageLicense(string packageName)
        {
            licenseResult = await this.ShowMessageAsync("License", $"Installing the {packageName} package implies that you accept the MAIW license, available at https://militaryaiworks.com/license.", MessageDialogStyle.AffirmativeAndNegative);
        }


        //Download

        private async Task CheckInstallPath()
        {
            if (string.IsNullOrEmpty(installPath))
            {
                await this.ShowMessageAsync("Settings", "Please select where you want to install your military AI traffic.\nYou only need to do this once.");

                WinForms.FolderBrowserDialog dlg = new WinForms.FolderBrowserDialog();
                dlg.Description = "Please select the folder where you want to install your military AI traffic from MAIW.";

                WinForms.DialogResult result = dlg.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                {
                    if (Directory.Exists(dlg.SelectedPath))
                    {
                        installPath = dlg.SelectedPath + "\\Military AI Works\\";
                        txtInstallationFolder.Text = installPath;
                    }
                    else
                    {
                        await this.ShowMessageAsync("Error", "There was a problem with the selected folder, or access to the folder was denied.");
                    }
                }
            }
        }

        private async Task DownloadVoicepack(string fileName, string location)
        {
            string url = $"{serverPath}/packages/{fileName}.zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), location);
        }

        private async Task DownloadFile(string fileName, string location)
        {
            string url = $"{serverPath}/packages/{fileName}.zip";
            string filePath = Path.Combine(location, fileName) + ".zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), filePath);
        }

        private async Task DownloadUpdate(string fileName, string location)
        {
            string url = $"{serverPath}/packages/updates/{fileName}.zip";
            string filePath = Path.Combine(location, fileName) + ".zip";

            client = new WebClient();
            client.DownloadProgressChanged += DownloadProgressChanged;
            await client.DownloadFileTaskAsync(new Uri(url), filePath);
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            string received = ((double)e.BytesReceived / 1048576).ToString("0.0");
            string total = ((double)e.TotalBytesToReceive / 1048576).ToString("0.0");

            if (progress.IsCanceled)
            {
                client.CancelAsync();
            }
            else
            {
                progress.SetProgress((double)e.ProgressPercentage / 100);
                progress.SetMessage($"{received} MB of {total} MB");
            }
        }


        // Unzip

        private async Task UnzipFile(string fileName)
        {
            string source = Path.Combine(tempPath, fileName) + ".zip";

            await Task.Run(() =>
            {
                using (ZipFile zip = new ZipFile(source))
                {
                    zip.ExtractProgress += new EventHandler<ExtractProgressEventArgs>(UnzipProgressChanged);
                    zip.ExtractAll(installPath, ExtractExistingFileAction.OverwriteSilently);
                }
            });

            File.Delete(source);

        }

        private void UnzipProgressChanged(object sender, ExtractProgressEventArgs e)
        {
            double unpacked = e.EntriesExtracted;
            double total = e.EntriesTotal;

            if (e.EntriesTotal != 0)
            {
                progress.SetProgress(unpacked / total);
                progress.SetMessage(e.EntriesExtracted.ToString() + " of " + e.EntriesTotal.ToString() + " files");
            }
        }


        // Button Clicks

        private async Task ClickInstallButton(Package p, Button b)
        {
            if (p.IsInstalled == false) //Package is not yet installed
            {
                //check install path
                await CheckInstallPath();
                if (string.IsNullOrEmpty(installPath)) return;

                //Show async license message
                await ShowMessageLicense(p.Name);
                if (licenseResult == MessageDialogResult.Affirmative)
                {
                    //Show async progress message
                    progress = await this.ShowProgressAsync("Downloading...", "", true);
                    progress.SetIndeterminate();

                    if (!progress.IsCanceled)
                    {
                        try
                        {
                            //Download Package
                            await DownloadFile(p.FileName, tempPath);
                            progress.SetMessage("Done!");

                            //Short pause
                            await Task.Delay(1500);

                            //Unzip Package
                            progress.SetTitle("Unpacking...");
                            progress.SetMessage("");
                            progress.SetIndeterminate();
                            progress.SetCancelable(false);
                            await UnzipFile(p.FileName);
                            progress.SetMessage("Done!");

                            //Short pause
                            await Task.Delay(1500);

                            //Create addon.xml file
                            progress.SetTitle("Creating add-on.xml...");
                            progress.SetMessage("");
                            progress.SetProgress(0);
                            packageService.CreateAddon(p, installPath, manual);
                            await Task.Delay(1500);
                            progress.SetMessage("Done!");
                            progress.SetProgress(1);

                            //Short pause
                            await Task.Delay(1500);

                        }
                        catch (WebException we)
                        {
                            logger.Error(we, "Download Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", $"{p.Name} can not be downloaded.\nPlease check your internet connection and try again.");
                            return;
                        }
                        catch (Exception ex)
                        {
                            if (progress.IsCanceled)
                            {
                                //Close progress without message
                                await progress.CloseAsync();
                            }
                            else
                            {
                                logger.Error(ex, "Install Error");
                                await progress.CloseAsync();
                                await this.ShowMessageAsync("Error", $"Something went wrong while installing {p.Name}.\nPlease try again or contact us on our forums.");
                            }
                            return;
                        }
                    }

                    //Change user setting and button
                    p.IsInstalled = true;
                    p.InstalledVersion = p.CurrentVersion;
                    MarkAsInstalled(b);


                    //Close progress and show success message
                    await progress.CloseAsync();
                    await this.ShowMessageAsync("Success!", $"{p.Name} is installed.");
                }
            }

            else //package is installed
            {
                //Uninstall
                try
                {
                    packageService.Uninstall(p, installPath);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Uninstall Error");
                    await this.ShowMessageAsync("Error", $"Something went wrong while uninstalling {p.Name}.\nPlease try again or contact us on our forums.");
                    return;
                }

                //Change user setting and button
                p.IsInstalled = false;
                MarkAsUninstalled(b);

                //Show success message
                await this.ShowMessageAsync("Success!", $"{p.Name} is uninstalled.");
            }
        }

        private async Task ClickUpdateButton(Package p, Button u, Badged b)
        {
            foreach (string update in p.Updates)
            {
                //Show async progress message
                progress = await this.ShowProgressAsync("Preparing update to version " + update + "...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    string fileName = p.FileName + "_" + update;

                    try
                    {
                        //Download File Removal List
                        WebRequest request = WebRequest.Create($"{serverPath}/packages/updates/{fileName}.txt");
                        WebResponse response = await request.GetResponseAsync();
                        List<string> files = new List<string>();

                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            while (!reader.EndOfStream)
                            {
                                files.Add(reader.ReadLine());
                            }
                        }

                        //Short pause
                        await Task.Delay(1500);

                        //Remove Files
                        progress.SetTitle("Removing files...");
                        progress.SetMessage("");
                        if (files.Count > 0)
                        {
                            foreach (string f in files)
                            {
                                string path = installPath + p.FolderName + f;
                                if (File.Exists(path)) File.Delete(path);
                            }
                        }
                        progress.SetMessage("Done!");
                        progress.SetProgress(1);

                        //Short pause
                        await Task.Delay(1500);

                        //Download Update
                        progress.SetTitle("Downloading update...");
                        progress.SetMessage("");
                        progress.SetIndeterminate();
                        await DownloadUpdate(fileName, tempPath);
                        progress.SetMessage("Done!");

                        //Short pause
                        await Task.Delay(1500);

                        //Unzip Update
                        progress.SetTitle("Unpacking update...");
                        progress.SetMessage("");
                        progress.SetIndeterminate();
                        progress.SetCancelable(false);
                        await UnzipFile(fileName);
                        progress.SetMessage("Done!");

                        //Short pause
                        await Task.Delay(1500);

                    }
                    catch (WebException we)
                    {
                        logger.Error(we, "Download Error");
                        await progress.CloseAsync();
                        await this.ShowMessageAsync("Error", $"The update can not be downloaded.\nPlease check your internet connection and try again.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (progress.IsCanceled)
                        {
                            //Close progress without message
                            await progress.CloseAsync();
                        }
                        else
                        {
                            logger.Error(ex, "Update Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", $"Something went wrong while updating {p.Name}.\nPlease try again or contact us on our forums.");
                        }
                        return;
                    }
                }
            }

            //Change user setting and button
            p.InstalledVersion = p.CurrentVersion;
            MarkAsNoUpdatesAvailable(u, b);


            //Close progress and show success message
            await progress.CloseAsync();
            await this.ShowMessageAsync("Success!", $"{p.Name} is updated.");            
        }



        //EVENTS


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetInitialStates();
        }

        private void lstMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Link menu to tabs
            if (tclMainWindow != null)
            {
                if (lstMenu.SelectedItem != null)
                {
                    int s = lstMenu.SelectedIndex;
                    tclMainWindow.SelectedIndex = s;
                }
                else lstMenu.SelectedIndex = 0;
            }
        }

        private void btnUserManual_Click(object sender, RoutedEventArgs e)
        {
            //Open user manual PDF
            string path = Path.Combine(Directory.GetCurrentDirectory(), "Matrix-Manual.pdf");
            if (File.Exists(path)) Process.Start(path);            
        }

        private void btnSupport_Click(object sender, RoutedEventArgs e)
        {
            //Open support forum in a browser window.
            string url = @"https://militaryaiworks.com/forums";
            Process.Start(url);
        }

        private void btnDonate_Click(object sender, RoutedEventArgs e)
        {
            //Open our PayPal donation page in a browser window.
            string url = Properties.Settings.Default.PayPalLink;
            Process.Start(url);
        }

        private async void btnDownloadGlobalVoicepack_Click(object sender, RoutedEventArgs e)
        {

            string fileName = packages[1].FileName;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Title = "Save the Global MAIW Voicepack";
            dlg.FileName = fileName;
            dlg.Filter = "Zip file (*.zip)|*.zip";


            if (dlg.ShowDialog() == true)
            {
                progress = await this.ShowProgressAsync("Downloading...", "", true);
                progress.SetIndeterminate();

                if (!progress.IsCanceled)
                {
                    try
                    {
                        await DownloadVoicepack(fileName, dlg.FileName);
                    }
                    catch (WebException we)
                    {
                        logger.Error(we, "Download Error");
                        await progress.CloseAsync();
                        await this.ShowMessageAsync("Error", $"The Global MAIW Voicepack can not be downloaded.\nPlease check your internet connection and try again.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        if (progress.IsCanceled)
                        {
                            await progress.CloseAsync();
                        }
                        else
                        {
                            logger.Error(ex, "Install Error");
                            await progress.CloseAsync();
                            await this.ShowMessageAsync("Error", "Something went wrong while installing the Global MAIW Voicepack.\nPlease try again or contact us on our forums.");
                        }
                        return;
                    }
                }

                await progress.CloseAsync();
                await this.ShowMessageAsync("Success!", "The Global MAIW Voicepack was downloaded succesfully.\nYou will need to unzip the files and import them into EVP manually.");
            }
        }

        private async void btnInstallGlobalLibraries_Click(object sender, RoutedEventArgs e)
        {
            //Check if any regions are installed before uninstalling the global package
            bool regionsInstalled = false;
            for (int i = 2; i < packages.Count; i++)
            {
                if (packages[i].IsInstalled) regionsInstalled = true;
            }

            if (regionsInstalled)
            {
                await this.ShowMessageAsync("Error", "You still have regions installed that depend on the Global MAIW Libraries.");
                return;
            }
            else await ClickInstallButton(packages[0], (Button)sender);

            MarkAsNoUpdatesAvailable(btnUpdateGlobalLibraries, bdgGlobalLibraries);
        }

        private async void btnUpdateGlobalLibraries_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[0], btnUpdateGlobalLibraries, bdgGlobalLibraries);
        }

        private async void btnInstallRegionAfrica_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[2], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[2], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionAfrica, bdgRegionAfrica);
        }

        private async void btnUpdateRegionAfrica_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[2], btnUpdateRegionAfrica, bdgRegionAfrica);
        }

        private async void btnInstallRegionAsia_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[3], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[3], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionAsia, bdgRegionAsia);
        }

        private async void btnUpdateRegionAsia_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[3], btnUpdateRegionAsia, bdgRegionAsia);
        }

        private async void btnInstallRegionEurope_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[4], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[4], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionEurope, bdgRegionEurope);
        }

        private async void btnUpdateRegionEurope_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[4], btnUpdateRegionEurope, bdgRegionEurope);
        }

        private async void btnInstallRegionNA_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[5], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[5], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionNA, bdgRegionNA);
        }

        private async void btnUpdateRegionNA_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[5], btnUpdateRegionNA, bdgRegionNA);
        }

        private async void btnInstallRegionOceania_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[6], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[6], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionOceania, bdgRegionOceania);
        }

        private async void btnUpdateRegionOceania_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[6], btnUpdateRegionOceania, bdgRegionOceania);
        }

        private async void btnInstallRegionSA_Click(object sender, RoutedEventArgs e)
        {
            if (packages[0].IsInstalled) await ClickInstallButton(packages[7], (Button)sender);
            else
            {
                await ShowMessageMissingLibs();

                if (messageResult == MessageDialogResult.Affirmative)
                {
                    await ClickInstallButton(packages[0], btnInstallGlobalLibraries);
                    await ClickInstallButton(packages[7], (Button)sender);
                }
                else return;
            }

            MarkAsNoUpdatesAvailable(btnUpdateRegionSA, bdgRegionSA);
        }

        private async void btnUpdateRegionSA_Click(object sender, RoutedEventArgs e)
        {
            await ClickUpdateButton(packages[7], btnUpdateRegionSA, bdgRegionSA);
        }

        private void tglManual_Checked(object sender, RoutedEventArgs e)
        {
            manual = true;
        }

        private void tglManual_Unchecked(object sender, RoutedEventArgs e)
        {
            manual = false;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            SaveUserSettings();
        }

        private void imgRegionAfrica_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionAfrica.pdf";
            Process.Start(url);
        }

        private void imgRegionOceania_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/docs/MAIW-RegionOceania.pdf";
            Process.Start(url);
        }

        private void imgRegionSA_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            string url = $"{serverPath}/matrix/docs/MAIW-RegionSA.pdf";
            Process.Start(url);
        }
    }
}
