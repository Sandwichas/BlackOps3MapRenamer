using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlackOps3MapRenamer
{
    public partial class Form1 : Form
    {

        private string _mapName;
        private string _mapDir;
        private string _mapSourceDir;
        private string _mapLedDir;
        private string _zoneFilePath;
        private string _zoneSourceDirectoryPath;
        private string[] _assetsLanguages = { "traditionalchinese", "spanish", "simplifiedchinese", "russian",
            "portuguese", "polish", "japanese", "italian", "german", "french", "englisharabic", "english" };
        private const string AllLanguage = "all";
        private const string MapNamePrefix = "zm_";
        // private string[] zonePrefixes = { "scriptparsetree,", "sound,", "gfx_map," };

        /*
		 * Ignore .deps files because they are generated with linker
		 * Rename/copy map led
		 * Change inside: map.zone
		 * Change inside: Call of Duty Black Ops III\\usermaps\\%map%\\zone_source\\loc\\zm_tp.zone:
		 * Change file name: Call of Duty Black Ops III\\usermaps\\%map%\\zone_source\\all\\scriptgdb\\scripts\\zm\\zm_tp.csc.gdb
		 * Change file name: Call of Duty Black Ops III\\usermaps\\zm_tp\\zone_source\\all\\scriptgdb\\scripts\\zm\\zm_tp.gsc.gdb
		 * Change all files names in: Call of Duty Black Ops III\\usermaps\\%map%\\zone
		 */
        public Form1()
        {
            InitializeComponent();
        }

        public static string GetSpecificSteamLibraryPath()
        {
            if (IsAdministrator())
            {
                return FindSteamLibraryPathWithWorkshopContent();
            }
            else
            {
                Console.WriteLine("Insufficient privileges. Steam library path not retrieved.");
                return null;
            }
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static string FindSteamLibraryPathWithWorkshopContent()
        {
            try
            {
                string steamPath = GetSteamPathFromRegistry();

                if (!string.IsNullOrEmpty(steamPath))
                {
                    string libraryFoldersVdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

                    if (File.Exists(libraryFoldersVdfPath))
                    {
                        string[] lines = File.ReadAllLines(libraryFoldersVdfPath);

                        foreach (string line in lines)
                        {
                            if (line.Contains("\"path\""))
                            {
                                string path = line.Split('"')[3];
                                string normalizedPath = path.Replace("\\\\", "\\");
                                string usermapsAutoFolder = Path.Combine(normalizedPath, "steamapps", "common", "Call of Duty Black Ops III", "usermaps");

                                if (Directory.Exists(usermapsAutoFolder))
                                {
                                    return usermapsAutoFolder;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding Steam library path: {ex.Message}");
            }

            return null; // Path not found.
        }

        public static string GetSteamPathFromRegistry()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        object steamPathObject = key.GetValue("SteamPath");
                        if (steamPathObject != null)
                        {
                            return steamPathObject.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting Steam path: {ex.Message}");
            }

            return null;
        }

        private static string GetParentDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);

            if (dir.Parent != null && dir.Parent.Parent != null)
            {
                return dir.Parent.Name; // Returns "Test" for "C:/Test/Hello"
            }

            return null; // Return null if there's no second-to-last directory
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(pathTextBox.Text))
            {
                folderBrowserDialog1.SelectedPath = pathTextBox.Text;
            }
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                pathTextBox.Text = folderBrowserDialog1.SelectedPath;
                DirectoryInfo dirInfo = new DirectoryInfo(pathTextBox.Text);
                _mapSourceDir = dirInfo.Parent.Parent.FullName + Path.DirectorySeparatorChar + "map_source" + Path.DirectorySeparatorChar + "zm";
                _mapLedDir = dirInfo.Parent.Parent.FullName + Path.DirectorySeparatorChar + "share" + Path.DirectorySeparatorChar + "raw" + Path.DirectorySeparatorChar + "maps" + Path.DirectorySeparatorChar + "zm";
                string lastDirectoryName = dirInfo.Name;
                _mapName = lastDirectoryName;
                _mapDir = dirInfo.FullName;
                string zoneFile = pathTextBox.Text + Path.DirectorySeparatorChar + "zone_source" + Path.DirectorySeparatorChar + _mapName + ".zone";
                _zoneFilePath = zoneFile;
                _zoneSourceDirectoryPath = pathTextBox.Text + Path.DirectorySeparatorChar + "zone_source" + Path.DirectorySeparatorChar;
                if (!File.Exists(zoneFile))
                {
                    MessageBox.Show("Invalid map folder selected: " + _mapName + ". No zone file found.", "Map selection", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _mapName = null;
                }
                string usermapDir = GetParentDirectory(_mapDir);
                if (usermapDir != null) Properties.Settings.Default.usermap_folder = usermapDir;
                Properties.Settings.Default.Save();
            }
        }

        private async void renameButton_Click(object sender, EventArgs e)
        {
            if (_mapName == null || pathTextBox.Text.Length == 0)
            {
                MessageBox.Show("No map folder was selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (newNameTextBox.Text.Length == 0)
            {
                MessageBox.Show("Please type a new name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (newNameTextBox.Text.StartsWith("zm_"))
            {
                MessageBox.Show("Don't add 'zm_' prefix. It will be included automatically. Please remove it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            detailsRichTextBox.Invoke(new Action(() => detailsRichTextBox.Clear()));
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 10;
            await CreateBackupFolder(true);
            await RenameScriptOccurences();
            await RenameZoneOccurrences();
            await RenameAssetsLangOccurrences();
            await RenameZoneLocOccurrences();
            await RenameAllLangOccurrences();
            await RenameLangOccurrences("english");
            await RenameLangOccurrences("french");
            await RenameLangOccurrences("german");
            await RenameLangOccurrences("italian");
            await RenameLangOccurrences("japanese");
            await RenameLangOccurrences("polish");
            await RenameLangOccurrences("portuguese");
            await RenameLangOccurrences("russian");
            await RenameLangOccurrences("simplifiedchinese");
            await RenameLangOccurrences("spanish");
            await RenameLangOccurrences("traditionalchinese");
            await RenameLangOccurrences("englisharabic");
            await RenameSoundZoneConfigOccurrence();
            await RenameMapSourceFile();
            await RenameMapLedFile();
            await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(_mapDir), _mapName, MapNamePrefix + newNameTextBox.Text));
            // Rename the map folder itself
            string parentDir = Path.GetDirectoryName(_mapDir.TrimEnd(Path.DirectorySeparatorChar));
            if (!string.IsNullOrEmpty(parentDir))
            {
                string newFolderName = MapNamePrefix + newNameTextBox.Text;
                string newFolderPath = Path.Combine(parentDir, newFolderName);
                try
                {
                    if (!Directory.Exists(newFolderPath))
                    {
                        await Task.Run(() => Directory.Move(_mapDir, newFolderPath));
                        AppendColoredTextSafe("[FOLDER RENAME] " + _mapDir + " -> " + newFolderPath + "\n", Color.Green);
                        _mapDir = newFolderPath;
                        _mapName = newFolderName;
                    }
                    else
                    {
                        AppendColoredTextSafe("[FOLDER RENAME SKIP] Target exists: " + newFolderPath + "\n", Color.Orange);
                    }
                }
                catch (Exception ex)
                {
                    AppendColoredTextSafe("[FOLDER RENAME ERROR] " + ex.Message + "\n", Color.Red);
                }
            }
            progressBar1.Style = ProgressBarStyle.Blocks;
            progressBar1.MarqueeAnimationSpeed = 0;
            MessageBox.Show("Renaming process completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task<List<string>> RenameScriptOccurences()
        {
            string scriptsFolderPath = _mapDir + Path.DirectorySeparatorChar + "scripts" + Path.DirectorySeparatorChar + "zm";
            if (!Directory.Exists(scriptsFolderPath)) return null;
            // search for map name usage with function calls and usings in .gsc, .csc files
            string[] scriptFiles = Directory.GetFiles(scriptsFolderPath, "*.*", SearchOption.AllDirectories).Where(item => item.EndsWith(".gsc") || item.EndsWith(".csc")).ToArray<string>();
            foreach (string scriptFilePath in scriptFiles)
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(scriptFilePath));
                List<string> newLines = new List<string>();
                string newMapName = MapNamePrefix + newNameTextBox.Text;
                foreach (string line in lines)
                {
                    string updatedLine = line;
                    if (line.Contains(_mapName + "::"))
                    {
                        updatedLine = line.Replace(_mapName + "::", newMapName + "::");
                        AppendColoredTextSafe("[SCRIPT] " + line + " -> " + updatedLine + "\n", Color.DarkGoldenrod);
                    }
                    if (line.Contains("#using scripts\\zm\\" + _mapName + ";"))
                    {
                        updatedLine = line.Replace("#using scripts\\zm\\" + _mapName + ";", "#using scripts\\zm\\" + newMapName + ";");
                    }
                    newLines.Add(updatedLine);
                }
                try
                {
                    await Task.Run(() => File.WriteAllLines(scriptFilePath, newLines));
                    AppendColoredTextSafe("[SCRIPT] File updated: " + scriptFilePath + "\n", Color.Blue);
                }
                catch (Exception ex)
                {
                    AppendColoredTextSafe("[SCRIPT WRITE ERROR] " + ex.Message + "\n", Color.Red);
                }
            }

            return null;
        }

        private async Task<List<string>> RenameMapSourceFile()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string oldFilePath = Path.Combine(_mapSourceDir, _mapName + ".map");
            string newFilePath = Path.Combine(_mapSourceDir, newMapName + ".map");
            await Task.Run(() => File.Copy(oldFilePath, newFilePath, true));
            return null;
        }

        private async Task<List<string>> RenameMapLedFile()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string oldFilePath = Path.Combine(_mapLedDir, _mapName + ".led");
            string newFilePath = Path.Combine(_mapLedDir, newMapName + ".led");
            await Task.Run(() => File.Copy(oldFilePath, newFilePath, true));
            return null;
        }

        private async Task<List<string>> RenameZoneOccurrences()
        {
            if (string.IsNullOrEmpty(_zoneFilePath) || !File.Exists(_zoneFilePath)) return null;
            string[] lines = await Task.Run(() => File.ReadAllLines(_zoneFilePath));
            List<string> newLines = new List<string>();
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            foreach (string line in lines)
            {
                string originalLine = line.TrimStart();
                string updatedLine = line.TrimStart();
                if (StartsWithAny(updatedLine, "col_map,", "gfx_map,") && updatedLine.EndsWith(_mapName + ".d3dbsp"))
                {
                    updatedLine = updatedLine.Replace(_mapName + ".d3dbsp", newMapName + ".d3dbsp");
                }
                else if (updatedLine.StartsWith("sound,") && updatedLine.EndsWith(_mapName))
                {
                    updatedLine = updatedLine.Replace("," + _mapName, "," + newMapName);
                }
                else if (updatedLine.StartsWith("scriptparsetree,") && EndsWithAny(updatedLine, _mapName + ".gsc", _mapName + ".csc"))
                {
                    updatedLine = updatedLine.Replace("scriptparsetree,scripts/zm/" + _mapName + ".", "scriptparsetree,scripts/zm/" + newMapName + ".");
                }
                newLines.Add(updatedLine);
                if (!originalLine.Equals(updatedLine))
                {
                    AppendColoredParts(("[ZONE] ", Color.FromArgb(255, 128, 0)), (originalLine, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                }
            }

            // Write updated content back to zone file
            try
            {
                await Task.Run(() => File.WriteAllLines(_zoneFilePath, newLines));
                AppendColoredTextSafe("[ZONE] File updated: " + _zoneFilePath + "\n", Color.Blue);

                // Rename files in related directories: zone_source and zone folder
                string zoneSourceDir = Path.GetDirectoryName(_zoneFilePath);
                if (!string.IsNullOrEmpty(zoneSourceDir))
                {
                    await Task.Run(() => RenameFilesInDirectoryRecursively(zoneSourceDir, _mapName, newMapName));
                }
                string zoneDir = Path.Combine(_mapDir, "zone");
                if (Directory.Exists(zoneDir))
                {
                    await Task.Run(() => RenameFilesInDirectoryRecursively(zoneDir, _mapName, newMapName));
                }
            }
            catch (Exception ex)
            {
                AppendColoredTextSafe("[ZONE WRITE ERROR] " + ex.Message + "\n", Color.Red);
            }

            return newLines;
        }

        private async Task<List<string>> RenameSoundZoneConfigOccurrence()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string soundZoneConfigPath = _mapDir + Path.DirectorySeparatorChar + "sound" + Path.DirectorySeparatorChar + "zoneconfig" + Path.DirectorySeparatorChar + _mapName + ".szc";
            if (!File.Exists(soundZoneConfigPath))
            {
                MessageBox.Show("Unable to find path: " + soundZoneConfigPath);
                return null;
            }
            string[] lines = await Task.Run(() => File.ReadAllLines(soundZoneConfigPath));
            List<string> newLines = new List<string>();
            foreach (string line in lines)
            {
                string updatedLine = line;
                if (updatedLine.Contains("\"Name\" : \"" + _mapName))
                {
                    updatedLine = updatedLine.Replace("\"Name\" : \"" + _mapName, "\"Name\" : \"" + newMapName);
                    AppendColoredParts(("[SOUND ZONE CONFIG] ", Color.DarkOliveGreen), (line, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                }
                newLines.Add(updatedLine);
            }
            await Task.Run(() => File.WriteAllLines(soundZoneConfigPath, newLines));
            return null;
        }

        private async Task<List<string>> RenameAssetsLangOccurrences()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            foreach (string lang in _assetsLanguages)
            {
                string langDir = _zoneSourceDirectoryPath + lang;
                if (Directory.Exists(langDir))
                {
                    string assetListDir = langDir + Path.DirectorySeparatorChar + "assetlist";
                    string assetInfoDir = langDir + Path.DirectorySeparatorChar + "assetinfo";
                    if (Directory.Exists(assetListDir))
                    {
                        string assetListPath = assetListDir + Path.DirectorySeparatorChar + _mapName + ".csv";
                        if (File.Exists(assetListPath))
                        {
                            string[] csvLines = await Task.Run(() => File.ReadAllLines(assetListPath));
                            List<string> newLines = new List<string>();
                            bool changed = false;
                            foreach (string csvLine in csvLines)
                            {
                                string updatedLine = csvLine;
                                if (csvLine.Equals("sound," + _mapName))
                                {
                                    updatedLine = "sound," + newMapName;
                                }
                                else if (csvLine.Equals("keyvaluepairs," + _mapName))
                                {
                                    updatedLine = "keyvaluepairs," + _mapName;
                                }
                                newLines.Add(updatedLine);
                                if (!csvLine.Equals(updatedLine))
                                {
                                    changed = true;
                                    AppendColoredParts(("[ZONE_SOURCE, " + lang + ", ASSETLIST] ", Color.DarkCyan), (csvLine, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                                }
                            }
                            if (changed)
                            {
                                try
                                {
                                    await Task.Run(() => File.WriteAllLines(assetListPath, newLines));
                                    AppendColoredTextSafe("[ZONE_SOURCE, " + lang + ", ASSETLIST] File updated: " + assetListPath + "\n", Color.Blue);
                                    // rename file itself
                                    string newAssetListPath = Path.Combine(Path.GetDirectoryName(assetListPath), newMapName + ".csv");
                                    try
                                    {
                                        if (!File.Exists(newAssetListPath)) File.Move(assetListPath, newAssetListPath);
                                        else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetListPath + "\n", Color.Orange);
                                        // also rename other related files in this assetListDir
                                        await Task.Run(() => RenameFilesInDirectoryRecursively(assetListDir, _mapName, newMapName));
                                    }
                                    catch (Exception ex)
                                    {
                                        AppendColoredTextSafe("[ASSETLIST RENAME ERROR] " + ex.Message + "\n", Color.Red);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AppendColoredTextSafe("[ZONE_SOURCE, " + lang + ", ASSETLIST WRITE ERROR] " + ex.Message + "\n", Color.Red);
                                }
                            }
                        }
                    }
                    if (Directory.Exists(assetInfoDir))
                    {
                        string assetInfoPath = assetInfoDir + Path.DirectorySeparatorChar + _mapName + ".csv";
                        if (File.Exists(assetInfoPath))
                        {
                            string[] csvLines = await Task.Run(() => File.ReadAllLines(assetInfoPath));
                            List<string> newLines = new List<string>();
                            bool changed = false;
                            foreach (string csvLine in csvLines)
                            {
                                string updatedLine = csvLine;
                                if (csvLine.EndsWith("|csv") && csvLine.Contains("|zone_source/loc/" + _mapName + ".zone"))
                                {
                                    updatedLine = csvLine.Replace("|zone_source/loc/" + _mapName + ".zone" + "|csv", "|zone_source/loc/" + newMapName + ".zone" + "|csv");
                                }
                                newLines.Add(updatedLine);
                                if (!csvLine.Equals(updatedLine))
                                {
                                    changed = true;
                                    AppendColoredParts(("[ZONE_SOURCE, " + lang + ", ASSETINFO] ", Color.DarkMagenta), (csvLine, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                                }
                            }
                            if (changed)
                            {
                                try
                                {
                                    await Task.Run(() => File.WriteAllLines(assetInfoPath, newLines));
                                    AppendColoredTextSafe("[ZONE_SOURCE, " + lang + ", ASSETINFO] File updated: " + assetInfoPath + "\n", Color.Blue);
                                    // rename file itself
                                    string newAssetInfoPath = Path.Combine(Path.GetDirectoryName(assetInfoPath), newMapName + ".csv");
                                    try
                                    {
                                        if (!File.Exists(newAssetInfoPath)) File.Move(assetInfoPath, newAssetInfoPath);
                                        else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetInfoPath + "\n", Color.Orange);
                                        await Task.Run(() => RenameFilesInDirectoryRecursively(assetInfoDir, _mapName, newMapName));
                                    }
                                    catch (Exception ex)
                                    {
                                        AppendColoredTextSafe("[ASSETINFO RENAME ERROR] " + ex.Message + "\n", Color.Red);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    AppendColoredTextSafe("[ZONE_SOURCE, " + lang + ", ASSETINFO WRITE ERROR] " + ex.Message + "\n", Color.Red);
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        private async Task<List<string>> RenameZoneLocOccurrences()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string locZoneFilePath = _zoneSourceDirectoryPath + "loc" + Path.DirectorySeparatorChar + _mapName + ".zone";
            if (!File.Exists(locZoneFilePath)) return null;
            string[] lines = await Task.Run(() => File.ReadAllLines(locZoneFilePath));
            List<string> newLines = new List<string>();
            foreach (string line in lines)
            {
                string updatedLine = line;
                if (line.Equals(">level.xpak_write," + _mapName))
                {
                    updatedLine = ">level.xpak_write," + newMapName;
                }
                else if (line.Equals(">level.xpak_read," + _mapName))
                {
                    updatedLine = ">level.xpak_read," + newMapName;
                }
                else if (line.Equals("sound," + _mapName))
                {
                    updatedLine = "sound," + newMapName;
                }
                newLines.Add(updatedLine);
                if (!line.Equals(updatedLine))
                    AppendColoredTextSafe("[ZONE_SOURCE, LOC, ZONE] " + line + " -> " + updatedLine + "\n", Color.Purple);
            }

            try
            {
                await Task.Run(() => File.WriteAllLines(locZoneFilePath, newLines));
                AppendColoredTextSafe("[ZONE_SOURCE, LOC] File updated: " + locZoneFilePath + "\n", Color.Blue);
                // rename loc zone file itself
                string newLocZonePath = Path.Combine(Path.GetDirectoryName(locZoneFilePath), newMapName + ".zone");
                try
                {
                    if (!File.Exists(newLocZonePath)) File.Move(locZoneFilePath, newLocZonePath);
                    else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newLocZonePath + "\n", Color.Orange);
                    await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(locZoneFilePath), _mapName, newMapName));
                }
                catch (Exception ex)
                {
                    AppendColoredTextSafe("[LOC RENAME ERROR] " + ex.Message + "\n", Color.Red);
                }
            }
            catch (Exception ex)
            {
                AppendColoredTextSafe("[ZONE_SOURCE, LOC WRITE ERROR] " + ex.Message + "\n", Color.Red);
            }

            return newLines;
        }

        private async Task<List<string>> RenameAllLangOccurrences()
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string assetListCSVFilePath = _zoneSourceDirectoryPath + AllLanguage + Path.DirectorySeparatorChar + "assetlist" + Path.DirectorySeparatorChar + _mapName + ".csv";
            string assetInfoCSVFilePath = _zoneSourceDirectoryPath + AllLanguage + Path.DirectorySeparatorChar + "assetinfo" + Path.DirectorySeparatorChar + _mapName + ".csv";
            if (File.Exists(assetListCSVFilePath))
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(assetListCSVFilePath));
                List<string> newLines = new List<string>();
                bool changed = false;
                foreach (string line in lines)
                {
                    string updatedLine = line;
                    if (line.Equals("image," + _mapName + "_BC3_SRGB_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC3_SRGB_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC4_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC4_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC7_SRGB_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC7_SRGB_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC7_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC7_combo";
                    }
                    else if (line.Equals("sound," + _mapName))
                    {
                        updatedLine = "sound," + newMapName;
                        // col_map, com_map, game_map, map_ents, gfx_map 
                    }
                    else if (line.EndsWith(",maps/zm/" + _mapName + ".d3dbsp"))
                    {
                        updatedLine = line.Replace("/" + _mapName + ".d3dbsp", "/" + newMapName + ".d3dbsp");
                    }
                    else if (line.Equals("scriptparsetree,scripts/zm/" + _mapName + ".csc"))
                    {
                        updatedLine = "scriptparsetree,scripts/zm/" + newMapName + ".csc";
                    }
                    else if (line.Equals("scriptparsetree,scripts/zm/" + _mapName + ".gsc"))
                    {
                        updatedLine = "scriptparsetree/scripts/zm/" + newMapName + ".gsc";
                    }
                    else if (line.Equals("keyvaluepairs," + _mapName))
                    {
                        updatedLine = "keyvaluepairs," + newMapName;
                    }
                    else if (line.Equals("texturecombo," + _mapName))
                    {
                        updatedLine = "texturecombo," + newMapName;
                    }
                    else if (line.Equals("navmesh,maps/zm/" + _mapName + "_navmesh"))
                    {
                        updatedLine = "navmesh,maps/zm/" + newMapName + "_navmesh";
                    }
                    newLines.Add(updatedLine);
                    if (!line.Equals(updatedLine))
                    {
                        changed = true;
                        AppendColoredParts(("[ZONE_SOURCE, ALL, ASSETLIST] ", Color.DarkCyan), (line, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                    }
                }

                if (changed)
                {
                    try
                    {
                        await Task.Run(() => File.WriteAllLines(assetListCSVFilePath, newLines));
                        AppendColoredTextSafe("[ZONE_SOURCE, ALL, ASSETLIST] File updated: " + assetListCSVFilePath + "\n", Color.Blue);
                        string newAssetListPath = Path.Combine(Path.GetDirectoryName(assetListCSVFilePath), newMapName + ".csv");
                        try
                        {
                            if (!File.Exists(newAssetListPath)) File.Move(assetListCSVFilePath, newAssetListPath);
                            else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetListPath + "\n", Color.Orange);
                            await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(assetListCSVFilePath), _mapName, newMapName));
                        }
                        catch (Exception ex)
                        {
                            AppendColoredTextSafe("[ALL ASSETLIST RENAME ERROR] " + ex.Message + "\n", Color.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendColoredTextSafe("[ZONE_SOURCE, ALL, ASSETLIST WRITE ERROR] " + ex.Message + "\n", Color.Red);
                    }
                }

            }
            if (File.Exists(assetInfoCSVFilePath))
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(assetInfoCSVFilePath));
                List<string> newLines = new List<string>();
                bool changed = false;
                foreach (string csvLine in lines)
                {
                    string updatedLine = csvLine;
                    if (csvLine.EndsWith("|csv") && csvLine.Contains("|zone_source/" + _mapName + ".zone"))
                    {
                        updatedLine = csvLine.Replace("|zone_source/" + _mapName + ".zone|csv", "|zone_source/" + newMapName + ".zone|csv");
                    }
                    if (csvLine.Contains("maps/zm/" + _mapName + ".d3dbsp"))
                    {
                        updatedLine = updatedLine.Replace("maps/zm/" + _mapName + ".d3dbsp", "maps/zm/" + newMapName + ".d3dbsp");
                    }
                    if (csvLine.Contains("|" + _mapName + "|texturecombo"))
                    {
                        updatedLine = updatedLine.Replace("|" + _mapName + "|texturecombo", "|" + newMapName + "|texturecombo");
                    }
                    if (csvLine.Contains("image," + _mapName + "_BC"))
                    {
                        updatedLine = updatedLine.Replace("image," + _mapName + "_BC", "image," + newMapName + "_BC");
                    }
                    if (csvLine.Contains("texturecombo," + _mapName + ","))
                    {
                        updatedLine = updatedLine.Replace("texturecombo," + _mapName + ",", "texturecombo," + newMapName + ",");
                    }
                    newLines.Add(updatedLine);
                    if (!csvLine.Equals(updatedLine))
                    {
                        changed = true;
                        AppendColoredParts(("[ZONE_SOURCE, ALL, ASSETINFO] ", Color.DarkMagenta), (csvLine, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                    }
                }
                if (changed)
                {
                    try
                    {
                        await Task.Run(() => File.WriteAllLines(assetInfoCSVFilePath, newLines));
                        AppendColoredTextSafe("[ZONE_SOURCE, ALL, ASSETINFO] File updated: " + assetInfoCSVFilePath + "\n", Color.Blue);
                        string newAssetInfoPath = Path.Combine(Path.GetDirectoryName(assetInfoCSVFilePath), newMapName + ".csv");
                        try
                        {
                            if (!File.Exists(newAssetInfoPath)) File.Move(assetInfoCSVFilePath, newAssetInfoPath);
                            else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetInfoPath + "\n", Color.Orange);
                            await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(assetInfoCSVFilePath), _mapName, newMapName));
                        }
                        catch (Exception ex)
                        {
                            AppendColoredTextSafe("[ALL ASSETINFO RENAME ERROR] " + ex.Message + "\n", Color.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendColoredTextSafe("[ZONE_SOURCE, ALL, ASSETINFO WRITE ERROR] " + ex.Message + "\n", Color.Red);
                    }
                }
            }
            return null;
        }

        private async Task<List<string>> RenameLangOccurrences(string lang)
        {
            string newMapName = MapNamePrefix + newNameTextBox.Text;
            string assetListCSVFilePath = _zoneSourceDirectoryPath + lang + Path.DirectorySeparatorChar + "assetlist" + Path.DirectorySeparatorChar + _mapName + ".csv";
            string assetInfoCSVFilePath = _zoneSourceDirectoryPath + lang + Path.DirectorySeparatorChar + "assetinfo" + Path.DirectorySeparatorChar + _mapName + ".csv";
            if (File.Exists(assetListCSVFilePath))
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(assetListCSVFilePath));
                List<string> newLines = new List<string>();
                bool changed = false;
                foreach (string line in lines)
                {
                    string updatedLine = line;
                    if (line.Equals("image," + _mapName + "_BC3_SRGB_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC3_SRGB_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC4_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC4_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC7_SRGB_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC7_SRGB_combo";
                    }
                    else if (line.Equals("image," + _mapName + "_BC7_combo"))
                    {
                        updatedLine = "image," + newMapName + "_BC7_combo";
                    }
                    else if (line.Equals("sound," + _mapName))
                    {
                        updatedLine = "sound," + newMapName;
                        // col_map, com_map, game_map, map_ents, gfx_map 
                    }
                    else if (line.EndsWith(",maps/zm/" + _mapName + ".d3dbsp"))
                    {
                        updatedLine = line.Replace("/" + _mapName + ".d3dbsp", "/" + newMapName + ".d3dbsp");
                    }
                    else if (line.Equals("scriptparsetree,scripts/zm/" + _mapName + ".csc"))
                    {
                        updatedLine = "scriptparsetree,scripts/zm/" + newMapName + ".csc";
                    }
                    else if (line.Equals("scriptparsetree,scripts/zm/" + _mapName + ".gsc"))
                    {
                        updatedLine = "scriptparsetree/scripts/zm/" + newMapName + ".gsc";
                    }
                    else if (line.Equals("keyvaluepairs," + _mapName))
                    {
                        updatedLine = "keyvaluepairs," + newMapName;
                    }
                    else if (line.Equals("texturecombo," + _mapName))
                    {
                        updatedLine = "texturecombo," + newMapName;
                    }
                    else if (line.Equals("navmesh,maps/zm/" + _mapName + "_navmesh"))
                    {
                        updatedLine = "navmesh,maps/zm/" + newMapName + "_navmesh";
                    }
                    newLines.Add(updatedLine);
                    if (!line.Equals(updatedLine))
                    {
                        changed = true;
                        AppendColoredParts(("[ZONE_SOURCE, " + lang.ToUpper() + ", ASSETLIST] ", Color.DarkCyan), (line, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                    }
                }

                if (changed)
                {
                    try
                    {
                        await Task.Run(() => File.WriteAllLines(assetListCSVFilePath, newLines));
                        AppendColoredTextSafe("[ZONE_SOURCE, " + lang.ToUpper() + ", ASSETLIST] File updated: " + assetListCSVFilePath + "\n", Color.Blue);
                        string newAssetListPath = Path.Combine(Path.GetDirectoryName(assetListCSVFilePath), newMapName + ".csv");
                        try
                        {
                            if (!File.Exists(newAssetListPath)) File.Move(assetListCSVFilePath, newAssetListPath);
                            else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetListPath + "\n", Color.Orange);
                            await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(assetListCSVFilePath), _mapName, newMapName));
                        }
                        catch (Exception ex)
                        {
                            AppendColoredTextSafe("[ASSETLIST RENAME ERROR] " + ex.Message + "\n", Color.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendColoredTextSafe("[ZONE_SOURCE, " + lang.ToUpper() + ", ASSETLIST WRITE ERROR] " + ex.Message + "\n", Color.Red);
                    }
                }

            }
            if (File.Exists(assetInfoCSVFilePath))
            {
                string[] lines = await Task.Run(() => File.ReadAllLines(assetInfoCSVFilePath));
                List<string> newLines = new List<string>();
                bool changed = false;
                foreach (string csvLine in lines)
                {
                    string updatedLine = csvLine;
                    if (csvLine.EndsWith("|csv") && csvLine.Contains("|zone_source/" + _mapName + ".zone"))
                    {
                        updatedLine = csvLine.Replace("|zone_source/" + _mapName + ".zone|csv", "|zone_source/" + newMapName + ".zone|csv");
                    }
                    if (csvLine.Contains("maps/zm/" + _mapName + ".d3dbsp"))
                    {
                        updatedLine = updatedLine.Replace("maps/zm/" + _mapName + ".d3dbsp", "maps/zm/" + newMapName + ".d3dbsp");
                    }
                    if (csvLine.Contains("|" + _mapName + "|texturecombo"))
                    {
                        updatedLine = updatedLine.Replace("|" + _mapName + "|texturecombo", "|" + newMapName + "|texturecombo");
                    }
                    if (csvLine.Contains("image," + _mapName + "_BC"))
                    {
                        updatedLine = updatedLine.Replace("image," + _mapName + "_BC", "image," + newMapName + "_BC");
                    }
                    if (csvLine.Contains("texturecombo," + _mapName + ","))
                    {
                        updatedLine = updatedLine.Replace("texturecombo," + _mapName + ",", "texturecombo," + newMapName + ",");
                    }
                    newLines.Add(updatedLine);
                    if (!csvLine.Equals(updatedLine))
                    {
                        changed = true;
                        AppendColoredParts(("[ZONE_SOURCE, " + lang.ToUpper() + ", ASSETINFO] ", Color.DarkMagenta), (csvLine, Color.Maroon), (" -> ", Color.Black), (updatedLine + "\n", Color.DarkGreen));
                    }
                }
                if (changed)
                {
                    try
                    {
                        await Task.Run(() => File.WriteAllLines(assetInfoCSVFilePath, newLines));
                        AppendColoredTextSafe("[ZONE_SOURCE, " + lang.ToUpper() + ", ASSETINFO] File updated: " + assetInfoCSVFilePath + "\n", Color.Blue);
                        string newAssetInfoPath = Path.Combine(Path.GetDirectoryName(assetInfoCSVFilePath), newMapName + ".csv");
                        try
                        {
                            if (!File.Exists(newAssetInfoPath)) File.Move(assetInfoCSVFilePath, newAssetInfoPath);
                            else AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newAssetInfoPath + "\n", Color.Orange);
                            await Task.Run(() => RenameFilesInDirectoryRecursively(Path.GetDirectoryName(assetInfoCSVFilePath), _mapName, newMapName));
                        }
                        catch (Exception ex)
                        {
                            AppendColoredTextSafe("[ASSETINFO RENAME ERROR] " + ex.Message + "\n", Color.Red);
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendColoredTextSafe("[ZONE_SOURCE, ALL, ASSETINFO WRITE ERROR] " + ex.Message + "\n", Color.Red);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a backup folder in the program's directory named "{mapName} backup" (timestamp appended if needed).
        /// Copies either the whole map folder or only the zone_source folder.
        /// Returns the path to the created backup directory or null on failure.
        /// </summary>
        private async Task<string> CreateBackupFolder(bool copyEntireMap = true)
        {
            if (string.IsNullOrEmpty(_mapName) || string.IsNullOrEmpty(_mapDir))
            {
                MessageBox.Show("No map selected to backup.", "Backup", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }

            string programPath = Application.StartupPath;
            string baseFolderName = _mapName + " backup";
            string backupRoot = Path.Combine(programPath, baseFolderName);

            // If folder exists, append timestamp to ensure uniqueness
            if (Directory.Exists(backupRoot))
            {
                string timeSuffix = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                backupRoot = Path.Combine(programPath, $"{baseFolderName} ({timeSuffix})");
            }

            try
            {
                Directory.CreateDirectory(backupRoot);

                if (copyEntireMap)
                {
                    string destMapFolder = Path.Combine(backupRoot, _mapName);
                    await DirectoryCopy(_mapDir, destMapFolder, true);
                    AppendColoredTextSafe("[BACKUP] Entire map copied to: " + destMapFolder + Environment.NewLine, Color.Blue);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_zoneSourceDirectoryPath) && Directory.Exists(_zoneSourceDirectoryPath))
                    {
                        string destZoneSource = Path.Combine(backupRoot, "zone_source");
                        await DirectoryCopy(_zoneSourceDirectoryPath.TrimEnd(Path.DirectorySeparatorChar), destZoneSource, true);
                        AppendColoredTextSafe("[BACKUP] zone_source copied to: " + destZoneSource + Environment.NewLine, Color.Blue);
                    }
                    else
                    {
                        AppendColoredTextSafe("[BACKUP] zone_source not found; no files copied." + Environment.NewLine, Color.Orange);
                    }
                }

                AppendColoredTextSafe("[BACKUP] Backup created at: " + backupRoot + Environment.NewLine, Color.Blue);
                return backupRoot;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create backup: " + ex.Message, "Backup error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AppendColoredTextSafe("[BACKUP ERROR] " + ex.Message + Environment.NewLine, Color.Red);
                return null;
            }
        }

        /// <summary>
        /// Recursively copies a directory (sourceDirName) to destination (destDirName) using Task.Run for IO.
        /// Overwrites files if they exist. Logs per-file errors to detailsRichTextBox.
        /// </summary>
        private async Task DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            await Task.Run(() =>
            {
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }

                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    try
                    {
                        file.CopyTo(temppath, true);
                    }
                    catch (Exception ex)
                    {
                        // Log and continue with other files
                        this.BeginInvoke((Action)(() => AppendColoredTextSafe("[BACKUP FILE ERROR] " + file.FullName + " -> " + temppath + " : " + ex.Message + Environment.NewLine, Color.Red)));
                    }
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        try
                        {
                            // Recursive call
                            DirectoryCopy(subdir.FullName, temppath, true).Wait();
                        }
                        catch (Exception ex)
                        {
                            // Log and continue with other directories
                            this.BeginInvoke((Action)(() => AppendColoredTextSafe("[BACKUP DIR ERROR] " + subdir.FullName + " -> " + temppath + " : " + ex.Message + Environment.NewLine, Color.Red)));
                        }
                    }
                }
            });
        }

        /// <summary>
        /// Replace all files under directory recursively that contain oldName in filename with newName.
        /// </summary>
        private void RenameFilesInDirectoryRecursively(string startDir, string oldName, string newName)
        {
            if (!Directory.Exists(startDir)) return;
            foreach (string file in Directory.GetFiles(startDir))
            {
                try
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName.IndexOf(oldName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        string newFileName = ReplaceIgnoreCaseAll(fileName, oldName, newName);
                        string newPath = Path.Combine(Path.GetDirectoryName(file), newFileName);
                        if (!File.Exists(newPath))
                        {
                            File.Move(file, newPath);
                            this.BeginInvoke((Action)(() => AppendColoredTextSafe("[RENAME] " + file + " -> " + newPath + "\n", Color.Green)));
                        }
                        else
                        {
                            this.BeginInvoke((Action)(() => AppendColoredTextSafe("[RENAME SKIP] Target exists: " + newPath + "\n", Color.Orange)));
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.BeginInvoke((Action)(() => AppendColoredTextSafe("[RENAME ERROR] " + file + " : " + ex.Message + "\n", Color.Red)));
                }
            }
            foreach (string dir in Directory.GetDirectories(startDir))
            {
                RenameFilesInDirectoryRecursively(dir, oldName, newName);
            }
        }

        private static string ReplaceIgnoreCaseAll(string input, string oldValue, string newValue)
        {
            if (string.IsNullOrEmpty(oldValue)) return input;
            string result = input;
            int idx = result.IndexOf(oldValue, StringComparison.OrdinalIgnoreCase);
            while (idx >= 0)
            {
                result = result.Substring(0, idx) + newValue + result.Substring(idx + oldValue.Length);
                idx = result.IndexOf(oldValue, idx + newValue.Length, StringComparison.OrdinalIgnoreCase);
            }
            return result;
        }

        private void AppendColoredTextSafe(string text, Color color)
        {
            if (detailsRichTextBox.InvokeRequired)
            {
                detailsRichTextBox.BeginInvoke((Action)(() => AppendColoredText(text, color)));
            }
            else
            {
                AppendColoredText(text, color);
            }
        }

        /// <summary>
        /// Append multiple colored segments in a single call. Each segment is a (text, color) tuple.
        /// </summary>
        private void AppendColoredParts(params (string text, Color color)[] parts)
        {
            if (parts == null || parts.Length == 0) return;
            if (detailsRichTextBox.InvokeRequired)
            {
                var p = parts;
                detailsRichTextBox.BeginInvoke((Action)(() => AppendColoredParts(p)));
                return;
            }
            Color prev = detailsRichTextBox.SelectionColor;
            foreach (var part in parts)
            {
                detailsRichTextBox.SelectionStart = detailsRichTextBox.TextLength;
                detailsRichTextBox.SelectionLength = 0;
                detailsRichTextBox.SelectionColor = part.color;
                detailsRichTextBox.AppendText(part.text);
            }
            detailsRichTextBox.SelectionColor = prev;
        }

        private void AppendColoredText(string text, Color color)
        {
            Color prev = detailsRichTextBox.SelectionColor;
            detailsRichTextBox.SelectionStart = detailsRichTextBox.TextLength;
            detailsRichTextBox.SelectionLength = 0;
            detailsRichTextBox.SelectionColor = color;
            detailsRichTextBox.AppendText(text);
            detailsRichTextBox.SelectionColor = prev;
        }

        private bool StartsWithAny(string source, params string[] strs)
        {
            if (strs.Length == 0) return false;
            foreach (string str in strs)
            {
                if (source.StartsWith(str)) return true;
            }
            return false;
        }

        private bool EndsWithAny(string source, params string[] strs)
        {
            if (strs.Length == 0) return false;
            foreach (string str in strs)
            {
                if (source.EndsWith(str)) return true;
            }
            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Reload settings first so we read the latest persisted values
            Properties.Settings.Default.Reload();

            var usermapFolder = Properties.Settings.Default.usermap_folder;
            // Diagnostic logging
            AppendColoredParts(("[DIAG] ", Color.Gray), ("usermap_folder=", Color.DarkGray), (usermapFolder ?? "(null)", Color.Maroon), ("\n", Color.Black));
            AppendColoredParts(("[DIAG] ", Color.Gray), ("IsAdmin=", Color.DarkGray), (IsAdministrator().ToString(), Color.Maroon), ("\n", Color.Black));

            if (!string.IsNullOrEmpty(usermapFolder) && Directory.Exists(usermapFolder))
            {
                folderBrowserDialog1.SelectedPath = usermapFolder;
            }
            else
            {
                // Try to discover Steam library path (robust across platform/bitness)
                string libraryPath = FindSteamLibraryPathWithWorkshopContent();
                AppendColoredParts(("[DIAG] ", Color.Gray), ("discoveredSteamLibrary=", Color.DarkGray), (libraryPath ?? "(null)", Color.Maroon), ("\n", Color.Black));
                if (!string.IsNullOrEmpty(libraryPath) && Directory.Exists(libraryPath))
                {
                    folderBrowserDialog1.SelectedPath = libraryPath;
                }
            }
        }

        private void newNameTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void mapFolderLabel_Click(object sender, EventArgs e)
        {

        }

        private void mapFolderLabel_DoubleClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.usermap_folder = null;
            Properties.Settings.Default.Save();
        }
    }
}
