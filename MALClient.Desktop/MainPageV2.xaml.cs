﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Core;
using MALClient.UWP.Shared.ViewModels.Interfaces;
using MALClient.UWP.ViewModels;
using MALClient.UWP.Shared.ViewModels;
using MALClient.UWP.Shared.UserControls;
using MALClient.XShared.ViewModels;
using MALClient.XShared.Utils;
using MALClient.Models.Enums;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.ViewModels.Main;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPageV2 : Page, IMainViewInteractions
	{

		public Visibility LogInButtonVisibility => Credentials.Authenticated ? Visibility.Collapsed : Visibility.Visible;

		public MainPageV2()
		{
			this.InitializeComponent();

			CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = false;

			Loaded += (sender, args) =>
			{
				var vm = DesktopViewModelLocator.Main;
				vm.MainNavigationRequested += Navigate;
				vm.OffNavigationRequested += NavigateOff;
				//vm.PropertyChanged += VmOnPropertyChanged;
				//UWPViewModelLocator.PinTileDialog.ShowPinDialog += () =>
				//{
				//	PinDialogStoryboard.Begin();
				//};
				//vm.MediaElementCollapsed += VmOnMediaElementCollapsed;
				//UWPViewModelLocator.PinTileDialog.HidePinDialog += HidePinDialog;
				DesktopViewModelLocator.Main.View = this;
				//StartAdsTimeMeasurements();
				//ViewModelLocator.Settings.OnAdsMinutesPerDayChanged += SettingsOnOnAdsMinutesPerDayChanged;
				ViewModelLocator.GeneralMain.ChangelogVisibility = ResourceLocator.ChangelogProvider.NewChangelog;
			};
		}

		private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
		{
			if (args.IsSettingsInvoked)
			{
				DesktopViewModelLocator.Main.Navigate(PageIndex.PageSettings);
			}
			else
			{
				PageIndex page;
				var tag = sender.MenuItems
						.OfType<NavigationViewItem>()
						.First(i => args.InvokedItem.Equals(i.Content))
						.Tag.ToString();
				if (Enum.TryParse(tag as string, out page))
				{
					DesktopViewModelLocator.Main.Navigate(page, GetAppropriateArgsForPage(page));
				}
			}
		}

		private object GetAppropriateArgsForPage(PageIndex page)
		{
			switch (page)
			{
				case PageIndex.PageSeasonal:
					return AnimeListPageNavigationArgs.Seasonal;
				case PageIndex.PageMangaList:
					return AnimeListPageNavigationArgs.Manga;
				case PageIndex.PageMangaSearch:
					return new SearchPageNavigationArgs { Anime = false };
				case PageIndex.PageSearch:
					return new SearchPageNavigationArgs();
				case PageIndex.PageTopAnime:
					return AnimeListPageNavigationArgs.TopAnime(TopAnimeType.General);
				case PageIndex.PageTopManga:
					return AnimeListPageNavigationArgs.TopManga;
				case PageIndex.PageProfile:
					return new ProfilePageNavigationArgs { TargetUser = Credentials.UserName };
				case PageIndex.PageArticles:
					return MalArticlesPageNavigationArgs.Articles;
				case PageIndex.PageNews:
					return MalArticlesPageNavigationArgs.News;
				case PageIndex.PageWallpapers:
					return new WallpaperPageNavigationArgs();
				default:
					return null;
			}
		}

		private void Navigate(Type page, object args = null)
		{
			MainContent.Navigate(page, args);
		}

		private void NavigateOff(Type page, object args = null)
		{
			OffContent.Navigate(page, args);
		}

		private void ToggleButton_OnClick(object sender, RoutedEventArgs e)
		{
			var btn = sender as LockableToggleButton;
			if (btn.IsChecked.GetValueOrDefault(false))
				SearchInput.Focus(FocusState.Keyboard);
		}

		private void SearchInput_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
		{
			if (SearchInput.Text.Length >= 2)
			{
				SearchInput.IsEnabled = false; //reset input
				SearchInput.IsEnabled = true;
				ViewModelLocator.GeneralMain.OnSearchInputSubmit();
			}
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width <= 960)
				MainSplitView.OpenPaneLength = e.NewSize.Width;
			else
				MainSplitView.OpenPaneLength = 535;

		}

		public Storyboard PinDialogStoryboard => FadeInPinDialogStoryboard;
		public Storyboard CurrentStatusStoryboard => FadeInCurrentStatus;
		public Storyboard CurrentOffStatusStoryboard => FadeInCurrentOffStatus;
		public Storyboard CurrentOffSubStatusStoryboard => FadeInCurrentSubStatus;
		public Storyboard HidePinDialogStoryboard => FadeOutPinDialogStoryboard;

		public SplitViewDisplayMode CurrentDisplayMode => throw new NotImplementedException();

		public void InitSplitter()
		{
		}

		public void SearchInputFocus(FocusState state)
		{
			SearchInput.Focus(state);
		}

	}
}
