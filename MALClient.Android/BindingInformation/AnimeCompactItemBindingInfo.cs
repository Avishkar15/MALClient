using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MALClient.Android.Activities;
using MALClient.Android.Listeners;
using MALClient.Android.Resources;
using MALClient.XShared.ViewModels;
using Debug = System.Diagnostics.Debug;

namespace MALClient.Android.BindingInformation
{
    public class AnimeCompactItemBindingInfo : BindingInfo<AnimeItemViewModel>
    {
        private int _position;
        private bool _initialized;
        public Action<AnimeItemViewModel> OnItemClickAction { get; set; }

        public override int Position
        {
            get { return _position; }
            set
            {
                _position = value;
                if (!_initialized)
                {
                    PrepareContainer();
                    _initialized = true;
                }
            }
        }


        public AnimeCompactItemBindingInfo(View container, AnimeItemViewModel viewModel, bool fling) : base(container, viewModel, fling)
        {
            OnConfigurationChanged(MainActivity.CurrentContext.Resources.Configuration);
        }

        protected override void InitBindings()
        {
            if(Fling)
                return;


        }

        protected override void InitOneTimeBindings()
        {
            Container.SetBackgroundResource(Position % 2 == 0
                ? ResourceExtension.BrushRowAlternate1Res
                : ResourceExtension.BrushRowAlternate2Res);
            ViewModel.AnimeItemDisplayContext = ViewModelLocator.AnimeList.AnimeItemsDisplayContext;

            Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemGlobalScore).Text = ViewModel.GlobalScoreBind;
            Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemType).Text = ViewModel.PureType;
            Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemTitle).Text = ViewModel.Title;
            Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemTopLeftInfo).Text = ViewModel.TopLeftInfoBind;
            Container.FindViewById(Resource.Id.AnimeCompactItemFavouriteIndicator).Visibility =
                ViewModel.IsFavouriteVisibility ? ViewStates.Visible : ViewStates.Gone;
            Container.FindViewById(Resource.Id.AnimeCompactItemTagsButton).Visibility = ViewModel.TagsControlVisibility
                ? ViewStates.Visible
                : ViewStates.Gone;


            if (!Fling && (int) Container.Tag != ViewModel.Id)
            {
                Container.Tag = ViewModel.Id;
            }
            else
            {
                Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemScoreLabel).Text = ViewModel.MyScoreBind;
                Container.FindViewById<TextView>(Resource.Id.AnimeCompactItemStatusLabel).Text = ViewModel.MyStatusBind;
                Container.FindViewById<Button>(Resource.Id.AnimeCompactItemWatchedButton).Text =
                    ViewModel.MyEpisodesBind;
                Container.FindViewById<FrameLayout>(Resource.Id.AnimeCompactItemIncButton).Visibility =
                    ViewModel.IncrementEpsVisibility ? ViewStates.Visible : ViewStates.Gone;
                Container.FindViewById<FrameLayout>(Resource.Id.AnimeCompactItemDecButton).Visibility =
                    ViewModel.DecrementEpsVisibility ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        protected override void DetachInnerBindings()
        {
            
        }

        private void ContainerOnClick()
        {
            if (OnItemClickAction != null)
                OnItemClickAction.Invoke(ViewModel);
            else
                ViewModel.NavigateDetailsCommand.Execute(null);
        }

        public void OnConfigurationChanged(Configuration newConfig)
        {
            var general = Container.FindViewById(Resource.Id.AnimeCompactItemGeneralSection);
            var edit = Container.FindViewById(Resource.Id.AnimeCompactItemEditSection);
            var stretcherLeft = Container.FindViewById(Resource.Id.AnimeCompactItemAdaptiveItemLeft);
            var stretcherRight = Container.FindViewById(Resource.Id.AnimeCompactItemAdaptiveItemRight);
            var titleLabel = Container.FindViewById(Resource.Id.AnimeCompactItemTitle);

            var parameter = general.LayoutParameters;
            var editParam = edit.LayoutParameters as RelativeLayout.LayoutParams;
            var titleParam = titleLabel.LayoutParameters as LinearLayout.LayoutParams;

            if (newConfig.ScreenWidthDp > 650)
            {
                if(parameter.Width == -2)
                    return;

                parameter.Width = -2;
                stretcherLeft.Visibility = ViewStates.Visible;
                stretcherRight.Visibility = ViewStates.Gone;
                titleParam.Width = -1;
                titleParam.Weight = 0;

                editParam.RemoveRule(LayoutRules.Below);
                editParam.AddRule(LayoutRules.RightOf, Resource.Id.AnimeCompactItemGeneralSection);
            }
            else
            {
                if (parameter.Width == -1)
                    return;

                parameter.Width = -1;
                stretcherLeft.Visibility = ViewStates.Gone;
                stretcherRight.Visibility = ViewStates.Visible;
                titleParam.Width = 0;
                titleParam.Weight = 1;

                editParam.RemoveRule(LayoutRules.RightOf);
                editParam.AddRule(LayoutRules.Below, Resource.Id.AnimeCompactItemGeneralSection);
            }

            titleLabel.LayoutParameters = titleParam;
            edit.LayoutParameters = editParam;
            general.LayoutParameters = parameter;
        }
    }
}