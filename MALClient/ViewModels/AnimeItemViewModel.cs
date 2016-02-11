﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MALClient.Comm;
using MALClient.Items;
using MALClient.Pages;

namespace MALClient.ViewModels
{
    public interface IAnimeItemInteractions
    {
        Flyout WatchedFlyout { get; }
    }

    public class AnimeItemViewModel : ViewModelBase , IAnimeData
    {
        private readonly string _imgUrl;
        
        private int _allEpisodes;
        //state fields
        private bool _expandState;
        
        private float _globalScore;
        public readonly AnimeItemAbstraction _parentAbstraction;
        private bool _seasonalState;
        //prop field pairs

        public IAnimeItemInteractions View;

        #region Constructors
        private AnimeItemViewModel(string img, int id, AnimeItemAbstraction parent)
        {           
            _parentAbstraction = parent;
            _imgUrl = img;
            Id = id;
            Image = new BitmapImage(new Uri(_imgUrl));
        }

        public AnimeItemViewModel(bool auth, string name, string img, int id, int myStatus, int myEps, int allEps, int myScore,
            AnimeItemAbstraction parent, bool setEpsAuth = false) : this(img, id, parent)
            //We are loading an item that IS on the list
        {
            //Assign fields
            Id = id;
            _allEpisodes = allEps;
            Auth = auth;
            //Assign properties
            MyStatus = myStatus;
            MyScore = myScore;
            Title = name;
            MyEpisodes = myEps;
            ShowMoreVisibility = Visibility.Collapsed;

            //We are not seasonal so it's already on list            
            AddToListVisibility = Visibility.Collapsed;
            SetAuthStatus(auth, setEpsAuth);
            AdjustIncrementButtonsVisibility();
            //There may be additional data available
            GlobalScore = _parentAbstraction.GlobalScore;
            if (_parentAbstraction.AirDay != -1)
                Airing = true;
         
        }

        public AnimeItemViewModel(SeasonalAnimeData data,
            AnimeItemAbstraction parent) : this(data.ImgUrl, data.Id, parent)
            //We are loading an item that is NOT on the list and is seasonal
        {
            _seasonalState = true;
            MyStatus = (int)AnimeStatus.AllOrAiring;
            Title = data.Title;
            MyScore = 0;
            GlobalScore = data.Score;
            int.TryParse(data.Episodes, out _allEpisodes);
            if (data.Genres != null)
                Genres = data.Genres;            
            Airing = true;
            Synopsis = data.Synopsis;
            SetAuthStatus(false, true);
            AdjustIncrementButtonsVisibility();
        }
        #endregion

        #region PropertyPairs
        public int AllEpisodes => _allEpisodes;

        public string AirDayBind => Utils.DayToString((DayOfWeek)(_parentAbstraction.AirDay - 1));
        private bool _airing;
        public bool Airing
        {
            get { return _airing; }
            set
            {
                if (_parentAbstraction.TryRetrieveVolatileData())
                {
                    RaisePropertyChanged(() => AirDayBind);
                    TitleMargin = new Thickness(5, 3, 50, 0);
                }
                _airing = value;
                RaisePropertyChanged(() => Airing);
            }
        }

        private Thickness _titleMargin = new Thickness(5,3,5,3);
        public Thickness TitleMargin
        {
            get { return _titleMargin; }
            set
            {
                _titleMargin = value;
                RaisePropertyChanged(() => TitleMargin);
            }
        }

        private List<string> _genres = new List<string>();
        private List<string> Genres
        {
            get { return _genres; }
            set
            {
                _genres = value;
                RaisePropertyChanged(() => Genres);
            }
        }

        private Orientation _incrementButtonsOrientation = Orientation.Horizontal;
        public Orientation IncrementButtonsOrientation
        {
            get { return _incrementButtonsOrientation; }
            set
            {
                _incrementButtonsOrientation = value;
                RaisePropertyChanged(() => IncrementButtonsOrientation);
            }
        }

        public bool Auth { get; private set; }

        public string MyStatusBind => Utils.StatusToString(MyStatus);
        public int MyStatus
        {
            get { return _parentAbstraction.MyStatus; }
            set
            {
                _parentAbstraction.MyStatus = value;
                RaisePropertyChanged(() => MyStatusBind);
                AdjustIncrementButtonsOrientation();
            }
        }

