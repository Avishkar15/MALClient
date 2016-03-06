﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MALClient.Comm;
using MALClient.Pages;
using MALClient.UserControls;
using MALClient.ViewModels;

namespace MALClient
{
    public static class Utils
    {
        private static readonly string[] SizeSuffixes = {"bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

        public static string StatusToString(int status)
        {
            switch (status)
            {
                case 1:
                    return "Watching";
                case 2:
                    return "Completed";
                case 3:
                    return "On hold";
                case 4:
                    return "Dropped";
                case 6:
                    return "Plan to watch";
                case 7:
                    return "All";
                case 8:
                    return "Airing";
                default:
                    return "Not Set";
            }
        }

        public static int StatusToInt(string status)
        {
            switch (status)
            {
                case "Watching":
                    return 1;
                case "Completed":
                    return 2;
                case "On hold":
                    return 3;
                case "Dropped":
                    return 4;
                case "Plan to watch":
                    return 6;
                case "All":
                    return 7;
                case "Airing":
                    return 8;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string DayToString(DayOfWeek day)
        {
            switch (day)
            {
                case DayOfWeek.Friday:
                    return "Fri";
                case DayOfWeek.Monday:
                    return "Mon";
                case DayOfWeek.Saturday:
                    return "Sat";
                case DayOfWeek.Sunday:
                    return "Sun";
                case DayOfWeek.Thursday:
                    return "Thu";
                case DayOfWeek.Tuesday:
                    return "Tue";
                case DayOfWeek.Wednesday:
                    return "Wed";
                default:
                    return "";
            }
        }

        public static void RegisterTile(string id)
        {
            var tiles = (string) ApplicationData.Current.LocalSettings.Values["tiles"];
            if (string.IsNullOrWhiteSpace(tiles))
                tiles = "";
            tiles += id + ";";
            ApplicationData.Current.LocalSettings.Values["tiles"] = tiles;
        }

        public static async void CheckTiles()
        {
            var tiles = (string) ApplicationData.Current.LocalSettings.Values["tiles"];
            if (string.IsNullOrWhiteSpace(tiles))
                return;


            var newTiles = "";
            foreach (var tileId in tiles.Split(';'))
            {
                if (!SecondaryTile.Exists(tileId))
                {
                    try
                    {
                        StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync($"{tileId}.png");
                        await file.DeleteAsync();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                else
                {
                    newTiles += tileId + ";";
                }
            }
            ApplicationData.Current.LocalSettings.Values["tiles"] = newTiles;
        }

        public static MainViewModel GetMainPageInstance()
        {
            return ViewModelLocator.Main;
        }

        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        public static int ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (int) Math.Floor(diff.TotalSeconds);
        }

        public static int GetCachePersitence()
        {
            return (int) (ApplicationData.Current.LocalSettings.Values["CachePersistency"] ?? 3600);
        }

        public static bool IsCachingEnabled()
        {
            return (bool) (ApplicationData.Current.LocalSettings.Values["EnableCache"] ?? false);
        }

        public static SortOptions GetSortOrder()
        {
            return (SortOptions) (int) (ApplicationData.Current.LocalSettings.Values["SortOrder"] ?? 3);
        }

        public static bool IsSortDescending()
        {
            return (bool) (ApplicationData.Current.LocalSettings.Values["SortDescending"] ?? true);
        }

        internal static int GetDefaultAnimeFilter()
        {
            return (int) (ApplicationData.Current.LocalSettings.Values["DefaultFilter"] ?? (int) AnimeStatus.Watching);
        }

        public static int GetItemsPerPage()
        {
            return (int) (ApplicationData.Current.LocalSettings.Values["ItemsPerPage"] ?? 5);
        }

        /// <summary>
        ///     http://stackoverflow.com/questions/14488796/does-net-provide-an-easy-way-convert-bytes-to-kb-mb-gb-etc
        /// </summary>
        public static string SizeSuffix(long value)
        {
            if (value < 0)
            {
                return "-" + SizeSuffix(-value);
            }
            if (value == 0)
            {
                return "0.0 bytes";
            }

            var mag = (int) Math.Log(value, 1024);
            var adjustedSize = (decimal) value/(1L << (mag*10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        public static async Task RemoveProfileImg()
        {
            try
            {
                await (await ApplicationData.Current.LocalFolder.GetFileAsync("UserImg.png")).DeleteAsync(
                    StorageDeleteOption.PermanentDelete);
            }
            catch (Exception)
            {
                //no file
            }
            
        }

        public static async void DownloadProfileImg(int retries = 5)
        {
            if (!Creditentials.Authenticated)
                return;
            try
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                StorageFile thumb = await folder.CreateFileAsync("UserImg.png", CreationCollisionOption.ReplaceExisting);

                var http = new HttpClient();
                byte[] response = {};

                await Task.Run(async () => response = await http.GetByteArrayAsync($"http://cdn.myanimelist.net/images/userimages/{Creditentials.Id}.jpg"));
                    //get bytes

                using (Stream fs = await thumb.OpenStreamForWriteAsync()) //get stream
                {
                    using (var writer = new DataWriter(fs.AsOutputStream()))
                    {
                        writer.WriteBytes(response); //write
                        await writer.StoreAsync();
                        await writer.FlushAsync();
                        await Task.Delay(2000);
                    }
                }

                await ViewModelLocator.Hamburger.UpdateProfileImg(false);
            }
            catch (Exception)
            {
                await ViewModelLocator.Hamburger.UpdateProfileImg(false);
            }
        }

        public static string CleanAnimeTitle(string title)
        {
            var index = title.IndexOf('+');
            return index == -1 ? title : title.Substring(0, index);
        }

        public static string FirstCharToUpper(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static async void PinTile(string targetUrl,int id,string imgUrl,string title)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile thumb = await folder.CreateFileAsync($"{id}.png", CreationCollisionOption.ReplaceExisting);

            var http = new HttpClient();
            byte[] response = await http.GetByteArrayAsync(imgUrl); //get bytes

            Stream fs = await thumb.OpenStreamForWriteAsync(); //get stream

            using (var writer = new DataWriter(fs.AsOutputStream()))
            {
                writer.WriteBytes(response); //write
                await writer.StoreAsync();
                await writer.FlushAsync();
            }

            if (!targetUrl.Contains("http"))
                targetUrl = "http://" + targetUrl;
            var til = new SecondaryTile($"{id}", $"{title}", targetUrl, new Uri($"ms-appdata:///local/{id}.png"),
                TileSize.Default);
            RegisterTile(id.ToString());
            await til.RequestCreateAsync();
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

        #region BackNavigation

        private static PageIndex _pageTo;
        private static object _args;

        public static void RegisterBackNav(PageIndex page, object args)
        {
            _pageTo = page;
            _args = args;
            SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            currentView.BackRequested += CurrentViewOnBackRequested;
        }

        private static async void CurrentViewOnBackRequested(object sender, BackRequestedEventArgs args)
        {
            args.Handled = true;
            var page = GetMainPageInstance();
            await page.Navigate(_pageTo, _args);
            ViewModelLocator.Hamburger.SetActiveButton(GetButtonForPage(_pageTo));
        }

        public static void DeregisterBackNav()
        {
            SystemNavigationManager currentView = SystemNavigationManager.GetForCurrentView();
            currentView.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            currentView.BackRequested -= CurrentViewOnBackRequested;
            _args = null;
        }

        public static HamburgerButtons GetButtonForPage(PageIndex page)
        {
            switch (page)
            {
                case PageIndex.PageAnimeList:
                    return HamburgerButtons.AnimeList;
                case PageIndex.PageAnimeDetails:
                    return HamburgerButtons.AnimeList;
                case PageIndex.PageSettings:
                    return HamburgerButtons.Settings;
                case PageIndex.PageSearch:
                    return HamburgerButtons.AnimeSearch;
                case PageIndex.PageLogIn:
                    return HamburgerButtons.LogIn;
                case PageIndex.PageProfile:
                    return HamburgerButtons.Profile;
                case PageIndex.PageAbout:
                    return HamburgerButtons.About;
                case PageIndex.PageRecomendations:
                    return HamburgerButtons.Recommendations;
                case PageIndex.PageSeasonal:
                    return HamburgerButtons.Seasonal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }

        #endregion

        public static string DecodeXmlSynopsis(string txt)
        {
            return Regex.Replace(txt, @"<[^>]+>|&nbsp;", "")
                    .Trim()
                    .Replace("[i]", "")
                    .Replace("[/i]", "")
                    .Replace("#039;", "'")
                    .Replace("&quot;", "\"")
                    .Replace("&mdash;", "—");
        }
    }
}