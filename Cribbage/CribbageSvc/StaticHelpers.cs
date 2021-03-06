﻿using CribbageService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Cribbage
{
    public enum AnimationSpeedSetting { Fast, Regular, Slow };

    public class AnimationSpeedsClass
    {
        public double VerySlow { get; set; }
        public double Slow { get; set; }
        public double Medium { get; set; }
        public double Fast { get; set; }
        public double VeryFast { get; set; }

        public double NoAnimation { get; set; }

        public double DefaultFlipSpeed { get; set; }

        private AnimationSpeedSetting _speed;
         
        public AnimationSpeedsClass(AnimationSpeedSetting speed)
        {

            _speed = speed;
            SetSpeedValues();
            NoAnimation = 1;
        }

        public AnimationSpeedSetting AnimationSpeed
        {
            get
            {
                return _speed;
            }
            set
            {
                _speed = value;
                SetSpeedValues();
            }

        }

        private void SetSpeedValues()
        {
            double normal = 250;
            if (_speed == AnimationSpeedSetting.Fast)
                normal = 50;
            else if (_speed == AnimationSpeedSetting.Slow)
                normal = 500;

            VerySlow = normal * 20;
            Slow = normal * 4;
            Medium = normal * 2;
            Fast = normal *.75;
            VeryFast = normal * .5;

            DefaultFlipSpeed = Fast;
        }
    }

    public static class StaticHelpers
    {

        public static Deck gs_Deck = null;


       

        public class KeyValuePair
        {
            public string Key { get; set; }
            public string Value { get; set; }

            public KeyValuePair(string key, string value)
            {
                Key = key;
                Value = value;
            }

        }


        public static string SerializeFromList(List<CardView> cards)
        {
            string s = "";
            foreach (CardView card in cards)
            {
                s += card.Serialize();
                s += ",";
            }
            return s;
        }

        public static List<CardView> DeserializeToList(string s)
        {

            return DeserializeToList(s, gs_Deck);
        }

        public static List<CardView> DeserializeToList(string s, Deck deck)
        {

            List<CardView> cards = new List<CardView>();
            char[] sep = { ',' };
            string[] values = s.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            foreach (string c in values)
            {
                CardView card = CardFromString(c, deck);                
                cards.Add(card);
            }
            return cards;
        }

        /// <summary>
        ///     comes in the form of "AceOfSpaces\FaceDown
        /// </summary>
        /// <param name="s"></param>
        /// <param name="deck"></param>
        /// <returns></returns>
        public static CardView CardFromString(string s, Deck deck)
        {
            List<CardView> cards = new List<CardView>();
            char[] sep = { '\\' };
            string[] values = s.Split(sep, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (CardView card in deck.Cards)
            {
                if (card.Data.Name.ToString() == values[0])
                {
                   CardOrientation orientation = (CardOrientation)Enum.Parse(typeof(CardOrientation), values[2]);                   
                    card.SetOrientationAsync(orientation);

                    Owner owner = (Owner)Enum.Parse(typeof(Owner), values[1]);
                    card.Owner = owner;
                    return card;         
                }

            }
            return null;
        }

        public static string GetValue(string s, char sep = '=')
        {
            string[] values = s.Split(sep);
            return values[1];
        }

        public static bool GetBoolValue(string s, char sep = '=')
        {
            string[] values = s.Split(sep);
            return Convert.ToBoolean(values[1]);

        }

        public static int GetIntValue(string s, char sep = '=')
        {
            string[] values = s.Split(sep);
            return Convert.ToInt32(values[1]);
        }

        public static string SetValue(string name, object value)
        {
            return String.Format("{0}={1}\n", name, value);

        }

        public static KeyValuePair GetKeyValue(string s, char sep = '=')
        {

            string[] tokens = s.Split(sep);
            KeyValuePair kvp = new KeyValuePair(tokens[0], tokens[1]);
            
            return kvp;
        }

        public static Dictionary<string, string> GetSections(string file)
        {
            char[] sep1 = new char[] { '['};
            char[] sep2 = new char[] { ']' };

            string[] tokens = file.Split(sep1, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> sections = new Dictionary<string, string>();
            foreach (string s in tokens)
            {
                string[] tok1 = s.Split(sep2, StringSplitOptions.RemoveEmptyEntries);
                sections.Add(tok1[0], tok1[1]);                

            }

            return sections;
        }

        public static string SerializeDictionary(Dictionary<string, string> dictionary, string seperator = "\n")
        {
            string ret = "";

            foreach (KeyValuePair<string, string> kvp in dictionary)
            {
                ret += String.Format("{0}={1}{2}", kvp.Key, kvp.Value, seperator);

            }

            return ret;

        }

        
        public static Dictionary<string, string> DeserializeDictionary(string section, char lineSeperator = '\n')
        {
            if (section.Length < 20)
                return null;
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            char[] sep1 = new char[] { lineSeperator };
            char[] sep2 = new char[] { '=' };

            string[] tokens = section.Split(sep1, StringSplitOptions.RemoveEmptyEntries);            
            foreach (string s in tokens)
            {
                string[] pairs = s.Split(sep2, StringSplitOptions.RemoveEmptyEntries);
                if (pairs.Count() == 2)
                {                    
                    dictionary.Add(pairs[0], pairs[1]);
                }
                if (pairs.Count() == 1)
                {

                    dictionary.Add(pairs[0], "");
                }

            }


            return dictionary;
        }

        public static Dictionary<string, string> GetSection(string file, string section)
        {
            Dictionary<string, string> sections = StaticHelpers.GetSections(file);
            if (sections == null)
                return null;

            return StaticHelpers.DeserializeDictionary(sections[section]);

        }

        public static bool RemoveCardByValueFromList(IList<CardView> list, CardView card)
        {            
            for (int i = list.Count - 1; i >= 0; i--)
            {                
                if (list[i].Index == card.Index)
                {
                    list.RemoveAt(i);
                    return true;
                }

            }
            return false;

        }

      
        static public async Task RunStoryBoard(Storyboard sb, bool callStop = true, double ms = 500, bool setTimeout = true)
        {
            if (setTimeout)
            {
                foreach (var animations in sb.Children)
                {
                    animations.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                }
            }

            var tcs = new TaskCompletionSource<object>();
            EventHandler<object> completed = (s, e) => tcs.TrySetResult(null);
            try
            {
                sb.Completed += completed;
                sb.Begin();
                await tcs.Task;
            }
            finally
            {
                sb.Completed -= completed; 
                if (callStop) sb.Stop();
            }

        }

        public static List<T> DestructiveIterator<T>(this List<T> list)
        {
            List<T> copy = new List<T>(list);
            return copy;

        }

        public static Task<object> ToTask(this Storyboard storyboard, CancellationTokenSource cancellationTokenSource = null)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(TaskCreationOptions.AttachedToParent);

            if (cancellationTokenSource != null)
            {
                // when the task is cancelled, 
                // Stop the storyboard
                cancellationTokenSource.Token.Register
                (
                    () =>
                    {
                        storyboard.Stop();
                    }
                );
            }

            EventHandler<object> onCompleted = null;

            onCompleted = (s, e) =>
            {
                storyboard.Completed -= onCompleted;

                tcs.SetResult(null);
            };

            storyboard.Completed += onCompleted;

            // start the storyboard during the conversion.
            storyboard.Begin();

            return tcs.Task;
        }  

        static public void RunStoryBoardAsync(Storyboard sb, double ms = 500, bool setTimeout = true)
        {
            if (setTimeout)
            {
                foreach (var animations in sb.Children)
                {
                    animations.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
                }
            }
           
            sb.Begin();                           
        }

        static public void SetFlipAnimationSpeed(Storyboard sb, double milliseconds)
        {

            foreach (var animation in sb.Children)
            {
                if (animation.Duration != TimeSpan.FromMilliseconds(0))
                {
                    animation.Duration = TimeSpan.FromMilliseconds(milliseconds);
                }

                if (animation.BeginTime != TimeSpan.FromMilliseconds(0))
                {
                    animation.BeginTime = TimeSpan.FromMilliseconds(milliseconds);
                }

            }
        }


        internal static string CardListToNames(IEnumerable<CardView> cards)
        {
            string s = "";
            foreach (CardView card in cards)
            {
                s += card.CardName;
                s += ",";
            }
            return s;
        }

    }

    public static class StorageHelper
    {
        #region Settings

        /// <summary>Returns if a setting is found in the specified storage strategy</summary>
        /// <param name="key">Path of the setting in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        public static bool SettingExists(string key, StorageStrategies location = StorageStrategies.Local)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    return Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
                case StorageStrategies.Roaming:
                    return Windows.Storage.ApplicationData.Current.RoamingSettings.Values.ContainsKey(key);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        /// <summary>Reads and converts a setting into specified type T</summary>
        /// <typeparam name="T">Specified type into which to value is converted</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="otherwise">Return value if key is not found or convert fails</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Specified type T</returns>
        public static T GetSetting<T>(string key, T otherwise = default(T), StorageStrategies location = StorageStrategies.Local)
        {
            try
            {
                if (!(SettingExists(key, location)))
                    return otherwise;
                switch (location)
                {
                    case StorageStrategies.Local:
                        return (T)Windows.Storage.ApplicationData.Current.LocalSettings.Values[key.ToString()];
                    case StorageStrategies.Roaming:
                        return (T)Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key.ToString()];
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch { /* error casting */ return otherwise; }
        }

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T">Specified type of object to serialize</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="location">Location storage strategy</param>
        public static void SetSetting<T>(string key, T value, StorageStrategies location = StorageStrategies.Local)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values[key.ToString()] = value;
                    break;
                case StorageStrategies.Roaming:
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values[key.ToString()] = value;
                    break;
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        public static void DeleteSetting(string key, StorageStrategies location = StorageStrategies.Local)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(key);
                    break;
                case StorageStrategies.Roaming:
                    Windows.Storage.ApplicationData.Current.RoamingSettings.Values.Remove(key);
                    break;
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        #endregion

        #region File

        /// <summary>Returns if a file is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Boolean: true if found, false if not found</returns>
        public static async Task<bool> FileExistsAsync(string key, StorageStrategies location = StorageStrategies.Local)
        {
            return (await GetIfFileExistsAsync(key, location)) != null;
        }

        public static async Task<bool> FileExistsAsync(string key, Windows.Storage.StorageFolder folder)
        {
            return (await GetIfFileExistsAsync(key, folder)) != null;
        }

        /// <summary>Deletes a file in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        public static async Task<bool> DeleteFileAsync(string key, StorageStrategies location = StorageStrategies.Local)
        {
            var _File = await GetIfFileExistsAsync(key, location);
            if (_File != null)
                await _File.DeleteAsync();
            return !(await FileExistsAsync(key, location));
        }

        /// <summary>Reads and deserializes a file into specified type T</summary>
        /// <typeparam name="T">Specified type into which to deserialize file content</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>Specified type T</returns>
        public static async Task<T> ReadFileAsync<T>(string key, StorageStrategies location = StorageStrategies.Local)
        {
            try
            {
                // fetch file
                var _File = await GetIfFileExistsAsync(key, location);
                if (_File == null)
                    return default(T);
                // read content
                var _String = await Windows.Storage.FileIO.ReadTextAsync(_File);
                // convert to obj
                var _Result = Deserialize<T>(_String);
                return _Result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>Serializes an object and write to file in specified storage strategy</summary>
        /// <typeparam name="T">Specified type of object to serialize</typeparam>
        /// <param name="key">Path to the file in storage</param>
        /// <param name="value">Instance of object to be serialized and written</param>
        /// <param name="location">Location storage strategy</param>
        public static async Task<bool> WriteFileAsync<T>(string key, T value, StorageStrategies location = StorageStrategies.Local)
        {
            // create file
            var _File = await CreateFileAsync(key, location, Windows.Storage.CreationCollisionOption.ReplaceExisting);
            // convert to string
            var _String = Serialize(value);
            // save string to file
            await Windows.Storage.FileIO.WriteTextAsync(_File, _String);
            // result
            return await FileExistsAsync(key, location);
        }

        private static async Task<Windows.Storage.StorageFile> CreateFileAsync(string key, StorageStrategies location = StorageStrategies.Local,
            Windows.Storage.CreationCollisionOption option = Windows.Storage.CreationCollisionOption.OpenIfExists)
        {
            switch (location)
            {
                case StorageStrategies.Local:
                    return await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileAsync(key, option);
                case StorageStrategies.Roaming:
                    return await Windows.Storage.ApplicationData.Current.RoamingFolder.CreateFileAsync(key, option);
                case StorageStrategies.Temporary:
                    return await Windows.Storage.ApplicationData.Current.TemporaryFolder.CreateFileAsync(key, option);
                default:
                    throw new NotSupportedException(location.ToString());
            }
        }

        private static async Task<Windows.Storage.StorageFile> GetIfFileExistsAsync(string key, Windows.Storage.StorageFolder folder,
            Windows.Storage.CreationCollisionOption option = Windows.Storage.CreationCollisionOption.FailIfExists)
        {
            Windows.Storage.StorageFile retval;
            try
            {
                retval = await folder.GetFileAsync(key);
            }
            catch (System.IO.FileNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine("GetIfFileExistsAsync:FileNotFoundException");
                return null;
            }
            return retval;
        }

        /// <summary>Returns a file if it is found in the specified storage strategy</summary>
        /// <param name="key">Path of the file in storage</param>
        /// <param name="location">Location storage strategy</param>
        /// <returns>StorageFile</returns>
        private static async Task<Windows.Storage.StorageFile> GetIfFileExistsAsync(string key,
            StorageStrategies location = StorageStrategies.Local,
            Windows.Storage.CreationCollisionOption option = Windows.Storage.CreationCollisionOption.FailIfExists)
        {
            Windows.Storage.StorageFile retval;
            try
            {
                switch (location)
                {
                    case StorageStrategies.Local:
                        retval = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(key);
                        break;
                    case StorageStrategies.Roaming:
                        retval = await Windows.Storage.ApplicationData.Current.RoamingFolder.GetFileAsync(key);
                        break;
                    case StorageStrategies.Temporary:
                        retval = await Windows.Storage.ApplicationData.Current.TemporaryFolder.GetFileAsync(key);
                        break;
                    default:
                        throw new NotSupportedException(location.ToString());
                }
            }
            catch (System.IO.FileNotFoundException)
            {
              
                return null;
            }

            return retval;
        }

        #endregion

        /// <summary>Serializes the specified object as a JSON string</summary>
        /// <param name="objectToSerialize">Specified object to serialize</param>
        /// <returns>JSON string of serialzied object</returns>
        public static string Serialize(object objectToSerialize)
        {
            using (System.IO.MemoryStream _Stream = new System.IO.MemoryStream())
            {
                try
                {
                    var _Serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(objectToSerialize.GetType());
                    _Serializer.WriteObject(_Stream, objectToSerialize);
                    _Stream.Position = 0;
                    System.IO.StreamReader _Reader = new System.IO.StreamReader(_Stream);
                    return _Reader.ReadToEnd();
                }
                catch (Exception)
                {
                    
                    return string.Empty;
                }
            }
        }

        /// <summary>Deserializes the JSON string as a specified object</summary>
        /// <typeparam name="T">Specified type of target object</typeparam>
        /// <param name="jsonString">JSON string source</param>
        /// <returns>Object of specied type</returns>
        public static T Deserialize<T>(string jsonString)
        {
            using (System.IO.MemoryStream _Stream = new System.IO.MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                try
                {
                    var _Serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(T));
                    return (T)_Serializer.ReadObject(_Stream);
                }
                catch (Exception) { throw; }
            }
        }

        public enum StorageStrategies
        {
            /// <summary>Local, isolated folder</summary>
            Local,
            /// <summary>Cloud, isolated folder. 100k cumulative limit.</summary>
            Roaming,
            /// <summary>Local, temporary folder (not for settings)</summary>
            Temporary
        }

        public static async Task DeleteFileFireAndForget(string key, StorageStrategies location)
        {
            await DeleteFileAsync(key, location);
        }

        public static async Task WriteFileFireAndForget<T>(string key, T value, StorageStrategies location)
        {
            await WriteFileAsync(key, value, location);
        }

        // Usage: await button1.WhenClicked();

        public static async Task WhenClicked(this Button button)
        {
            var tcs = new TaskCompletionSource<object>();
            RoutedEventHandler lambda = (s, e) => tcs.TrySetResult(null);

            try
            {
                button.Click += lambda;
                await tcs.Task;
            }
            finally
            {
                button.Click -= lambda;
            }
        }

    }
}
