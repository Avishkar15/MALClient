﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MALClient.Models.Enums;
using MALClient.Models.Models.MalSpecific;
using MALClient.XShared.Comm.Articles;

namespace MALClient.XShared.ViewModels.Main
{
    public class MalArticlesPageNavigationArgs
    {
        public ArticlePageWorkMode WorkMode { get; set; }
        public int NewsId { get; set; } = -1;

        public static MalArticlesPageNavigationArgs Articles => new MalArticlesPageNavigationArgs {WorkMode = ArticlePageWorkMode.Articles};
        public static MalArticlesPageNavigationArgs News => new MalArticlesPageNavigationArgs {WorkMode = ArticlePageWorkMode.News};
    }

    public delegate void OpenWebViewRequest(string html,MalNewsUnitModel model);

    public class MalArticlesViewModel : ViewModelBase
    {
        private List<MalNewsUnitModel> _articles = new List<MalNewsUnitModel>();

        public List<MalNewsUnitModel> Articles
        {
            get { return _articles; }
            set
            {
                _articles = value;
                RaisePropertyChanged(() => Articles);
            }
        }

        public event OpenWebViewRequest OpenWebView;

        private ICommand _loadArticleCommand;

        public ICommand LoadArticleCommand
            => _loadArticleCommand ?? (_loadArticleCommand = new RelayCommand<MalNewsUnitModel>(LoadArticle));

        private bool _webViewVisibility = false;

        public bool WebViewVisibility
        {
            get { return _webViewVisibility; }
            set
            {
                _webViewVisibility = value;
                RaisePropertyChanged(() => WebViewVisibility);
            }
        }

        private bool _articleIndexVisibility = true;

        public bool ArticleIndexVisibility
        {
            get { return _articleIndexVisibility; }
            set
            {
                _articleIndexVisibility = value;
                RaisePropertyChanged(() => ArticleIndexVisibility);
            }
        }

        private bool _loadingVisibility = false;

        public bool LoadingVisibility
        {
            get { return _loadingVisibility; }
            set
            {
                if(_loadingData)
                    return;
                _loadingVisibility = value;
                
                RaisePropertyChanged(() => LoadingVisibility);
            }
        }

        private double _thumbnailWidth = 150;
        private double _thumbnailHeight = 150;

        public double ThumbnailWidth
        {
            get { return _thumbnailWidth; }
            set
            {
                _thumbnailWidth = value;
                RaisePropertyChanged(() => ThumbnailWidth);
            }
        }

        public double ThumbnailHeight
        {
            get { return _thumbnailHeight; }
            set
            {
                _thumbnailHeight = value;
                RaisePropertyChanged(() => ThumbnailHeight);
            }
        }

        private bool _loadingData;
        public ArticlePageWorkMode? PrevWorkMode;
        public int CurrentNews = -1;
        public async void Init(MalArticlesPageNavigationArgs args,bool force = false)
        {
            if (args == null) //refresh
            {
                args = PrevWorkMode == ArticlePageWorkMode.Articles
                    ? MalArticlesPageNavigationArgs.Articles
                    : MalArticlesPageNavigationArgs.News;
                force = true;
            }
            ViewModelLocator.NavMgr.RegisterBackNav(PageIndex.PageAnimeList, null);
            ArticleIndexVisibility = true;
            WebViewVisibility = false;
            ViewModelLocator.GeneralMain.CurrentStatus = args.WorkMode == ArticlePageWorkMode.Articles ? "Articles" : "News";

            if (PrevWorkMode == args.WorkMode && !force)
            {
                try
                {
                    if (args.NewsId != -1)
                        LoadArticle(Articles[args.NewsId]);
                }
                catch (Exception)
                {
                    //
                }
                return;
            }          
            LoadingVisibility = true;
            _loadingData = true;

            switch (args.WorkMode)
            {
                case ArticlePageWorkMode.Articles:
                    ThumbnailWidth = ThumbnailHeight = 150;
                    break;
                case ArticlePageWorkMode.News:
                    ThumbnailWidth = 100;
                    ThumbnailHeight = 150;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            PrevWorkMode = args.WorkMode;

            var data = new List<MalNewsUnitModel>();
            Articles = new List<MalNewsUnitModel>();
           
            await Task.Run(async () =>
            {
                data = await new MalArticlesIndexQuery(args.WorkMode).GetArticlesIndex(force);                
            });
            Articles = data;
            _loadingData = false;
            LoadingVisibility = false;


        }
        
        private async void LoadArticle(MalNewsUnitModel data)
        {
            LoadingVisibility = true;
            ArticleIndexVisibility = false;
            ViewModelLocator.GeneralMain.CurrentStatus = data.Title;
            CurrentNews = Articles.IndexOf(data);
            OpenWebView?.Invoke(await new MalArticleQuery(data.Url, data.Title,data.Type).GetArticleHtml(), data);
        }
    }
}
