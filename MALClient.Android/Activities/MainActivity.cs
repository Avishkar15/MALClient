﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Com.Orhanobut.Dialogplus;
using Com.Shehabic.Droppy;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using MALClient.Android.DIalogs;
using MALClient.Android.Fragments;
using MALClient.Android.Resources;
using MALClient.Android.ViewModels;
using MALClient.Models.Enums;
using MALClient.Models.Models.Notifications;
using MALClient.XShared.BL;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.Comm.Manga;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.Utils.Managers;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Interfaces;
using Fragment = Android.App.Fragment;

namespace MALClient.Android.Activities
{
    [Activity(Label = "MALClient", ScreenOrientation = ScreenOrientation.Portrait, ResizeableActivity = true,
        Icon = "@mipmap/ic_launcher", RoundIcon = "@mipmap/ic_launcher_round",
        WindowSoftInputMode = SoftInput.AdjustUnspecified, MainLauncher = true, LaunchMode = LaunchMode.SingleInstance,
        Theme = "@style/Theme.Splash", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public partial class MainActivity : AppCompatActivity, IDimensionsProvider
    {
        private static bool _staticInitPerformed;
        private static bool _firstCreation = true;


        private static Fragment _lastFragment;

        private bool _addedNavHandlers;

        private MainViewModel _viewModel;

        public MainActivity()
        {
            CurrentContext = this;
            RegisterIoC();
        }

        public static MainActivity CurrentContext { get; private set; }
        public static bool IsAmoledApplied { get; private set; }
        public static int CurrentTheme { get; private set; }
        public static AndroidColorThemes CurrentAccent { get; set; }

        public DialogPlus DialogToCollapseOnBack { get; set; }
        private MainViewModel ViewModel => _viewModel ?? (_viewModel = SimpleIoc.Default.GetInstance<MainViewModel>());

        public double ActualWidth => -1;
        public double ActualHeight => -1;

        private void RegisterIoC()
        {
            if (SimpleIoc.Default.IsRegistered<Activity>())
                SimpleIoc.Default.Unregister<Activity>();
            SimpleIoc.Default.Register<Activity>(() => this);
        }

        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            CurrentTheme = Settings.SelectedTheme;
            CurrentAccent = AndroidColourThemeHelper.CurrentTheme;
            SetRightTheme();
            ResourceExtension.Init();
            base.OnCreate(bundle);

            if (Resources.DisplayMetrics.WidthPixels >= 1080)
                Settings.MakeGridItemsSmaller = true;

            AnimeListPageFragment.RightDrawer = null;
            if (!_addedNavHandlers)
            {
                RegisterIoC();
                SetContentView(Resource.Layout.MainPage);
                InitAdContainer();
                InitBindings();
                ViewModel.MainNavigationRequested += ViewModelOnMainNavigationRequested;
                ViewModel.MainNavigationRequested += ViewModelOnMainNavigationRequestedFirst;
                ViewModel.MediaElementCollapsed += ViewModelOnMediaElementCollapsed;

                ViewModelLocator.AnimeList.DimensionsProvider = this;

                var args = Intent.Extras?.GetString("launchArgs") ?? Intent.Data?.ToString();
                ProcessLaunchArgs(args, true);
                ViewModel.PerformFirstNavigation();

                DroppyMenuPopup.RequestedElevation = DimensionsHelper.DpToPx(10);

                ResourceLocator.NotificationsTaskManager.StartTask(BgTasks.Notifications);


                //if ((Resources.Configuration.ScreenLayout & ScreenLayout.SizeMask) == ScreenLayout.SizeSmall)
                //{
                //    Settings.PullHigherQualityImages = false;
                //}


                DroppyMenuPopup.OverrideRequested +=
                    (sender, action) => ViewModelLocator.NavMgr.RegisterOneTimeMainOverride(new RelayCommand(action));
                DroppyMenuPopup.ResetOverrideRequested +=
                    (sender, eventArgs) => ViewModelLocator.NavMgr.ResetOneTimeOverride();

                //Check permissions
                var requiredPermission = new List<string>();
                if (ContextCompat.CheckSelfPermission(this,
                        Manifest.Permission.ReadExternalStorage)
                    != Permission.Granted)
                    requiredPermission.Add(Manifest.Permission.ReadExternalStorage);

                if (ContextCompat.CheckSelfPermission(this,
                        Manifest.Permission.WriteExternalStorage)
                    != Permission.Granted)
                    requiredPermission.Add(Manifest.Permission.WriteExternalStorage);

                if (requiredPermission.Any())
                    ActivityCompat.RequestPermissions(this,
                        requiredPermission.ToArray(), 12);
                _addedNavHandlers = true;
            }

            if (!_staticInitPerformed)
            {
                PerformStaticInit();
            }

            //basically splitscreen
            if (_lastFragment != null && !_firstCreation) ViewModelOnMainNavigationRequested(_lastFragment);

            _firstCreation = false;


            Ehh();
        }

        private async void PerformStaticInit()
        {
            var policy = new StrictMode.ThreadPolicy.Builder().PermitAll().Build();
            StrictMode.SetThreadPolicy(policy);

            InitializationRoutines.InitPostUpdate();

            await Task.Delay(1000);
            if (ResourceLocator.ChangelogProvider.NewChangelog)
                ChangelogDialog.BuildChangelogDialog(ResourceLocator.ChangelogProvider);

            RateReminderPopUp.ProcessRatePopUp();

            MemoryWatcher.Watcher.Resume(true);
            ResourceLocator.TelemetryProvider.Init();

            _staticInitPerformed = true;
        }

        private async void Ehh()
        {
            while (true)
            {
                var client = new HttpClient();
                try
                {
                    var response = await client.GetAsync("https://myanimelist.net/malappinfo.php?u=Drutol");
                    if(response.IsSuccessStatusCode)
                        return;
                }
                catch (Exception e)
                {
                    //mal bugged
                }
                await ResourceLocator.MessageDialogProvider.ShowMessageDialogAsync(
                    "Hello,\r\n\r\nToday is sad day. After years of me struggling with MAL and their crappy API, they have decided to just disable it for God knows how long. No warning. No notice. Just dead. App is now unusable. Let me repeat: UNUSABLE, for an undisclosed period of time. Maybe they will revert it, maybe not? Either way the future is grim. It\'s same for all 3rd party apps.\r\n\r\nI\'d really love if you could express your opinion on the matter on MAL forums and directly to their support.\r\nThe app will start working normally once they revert it. Bring the fire to their support.\r\n\r\nYou can grab more info on github. We also have some rough discord server going on, link on github.",
                    "MAL Disabled Our Access");
                ResourceLocator.SystemControlsLauncherService.LaunchUri(new Uri("https://myanimelist.net/about.php?go=support"));
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            Permission[] grantResults)
        {
            if (requestCode == 12)
                if (grantResults.Any(permission => permission == Permission.Denied))
                    ResourceLocator.MessageDialogProvider.ShowMessageDialog(
                        "Hey hey, You've just declined some permissions... You won't be able to save images!",
                        "Umm...");

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private async void ViewModelOnMainNavigationRequestedFirst(Fragment fragment)
        {
            ViewModel.MainNavigationRequested -= ViewModelOnMainNavigationRequestedFirst;

            await Task.Delay(1000);
            if (ViewModel.CurrentMainPage != PageIndex.PageLogIn)
                RequestedOrientation = ScreenOrientation.Unspecified;
        }

        private void ViewModelOnMediaElementCollapsed()
        {
            MainPageVideoView.StopPlayback();
            MainPageVideoView.SetMediaController(null);
            MainPageVideoView.SetVideoURI(null);
        }

        public override void OnBackPressed()
        {
            if (DialogToCollapseOnBack != null)
            {
                DialogToCollapseOnBack.Dismiss();
                DialogToCollapseOnBack = null;
                return;
            }

            if (!ViewModel.SearchToggleLock)
                if (ViewModel.SearchToggleStatus)
                {
                    MainPageSearchView.SetQuery("", false);
                    MainPageSearchView.FindViewById(Resource.Id.search_close_btn).PerformClick();
                    return;
                }

            ViewModelLocator.NavMgr.CurrentMainViewOnBackRequested();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            RunOnUiThread(() =>
            {
                var args = intent.Extras?.GetString("launchArgs") ?? intent.Data?.ToString();
                ProcessLaunchArgs(args, false);
            });
        }

        private void ViewModelOnMainNavigationRequested(Fragment fragment)
        {
            try
            {
                _lastFragment = fragment;
                var trans = FragmentManager.BeginTransaction();
                trans.SetCustomAnimations(Resource.Animator.animation_slide_btm,
                    Resource.Animator.animation_fade_out,
                    Resource.Animator.animation_slide_btm,
                    Resource.Animator.animation_fade_out);
                trans.Replace(Resource.Id.MainContentFrame, fragment);
                trans.CommitAllowingStateLoss();
            }
            catch (Exception)
            {
            }
        }

        private void ProcessLaunchArgs(string args, bool startup)
        {
            if (!string.IsNullOrWhiteSpace(args))
            {
                Tuple<int, string> navArgs = null;
                Tuple<PageIndex, object> fullNavArgs = null;
                if (args.Contains('~')) //from notification -> mark read
                {
                    var arg = args;
                    var pos = arg.IndexOf('~');
                    if (pos != -1)
                    {
                        var id = arg.Substring(0, pos);
                        arg = arg.Substring(pos + 1);
                        MalNotificationsQuery.MarkNotifiactionsAsRead(new MalNotification(id));
                        fullNavArgs = MalLinkParser.GetNavigationParametersForUrl(arg);
                    }
                    else
                    {
                        fullNavArgs = MalLinkParser.GetNavigationParametersForUrl(arg);
                    }
                }
                else if (Regex.IsMatch(args, @"[OpenUrl,OpenDetails];.*"))
                {
                    var options = args.Split(';');
                    if (args.Contains('|')) //legacy
                    {
                        var detailArgs = options[1].Split('|');
                        navArgs = new Tuple<int, string>(int.Parse(detailArgs[0]), detailArgs[1]);
                    }
                    else
                    {
                        fullNavArgs = MalLinkParser.GetNavigationParametersForUrl(options[1]);
                    }
                }
                else
                {
                    fullNavArgs = MalLinkParser.GetNavigationParametersForUrl(args);
                }

                if (startup)
                {
                    MainViewModelBase.InitDetailsFull = fullNavArgs;
                    MainViewModelBase.InitDetails = navArgs;
                }
                else
                {
                    if (navArgs != null)
                        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageAnimeDetails,
                            new AnimeDetailsPageNavigationArgs(navArgs.Item1, navArgs.Item2, null, null));
                    if (fullNavArgs != null)
                        ViewModelLocator.GeneralMain.Navigate(fullNavArgs.Item1, fullNavArgs.Item2);
                }
            }
        }

        protected override void OnResume()
        {
            //_videoAd.Resume(this);
            MemoryWatcher.Watcher.Resume(false);
            base.OnResume();
        }

        protected override void OnDestroy()
        {
            _videoAd?.Destroy(this);
            ViewModel.MediaElementCollapsed -= ViewModelOnMediaElementCollapsed;
            ViewModel.MainNavigationRequested -= ViewModelOnMainNavigationRequested;
            base.OnDestroy();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            //outState.PutString("WORKAROUND_FOR_BUG_19917_KEY", "WORKAROUND_FOR_BUG_19917_VALUE");
            //base.OnSaveInstanceState(outState);
        }

        protected override void OnPause()
        {
            _videoAd?.Pause(this);
            MemoryWatcher.Watcher.Pause();
#pragma warning disable 4014
            if (Settings.IsCachingEnabled)
            {
                if (AnimeUpdateQuery.UpdatedSomething)
                {
                    DataCache.SaveDataForUser(Credentials.UserName,
                        ResourceLocator.AnimeLibraryDataStorage.AllLoadedAnimeItemAbstractions.Select(
                            abstraction => abstraction.EntryData), AnimeListWorkModes.Anime);
                    AnimeUpdateQuery.UpdatedSomething = false;
                }

                if (MangaUpdateQuery.UpdatedSomething)
                {
                    DataCache.SaveDataForUser(Credentials.UserName,
                        ResourceLocator.AnimeLibraryDataStorage.AllLoadedMangaItemAbstractions.Select(
                            abstraction => abstraction.EntryData), AnimeListWorkModes.Manga);
                    MangaUpdateQuery.UpdatedSomething = false;
                }
            }

            DataCache.SaveVolatileData();
            DataCache.SaveHumMalIdDictionary();
            ViewModelLocator.ForumsMain.SavePinnedTopics();
            FavouritesManager.SaveData();
            AnimeImageQuery.SaveData();
            ResourceLocator.HandyDataStorage.SaveData();
#pragma warning restore 4014
            base.OnPause();
        }
    }
}