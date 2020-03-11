using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;

namespace RedditFeed
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonSettings
    {
        public string FilePath { get; set; }

        #region JSON PROPERTIES
        [JsonProperty("preferences", Order = 2)]
        public FeedPreferences Preferences { get; set; }

        [JsonProperty("$schema", Order = 1)]
        public Uri Schema { get; set; } = new Uri(Resources.SchemaUrl, UriKind.Absolute);

        public JsonSettings() { }
        internal JsonSettings(bool loading)
        {
            bool exists = this.TryGetFile(out string filePath);
            this.FilePath = filePath;
            if (!exists)
            {
                this.Preferences = new FeedPreferences();
                if (this.TryCreateFolders())
                {
                    this.Save();
                    this.Read();
                }
            }
            else
            {
                this.Read();
            }
        }

        private string GetCombinedPath()
        {
            string roaming = this.GetRoamingPath();
            return string.Format("{0}\\{1}", roaming, Resources.DefaultSettingsPath);
        }
        private string GetRoamingPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        public void Read()
        {
            if (string.IsNullOrEmpty(this.FilePath))
                throw new ArgumentNullException("FilePath");

            using (var reader = new StreamReader(File.OpenRead(this.FilePath), Encoding.UTF8))
            {
                string rawJson = reader.ReadToEnd();

                JsonConvert.PopulateObject(rawJson, this);
            }
        }
        public void Save()
        {
            using (StreamWriter streamWriter = File.CreateText(this.FilePath))
            {
                var serializer = new JsonSerializer
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Include
                };
                serializer.Serialize(streamWriter, this);
            }
        }
        public void SaveSubreddit(string newSub, bool isLoading)
        {
            if (newSub != this.Preferences.Subreddit && !isLoading)
            {
                this.Preferences.Subreddit = newSub;
                this.Save();
            }
        }
        private bool TryCreateFolder(string path, out Exception e)
        {
            e = null;
            bool result = false;
            try
            {
                Directory.CreateDirectory(path);
                result = Directory.Exists(path);
            }
            catch (Exception caught)
            {
                e = caught;
            }
            return result;
        }
        private bool TryCreateFolders()
        {
            string roaming = this.GetRoamingPath();
            string[] restOfPath = Path
                .GetDirectoryName(Resources.DefaultSettingsPath)
                    .Split(@"\", StringSplitOptions.RemoveEmptyEntries);
            List<string> list = new List<string>(restOfPath);

            for (int i = 0; i < list.Count; i++)
            {
                string toCreate = list[i];
                if (i > 0)
                    toCreate = string.Join(@"\", list.GetRange(0, i+1));

                toCreate = Path.Combine(roaming, toCreate);

                if (!Directory.Exists(toCreate) && !this.TryCreateFolder(toCreate, out Exception ex))
                {
                    return false;
                }
            }
            return true;
        }
        private bool TryGetFile(out string filePath)
        {
            filePath = this.GetCombinedPath();
            return File.Exists(filePath);
        }

        #endregion
    }
}