﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MALClient.Models.Enums;
using MALClient.Models.Models.AnimeScrapped;
using MALClient.Models.Models.Library;
using MALClient.Models.Models.Misc;
using MALClient.XShared.Utils;

// ReSharper disable InconsistentNaming

namespace MALClient.XShared.ViewModels
{
    /// <summary>
    ///     This class serves as a container for actual UI AnimeItem element.
    ///     It contains all values required to sort and filter those items before loading them.
    /// </summary>
    public class AnimeItemAbstraction
    {
        private readonly bool _firstConstructor;
        private readonly SeasonalAnimeData _seasonalData;
        public readonly bool Auth;
        public readonly bool RepresentsAnime = true;

        private AnimeItemViewModel _viewModel;

        public int AirDay { get; }


        public string AirStartDate
        {
            get { return VolatileData.AirStartDate; }
            set { VolatileData.AirStartDate = value; }
        }

        public float GlobalScore
        {
            get { return VolatileData.GlobalScore; }
            set { VolatileData.GlobalScore = value; }
        }

        public AnimePriority Priority
        {
            get => EntryData?.Priority ?? AnimePriority.Low;
            set => EntryData.Priority = value;
        }

        public int Index;
        public bool LoadedAnime;
        public bool LoadedModel;
        public bool LoadedVolatile;
        public VolatileDataCache VolatileData { get; set; } = new VolatileDataCache();

        //three constructors depending on original init
        private AnimeItemAbstraction(ILibraryData entry, int? id = null)
        {
            if (entry != null)
            {
                EntryData = entry;
                MyStartDate = entry.MyStartDate;
                MyEndDate = entry.MyEndDate;
            }

            if (ResourceLocator.AiringInfoProvider.TryGetAiringDay(id ?? Id, out DayOfWeek day))
                AirDay = (int) day+1;
            else
                AirDay = -1;

            if (!DataCache.TryRetrieveDataForId(id ?? Id, out VolatileDataCache data)) return;
            VolatileData = data;
            LoadedVolatile = true;
        }

        //three constructors depending on original init
        public AnimeItemAbstraction(SeasonalAnimeData data, bool anime) : this(null, data.Id)
        {
            _seasonalData = data;           
            Index = data.Index;
            RepresentsAnime = anime;
            if (!int.TryParse(data.Episodes, out var eps))
                eps = 0;
            AllEpisodes = eps;
        }

        public AnimeItemAbstraction(bool auth, MangaLibraryItemData data) : this(data)
        {
            RepresentsAnime = false;
            Auth = auth;
            _firstConstructor = true;
        }

        public AnimeItemAbstraction(bool auth, AnimeLibraryItemData data) : this(data)
        {
            Auth = auth;
            _firstConstructor = true;
        }

        public ILibraryData EntryData { get; set; }
        private int _seasonalAllEps;
        public string Title => EntryData?.Title ?? _seasonalData?.Title;
        public int Id => EntryData?.Id ?? _seasonalData?.Id ?? 0;
        public int MalId => EntryData?.MalId ?? _seasonalData?.Id ?? 0;
        public string ImgUrl => EntryData?.ImgUrl ?? _seasonalData?.ImgUrl;

        public bool IsRewatching
        {
            get { return EntryData?.IsRewatching ?? false; }
            set
            {
                if (EntryData != null)
                    EntryData.IsRewatching = value;
            }
        }

        public string AlternateTitle
        {
            get { return EntryData?.AlternateTitle; }
        }

        public int AllEpisodes
        {
            get { return EntryData?.AllEpisodes ?? _seasonalAllEps; }
            set { _seasonalAllEps = value; }
        }

        public int AllVolumes => (EntryData as MangaLibraryItemData)?.AllVolumes ?? 0;
        public int Type => EntryData?.Type ?? 0;

        public string Notes
        {
            get { return EntryData?.Notes ?? ""; }
            set
            {
                if (EntryData != null)
                {
                    EntryData.Notes = value;
                    _tags = null;
                }
            }
        }

        private List<string> _tags;

        public List<string> Tags => _tags ?? (_tags = string.IsNullOrEmpty(Notes)
            ? new List<string>()
            : Notes.Contains(",")
                ? Notes.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.ToLower()).ToList()
                : new List<string> {Notes.ToLower()});

        public DateTime LastWatched
        {
            get { return EntryData?.LastWatched ?? DateTime.MinValue; }
            set
            {
                if (EntryData != null)
                    EntryData.LastWatched = value;
            }
        }

        public int MyEpisodes
        {
            get { return EntryData?.MyEpisodes ?? 0; }
            set { EntryData.MyEpisodes = value; }
        }

        public float MyScore
        {
            get { return EntryData?.MyScore ?? 0; }
            set { EntryData.MyScore = value; }
        }

        public AnimeStatus MyStatus
        {
            get { return EntryData?.MyStatus ?? AnimeStatus.AllOrAiring; }
            set { EntryData.MyStatus = (AnimeStatus) value; }
        }

        public int MyVolumes
        {
            get { return (EntryData as MangaLibraryItemData)?.MyVolumes ?? 0; }
            set
            {
                if (EntryData is MangaLibraryItemData)
                    ((MangaLibraryItemData) EntryData).MyVolumes = value;
            }
        }

        public string MyStartDate
        {
            get { return EntryData?.MyStartDate ?? AnimeItemViewModel.InvalidStartEndDate; }
            set
            {
                if (EntryData != null) EntryData.MyStartDate = value;
            }
        }

        public string MyEndDate
        {
            get { return EntryData?.MyEndDate ?? AnimeItemViewModel.InvalidStartEndDate; }
            set
            {
                if (EntryData != null) EntryData.MyEndDate = value;
            }
        }

        public ExactAiringTimeData ExactAiringTime
        {
            get { return VolatileData.ExactAiringTime; }
            set
            {
                VolatileData.ExactAiringTime = value;
                VolatileData.LastFailedAiringTimeFetchAttempt = null;
            }
        }


        public AnimeItemViewModel ViewModel
        {
            get
            {
                if (LoadedAnime)
                    return _viewModel;
                ViewModel = LoadElementModel();
                return _viewModel;
            }
            private set { _viewModel = value; }
        }



        public bool TryRetrieveVolatileData(bool force = false)
        {
            if (LoadedVolatile && !force)
                return true;
            VolatileDataCache data;
            if (!DataCache.TryRetrieveDataForId(Id, out data)) return false;
            VolatileData = data;
            LoadedVolatile = true;
            return true;
        }

        private AnimeItemViewModel LoadElementModel()
        {
            if (LoadedModel)
                return _viewModel;
            LoadedModel = true;
            if (RepresentsAnime)
                return _firstConstructor
                    ? new AnimeItemViewModel(Auth, Title, ImgUrl, Id, AllEpisodes, this, false)
                    : new AnimeItemViewModel(_seasonalData, this);
            return
                _firstConstructor
                    ? new AnimeItemViewModel(Auth, Title, ImgUrl, Id, AllEpisodes, this, false, AllVolumes)
                    : new AnimeItemViewModel(_seasonalData, this);
        }
    }
}