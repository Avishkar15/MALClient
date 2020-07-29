using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using GalaSoft.MvvmLight.Helpers;
using MALClient.Android.Activities;
using MALClient.Android.BindingConverters;
using MALClient.Android.CollectionAdapters;
using MALClient.Android.Dialogs;
using MALClient.Android.DIalogs;
using MALClient.Android.Listeners;
using MALClient.Android.Resources;
using MALClient.Android.UserControls;
using MALClient.Android.UserControls.ForumItems;
using MALClient.XShared.NavArgs;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Forums;
using MALClient.XShared.ViewModels.Forums.Items;

namespace MALClient.Android.Fragments.ForumFragments
{
    public class ForumTopicPageFragment : MalFragmentBase, ForumTopicViewModel.IScrollInfoProvider
    {
        private readonly ForumsTopicNavigationArgs _args;
        private ForumTopicViewModel ViewModel;

        private View _prevHighlightedPageIndicator;

        public ForumTopicPageFragment(ForumsTopicNavigationArgs args)
        {
            _args = args;
        }

        protected override void Init(Bundle savedInstanceState)
        {
            ViewModel = ViewModelLocator.ForumsTopic;
            ViewModel.RequestScroll += ViewModelOnRequestScroll;
            ViewModel.ScrollInfoProvider = this;
            ViewModel.Init(_args);
        }

        private async void ViewModelOnRequestScroll(object sender, int i)
        {
            try
            {
                if (RootView != null && i < ForumTopicPagePostsList?.Adapter.Count)
                    ForumTopicPagePostsList?.SetSelection(i);
                else
                {
                    await Task.Delay(100);
                    ViewModelOnRequestScroll(sender, i);
                }
            }
            catch (Exception)
            {
                //such scrolling is prone to error
            }
        }

        protected override void InitBindings()
        {
            Bindings.Add(
                this.SetBinding(() => ViewModel.LoadingTopic,
                    () => ForumTopicPageLoadingSpinner.Visibility).ConvertSourceToTarget(Converters.BoolToVisibility));

            Bindings.Add(this.SetBinding(() => ViewModel.Messages).WhenSourceChanges(() =>
            {
                if (ViewModel.Messages != null)
                {
                    _items = ViewModel.Messages.Select(model => new ForumTopicItem(Activity)).ToList();
                    ForumTopicPagePostsList.InjectFlingAdapter(ViewModel.Messages,DataTemplateFull,DataTemplateFling,ContainerTemplate,DataTemplateBasic);
                }
            }));

            Bindings.Add(
                this.SetBinding(() => ViewModel.ToggleWatchingButtonText,
                    () => ForumTopicPageToggleWatchingButton.Text));

            ForumTopicPageGotoPageButton.SetOnClickListener(new OnClickListener(v => ForumTopicPageGotoPageButtonOnClick()));
            ForumTopicPageActionButton.SetOnClickListener(new OnClickListener(v => ForumTopicPageActionButtonOnClick()));
            ForumTopicPageToggleWatchingButton.SetOnClickListener(new OnClickListener(v => ViewModel.ToggleWatchingCommand.Execute(null)));

            Bindings.Add(this.SetBinding(() => ViewModel.AvailablePages).WhenSourceChanges(() =>
            {
                ViewModel.AvailablePages.CollectionChanged += (sender, args) => UpdatePageSelection();
                UpdatePageSelection();
            }));
        }

        private async void ForumTopicPageGotoPageButtonOnClick()
        {
            if (ViewModel.LoadingTopic)
                return;

            var result = await ForumDialogBuilder.BuildGoPageDialog(Context);
            if (result == null)
                return;

            if (result == -1)
            {
                ViewModel.GotoFirstPageCommand.Execute(null);
            }
            else if (result == -2)
            {
                ViewModel.GotoLastPageCommand.Execute(null);
            }
            else
            {
                if (result == 0)
                    ViewModel.GotoPageTextBind = result.ToString();
                else
                    ViewModel.GotoPageTextBind = (result - 1).ToString();
                ViewModel.LoadGotoPageCommand.Execute(null);
            }
        }

        private async void ForumTopicPageActionButtonOnClick()
        {
            ResourceLocator.MessageDialogProvider.ShowMessageDialog("Sorry, but MAL has introduced recaptchas which limit what I can do on the forums." +
                                                                    " Forums will be back once their API is well enough featured and stable.", "Sorry.");
            return;
            
            var str = await TextInputDialogBuilder.BuildForumPostTextInputDialog(Context, TextInputDialogBuilder.ForumPostTextInputContext.Reply, "");
            if (!string.IsNullOrEmpty(str))
            {
                ViewModel.ReplyMessage = str;
                ViewModel.CreateReplyCommand.Execute(null);
            }
        }

        private List<ForumTopicItem> _items = new List<ForumTopicItem>();

        private View ContainerTemplate(int i)
        {
            return new FrameLayout(Activity) { LayoutParameters = new ViewGroup.LayoutParams(-1,-2)};
        }