        public string MyScoreBind => MyScore == 0 ? "Unranked" : $"{MyScore}/10";
        public int MyScore
        {
            get { return _parentAbstraction.MyScore; }
            set
            {
                _parentAbstraction.MyScore = value;
                RaisePropertyChanged(() => MyScoreBind);
                AdjustIncrementButtonsOrientation();
            }
        }

        public string MyEpisodesBind
        {
            get
            {
                if(_seasonalState)
                    return $"{(AllEpisodes == 0 ? "?" : AllEpisodes.ToString())} Episodes";

                return "Watched : " + (Auth || MyEpisodes != 0 ? $"{MyEpisodes}/{(AllEpisodes == 0 ? "?" : AllEpisodes.ToString())}" : $"{(AllEpisodes == 0 ? "?" : AllEpisodes.ToString())} Episodes");
            }
        } 
        public int MyEpisodes
        {
            get { return _parentAbstraction.MyEpisodes; }
            set
            {
                _parentAbstraction.MyEpisodes = value;
                RaisePropertyChanged(() => MyEpisodesBind);
            }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(() => Title);
            }
        }

        public string GlobalScoreBind => GlobalScore == 0 ? "" : GlobalScore.ToString();
        public float GlobalScore
        {
            get { return _globalScore; }
            set
            {
                if (value == 0)
                    return;
                _globalScore = value;
                RaisePropertyChanged(() => GlobalScoreBind);
            }
        }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                _image = value;
                RaisePropertyChanged(() => Image);
            }
        }

        private bool _updateButtonsEnableState;
        public bool UpdateButtonsEnableState
        {
            get { return _updateButtonsEnableState; }
            set
            {
                _updateButtonsEnableState = value;
                RaisePropertyChanged(() => UpdateButtonsEnableState);
            }
        }

        private string _synopsis;
        public string Synopsis
        {
            get { return _synopsis; }
            set
            {
                _synopsis = value;
                RaisePropertyChanged(() => Synopsis);
            }
        }

        private Visibility _addToListVisibility;
        public Visibility AddToListVisibility
        {
            get { return _addToListVisibility; }
            set
            {
                _addToListVisibility = value;
                RaisePropertyChanged(() => AddToListVisibility);
            }
        }

        private Visibility _incrementEpsVisibility;
        public Visibility IncrementEpsVisibility
        {
            get { return _incrementEpsVisibility; }
            set
            {
                _incrementEpsVisibility = value;
                RaisePropertyChanged(() => IncrementEpsVisibility);
            }
        }

        private Visibility _decrementEpsVisibility;
        public Visibility DecrementEpsVisibility
        {
            get { return _decrementEpsVisibility; }
            set
            {
                _decrementEpsVisibility = value;
                RaisePropertyChanged(() => DecrementEpsVisibility);
            }
        }

        private Visibility _showMoreVisiblity;
        public Visibility ShowMoreVisibility
        {
            get { return _showMoreVisiblity; }
            set
            {
                _showMoreVisiblity = value;
                RaisePropertyChanged(() => ShowMoreVisibility);
            }
        }

        private Visibility _updateButtonsVisibility;
        public Visibility UpdateButtonsVisibility
        {
            get { return _updateButtonsVisibility; }
            set
            {
                _updateButtonsVisibility = value;
                RaisePropertyChanged(() => UpdateButtonsVisibility);
            }
        }

        private Visibility _tileUrlInputVisibility = Visibility.Collapsed;
        public Visibility TileUrlInputVisibility
        {
            get { return _tileUrlInputVisibility; }
            set
            {
                _tileUrlInputVisibility = value;
                RaisePropertyChanged(() => TileUrlInputVisibility);
            }
        }

        private string _tileUrlInput;
        public string TileUrlInput
        {
            get { return _tileUrlInput; }
            set
            {
                _tileUrlInput = value;
                RaisePropertyChanged(() => TileUrlInput);
            }
        }

        private string _watchedEpsInput;
        public string WatchedEpsInput
        {
            get { return _watchedEpsInput; }
            set
            {
                _watchedEpsInput = value;
                RaisePropertyChanged(() => WatchedEpsInput);
            }
        }

        private Visibility _watchedEpsInputNoticeVisibility = Visibility.Collapsed;
        public Visibility WatchedEpsInputNoticeVisibility
        {
            get { return _watchedEpsInputNoticeVisibility; }
            set
            {
                _watchedEpsInputNoticeVisibility = value;
                RaisePropertyChanged(() => WatchedEpsInputNoticeVisibility);
            }
        }

        private Brush _rootBrush = new SolidColorBrush(Colors.WhiteSmoke);
        public Brush RootBrush
        {
            get { return _rootBrush; }
            set
            {
                _rootBrush = value;
                RaisePropertyChanged(() => RootBrush);
            }
        }

        private Visibility _loadingUpdate = Visibility.Collapsed;
        public Visibility LoadingUpdate
        {
            get { return _loadingUpdate; }
            set
            {
                _loadingUpdate = value;
                RaisePropertyChanged(() => LoadingUpdate);
            }
        }

        private ICommand _changeStatusCommand;
        public ICommand ChangeStatusCommand
        {
            get
            {
                return _changeStatusCommand ?? (_changeStatusCommand = new RelayCommand<Object>(ChangeStatus));
            }
        }

        private ICommand _changeScoreCommand;
        public ICommand ChangeScoreCommand
        {
            get
            {
                return _changeScoreCommand ?? (_changeScoreCommand = new RelayCommand<Object>(ChangeScore));
            }
        }

        private ICommand _changeWatchedCommand;
        public ICommand ChangeWatchedCommand
        {
            get
            {
                return _changeWatchedCommand ?? (_changeWatchedCommand = new RelayCommand(ChangeWatchedEps));
            }
        }

        private ICommand _incrementWatchedCommand;
        public ICommand IncrementWatchedCommand
        {
            get
            {
                return _incrementWatchedCommand ?? (_incrementWatchedCommand = new RelayCommand(IncrementWatchedEp));
            }
        }

        private ICommand _decrementWatchedCommand;
        public ICommand DecrementWatchedCommand
        {
            get
            {
                return _decrementWatchedCommand ?? (_decrementWatchedCommand = new RelayCommand(DecrementWatchedEp));
            }
        }

        private ICommand _addAnimeCommand;
        public ICommand AddAnimeCommand
        {
            get
            {
                return _addAnimeCommand ?? (_addAnimeCommand = new RelayCommand(AddThisToMyList));
            }
        }

        private ICommand _pinTileCommand;
        public ICommand PinTileCommand
        {
            get
            {
                return _pinTileCommand ?? (_pinTileCommand = new RelayCommand(PinTile));
            }
        }

        private ICommand _navigateDetailsCommand;
        public ICommand NavigateDetailsCommand
        {
            get
            {
                return _navigateDetailsCommand ?? (_navigateDetailsCommand = new RelayCommand(NavigateDetails));
            }
        }

        #endregion
        //fields
        public int Id { get; set; }
        //private int SeasonalMembers { get; set; } //TODO : Use this
        

        public async void NavigateDetails()
        {
            await Utils.GetMainPageInstance()
                .Navigate(PageIndex.PageAnimeDetails,
                    new AnimeDetailsPageNavigationArgs(Id, Title, null, this,
                        Utils.GetMainPageInstance().GetCurrentListOrderParams())
                    { Source = PageIndex.PageAnimeList });
        }

        public void UpdateWithSeasonData(SeasonalAnimeData data)
        {
            GlobalScore = data.Score;
            if (data.Genres != null)
                Genres = data.Genres;
            Airing = true;
            if (!Auth)
            {
                UpdateButtonsVisibility = Visibility.Collapsed;
                _seasonalState = true;
            }
            RaisePropertyChanged(() => MyEpisodesBind);

        }

        public void SignalBackToList()
        {
            _seasonalState = false;
            RaisePropertyChanged(() => MyEpisodesBind);
        }

        private async void AddThisToMyList()
        {
            var response = await new AnimeAddQuery(Id.ToString()).GetRequestResponse();
            if (!response.Contains("Created"))
                return;
            _seasonalState = false;
            SetAuthStatus(true);
            MyScore = 0;
            MyStatus = 6;
            MyEpisodes = 0;
            
            
            AdjustIncrementButtonsVisibility();
            AddToListVisibility = Visibility.Collapsed;
            ViewModelLocator.AnimeList.AddAnimeEntry(_parentAbstraction);
        }

        #region Utils/Helpers

        public async void PinTile(string targetUrl)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile thumb = await folder.CreateFileAsync($"{Id}.png", CreationCollisionOption.ReplaceExisting);

            var http = new HttpClient();
            byte[] response = await http.GetByteArrayAsync(_imgUrl); //get bytes

            Stream fs = await thumb.OpenStreamForWriteAsync(); //get stream

            using (var writer = new DataWriter(fs.AsOutputStream()))
            {
                writer.WriteBytes(response); //write
                await writer.StoreAsync();
                await writer.FlushAsync();
            }

            if (!targetUrl.Contains("http"))
                targetUrl = "http://" + targetUrl;
            var til = new SecondaryTile($"{Id}", $"{Title}", targetUrl, new Uri($"ms-appdata:///local/{Id}.png"),
                TileSize.Default);
            Utils.RegisterTile(Id.ToString());
            await til.RequestCreateAsync();
        }

        //Pinned with custom link.
        public void PinTile()
        {
            PinTile(TileUrlInput);
            TileUrlInputVisibility = Visibility.Collapsed;
        }


        /// <summary>
        ///     Used by alternate row colors
        /// </summary>
        /// <param name="brush"></param>
        public void Setbackground(SolidColorBrush brush)
        {
            RootBrush = brush;
        }



        public void SetAuthStatus(bool auth, bool eps = false)
        {
            Auth = auth;
            if (auth)
            {
                AddToListVisibility = Visibility.Collapsed;
                UpdateButtonsVisibility = Visibility.Visible;
                UpdateButtonsEnableState = true;
            }
            else
            {
                AddToListVisibility = _seasonalState && Creditentials.Authenticated ? Visibility.Visible : Visibility.Collapsed;
                UpdateButtonsEnableState = false;

                if (eps)
                {
                    RaisePropertyChanged(() => MyEpisodesBind);
                    UpdateButtonsVisibility = Visibility.Collapsed;
                }
            }
            AdjustIncrementButtonsVisibility();
        }

        private void AdjustIncrementButtonsVisibility()
        {
            if (!Auth || !Creditentials.Authenticated || MyStatus == (int)AnimeStatus.PlanToWatch)
            {
                IncrementEpsVisibility = Visibility.Collapsed;
                DecrementEpsVisibility = Visibility.Collapsed;
                return;
            }

            if (MyEpisodes == _allEpisodes && _allEpisodes != 0)
            {
                IncrementEpsVisibility = Visibility.Collapsed;
                DecrementEpsVisibility = Visibility.Visible;
            }
            else if (MyEpisodes == 0)
            {
                IncrementEpsVisibility = Visibility.Visible;
                DecrementEpsVisibility = Visibility.Collapsed;
            }
            else
            {
                IncrementEpsVisibility = Visibility.Visible;
                DecrementEpsVisibility = Visibility.Visible;
            }

        }

        private void AdjustIncrementButtonsOrientation()
        {
            //Too wide update buttons
            if (MyScore == 0)
            {
                if (MyStatus == (int) AnimeStatus.Dropped ||
                    MyStatus == (int) AnimeStatus.OnHold ||
                    MyStatus == (int) AnimeStatus.Completed)
                    IncrementButtonsOrientation = Orientation.Vertical;
            }
        }

        #endregion

        #region AnimeUpdate

        #region Watched

        private async void IncrementWatchedEp()
        {
            LoadingUpdate = Visibility.Visible;

            if (MyStatus == (int)AnimeStatus.PlanToWatch || MyStatus == (int)AnimeStatus.Dropped ||
                MyStatus == (int)AnimeStatus.OnHold)
                PromptForStatusChange((int)AnimeStatus.Watching);

            MyEpisodes++;
            var response = await new AnimeUpdateQuery(this).GetRequestResponse();
            if (response != "Updated")
                MyEpisodes--; // Shouldn't occur really , but hey shouldn't and MAL api goes along very well.

            if (MyEpisodes == _allEpisodes && _allEpisodes != 0)
                PromptForStatusChange((int)AnimeStatus.Completed);


            AdjustIncrementButtonsVisibility();

            LoadingUpdate = Visibility.Collapsed;
        }

        private async void DecrementWatchedEp()
        {
            LoadingUpdate = Visibility.Visible;
            MyEpisodes--;
            var response = await new AnimeUpdateQuery(this).GetRequestResponse();
            if (response != "Updated")
                MyEpisodes++;

            AdjustIncrementButtonsVisibility();

            LoadingUpdate = Visibility.Collapsed;
        }

        public async void ChangeWatchedEps()
        {
            int watched;
            if (!int.TryParse(WatchedEpsInput, out watched))
            {
                WatchedEpsInputNoticeVisibility = Visibility.Visible;
                return;
            }
            if (watched >= 0 && (_allEpisodes == 0 || watched <= _allEpisodes))
            {
                View.WatchedFlyout.Hide();
                LoadingUpdate = Visibility.Visible;
                WatchedEpsInputNoticeVisibility = Visibility.Collapsed;
                var prevWatched = MyEpisodes;
                MyEpisodes = watched;
                var response = await new AnimeUpdateQuery(this).GetRequestResponse();
                if (response != "Updated")
                    MyEpisodes = prevWatched;

                if (MyEpisodes == _allEpisodes && _allEpisodes != 0)
                    PromptForStatusChange((int)AnimeStatus.Completed);

                AdjustIncrementButtonsVisibility();


                LoadingUpdate = Visibility.Collapsed;
                WatchedEpsInput = "";
            }
            else
            {
                WatchedEpsInputNoticeVisibility = Visibility.Visible;
            }
        }



        #endregion

        private async void ChangeStatus(object status)
        {
            LoadingUpdate = Visibility.Visible;
            var myPrevStatus = MyStatus;
            MyStatus = Utils.StatusToInt(status as string);
            var response = await new AnimeUpdateQuery(this).GetRequestResponse();
            if (response != "Updated")
                MyStatus = myPrevStatus;

            if (MyStatus == (int)AnimeStatus.Completed && _allEpisodes != 0)
                PromptForWatchedEpsChange(_allEpisodes);

            LoadingUpdate = Visibility.Collapsed;
        }

        private async void ChangeScore(object score)
        {
            LoadingUpdate = Visibility.Visible;
            var myPrevScore = MyScore;
            MyScore = Convert.ToInt32(score);
            var response = await new AnimeUpdateQuery(this).GetRequestResponse();
            if (response != "Updated")
                MyScore = myPrevScore;

            LoadingUpdate = Visibility.Collapsed;
        }

        #endregion

        #region Prompts

        private async void PromptForStatusChange(int to)
        {
            if (MyStatus == to)
                return;
            var msg = new MessageDialog($"From : {Utils.StatusToString(MyStatus)}\nTo : {Utils.StatusToString(to)}",
                "Would you like to change current status?");
            var confirmation = false;
            msg.Commands.Add(new UICommand("Yes", command => confirmation = true));
            msg.Commands.Add(new UICommand("No"));
            await msg.ShowAsync();
            if (confirmation)
            {
                var myPrevStatus = MyStatus;
                MyStatus = to;
                var response = await new AnimeUpdateQuery(this).GetRequestResponse();
                if (response != "Updated")
                    MyStatus = myPrevStatus;
            }
        }

        private async void PromptForWatchedEpsChange(int to)
        {
            if (MyEpisodes == to)
                return;
            var msg = new MessageDialog($"From : {MyEpisodes}\nTo : {to}",
                "Would you like to change watched episodes value?");
            var confirmation = false;
            msg.Commands.Add(new UICommand("Yes", command => confirmation = true));
            msg.Commands.Add(new UICommand("No"));
            await msg.ShowAsync();
            if (confirmation)
            {
                var myPrevEps = MyEpisodes;
                MyEpisodes = to;
                var response = await new AnimeUpdateQuery(this).GetRequestResponse();
                if (response != "Updated")
                    MyStatus = myPrevEps;

                AdjustIncrementButtonsVisibility();
            }
        }

        #endregion


    }


}
