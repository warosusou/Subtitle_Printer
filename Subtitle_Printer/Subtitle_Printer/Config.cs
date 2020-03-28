using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using static Subtitle_Printer.SubtitleDrawer;
using FontFamily = System.Windows.Media.FontFamily;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Subtitle_Printer
{
    class Config
    {
        public FontFamily EditorFont { get; set; }
        public double EditorFontSize { get; set; }
        public Font PrintingFont { get; set; }
        public double EQSize { get; set; }
        public bool AutoShrink { get; set; }
        public Alignment Alignment { get; set; }
        public SizeF ImageResolution { get; set; }

        [JsonIgnore]
        public bool ConfigAutoSave
        {
            get { return autoSave; }
            set
            {
                autoSaveTimer.Enabled = value;
                autoSave = value;
            }
        }
        [JsonIgnore]
        public string ConfigSavePath { get; set; }
        private bool autoSave;
        private Timer autoSaveTimer = new Timer { Interval = 10000 };
        private Config oldConfig;

        public Config()
        {
            oldConfig = this;
            autoSaveTimer.Tick += AutoSaveTimer_Tick;
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            autoSaveTimer.Enabled = false;
            if (this != oldConfig && ConfigSavePath != "")
            {
                SaveConfig(ConfigSavePath, this);
                oldConfig = this;
            }
            autoSaveTimer.Enabled = true;
        }

        public static void SaveConfig(string path, Config config)
        {
            var jsonString = JsonConvert.SerializeObject(config);
            try
            {
                File.WriteAllText(path, jsonString);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public static Config LoadConfig(string path)
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<Config>(jsonString);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