        private void DataTemplateBasic(View view, int i, ForumTopicMessageEntryViewModel arg3)
        {
            var frame = view as FrameLayout;
            var item = _items[i];
            if (!(frame.ChildCount == 1 && frame.GetChildAt(0) as ForumTopicItem == item))
            {
                if(item.Parent is FrameLayout parent)
                    parent.RemoveView(item);
                frame.RemoveAllViews();
                frame.AddView(item);
            }
        }

        private void DataTemplateFling(View view, int i, ForumTopicMessageEntryViewModel arg3)
        {
            var item = _items[i];
            item.BindModelOnce(arg3, true);
            // ((ForumTopicItem)view).BindModel(arg3,true);
        }

        private void DataTemplateFull(View view, int i, ForumTopicMessageEntryViewModel arg3)
        {
            var item = _items[i];
            item.BindModelOnce(arg3, false);
            // ((ForumTopicItem)view).BindModel(arg3, false);
        }

        private void UpdatePageSelection()
        {
            if(ViewModel.AvailablePages != null)
                ForumTopicPageList.SetAdapter(ViewModel.AvailablePages.GetAdapter(GetPageItemTemplateDelegate));
        }

        private View GetPageItemTemplateDelegate(int i, Tuple<int, bool> tuple, View arg3)
        {
            var view = MainActivity.CurrentContext.LayoutInflater.Inflate(Resource.Layout.PageIndicatorItem, null);

            view.Click += PageItemOnClick;
            view.Tag = tuple.Item1;

            var backgroundPanel = view.FindViewById(Resource.Id.PageIndicatorItemBackgroundPanel);
            var textView = view.FindViewById<TextView>(Resource.Id.PageIndicatorItemNumber);
            if (tuple.Item2)
            {
                textView.SetTextColor(Color.White);
                backgroundPanel.SetBackgroundResource(ResourceExtension.AccentColourRes);
            }
            else
            {
                textView.SetTextColor(new Color(ResourceExtension.BrushText));
                backgroundPanel.SetBackgroundResource(ResourceExtension.BrushAnimeItemInnerBackgroundRes);
            }

            textView.Text = tuple.Item1.ToString();

            if (tuple.Item2)
                _prevHighlightedPageIndicator = view;

            return view;
        }

        private void PageItemOnClick(object sender, EventArgs eventArgs)
        {
            if(ViewModel.LoadingTopic)
                return;

            var view = sender as View;

            //update it immediatelly
            view.FindViewById(Resource.Id.PageIndicatorItemBackgroundPanel)
                .SetBackgroundResource(ResourceExtension.AccentColourRes);
            view.FindViewById<TextView>(Resource.Id.PageIndicatorItemNumber)
                .SetTextColor(Color.White);
            _prevHighlightedPageIndicator.FindViewById(Resource.Id.PageIndicatorItemBackgroundPanel)
                .SetBackgroundResource(ResourceExtension.BrushAnimeItemInnerBackgroundRes);
            _prevHighlightedPageIndicator.FindViewById<TextView>(Resource.Id.PageIndicatorItemNumber)
                .SetTextColor(new Color(ResourceExtension.BrushText));

            ViewModel.LoadPageCommand.Execute((int)view.Tag);

        }

        public override int LayoutResourceId => Resource.Layout.ForumTopicPage;

        public int GetFirstVisibleItemIndex()
        {
            return ForumTopicPagePostsList.FirstVisiblePosition;
        }

        #region Views

        private Button _forumTopicPageToggleWatchingButton;
        private ImageButton _forumTopicPageGotoPageButton;
        private LinearLayout _forumTopicPageList;
        private ListView _forumTopicPagePostsList;
        private ProgressBar _forumTopicPageLoadingSpinner;
        private FloatingActionButton _forumTopicPageActionButton;

        public Button ForumTopicPageToggleWatchingButton => _forumTopicPageToggleWatchingButton ?? (_forumTopicPageToggleWatchingButton = FindViewById<Button>(Resource.Id.ForumTopicPageToggleWatchingButton));

        public ImageButton ForumTopicPageGotoPageButton => _forumTopicPageGotoPageButton ?? (_forumTopicPageGotoPageButton = FindViewById<ImageButton>(Resource.Id.ForumTopicPageGotoPageButton));

        public LinearLayout ForumTopicPageList => _forumTopicPageList ?? (_forumTopicPageList = FindViewById<LinearLayout>(Resource.Id.ForumTopicPageList));

        public ListView ForumTopicPagePostsList => _forumTopicPagePostsList ?? (_forumTopicPagePostsList = FindViewById<ListView>(Resource.Id.ForumTopicPagePostsList));

        public ProgressBar ForumTopicPageLoadingSpinner => _forumTopicPageLoadingSpinner ?? (_forumTopicPageLoadingSpinner = FindViewById<ProgressBar>(Resource.Id.ForumTopicPageLoadingSpinner));

        public FloatingActionButton ForumTopicPageActionButton => _forumTopicPageActionButton ?? (_forumTopicPageActionButton = FindViewById<FloatingActionButton>(Resource.Id.ForumTopicPageActionButton));



        #endregion
    }
}