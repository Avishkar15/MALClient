﻿using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using PagerSlidingTab;

//version 1.0.9

namespace MALClient.Android.UserControls
{
    [global::Android.Runtime.Preserve(AllMembers = true)]
    public class PagerSlidingTabStrip : HorizontalScrollView, ViewPager.IOnPageChangeListener, ViewTreeObserver.IOnGlobalLayoutListener
    {

        /// <summary>
        /// Gets or sets the page change listener
        /// </summary>
        public global::Android.Support.V4.View.ViewPager.IOnPageChangeListener OnPageChangeListener { get; set; }
        /// <summary>
        /// Gets or sets the tab reselected listener
        /// </summary>
        public IOnTabReselectedListener OnTabReselectedListener { get; set; }


        public PagerSlidingTabStrip(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {

        }

        private static int[] Attrs = new int[]
        {
            global::Android.Resource.Attribute.TextColorPrimary,
            global::Android.Resource.Attribute.TextSize,
            global::Android.Resource.Attribute.TextColor,
      global::Android.Resource.Attribute.Padding,
            global::Android.Resource.Attribute.PaddingLeft,
            global::Android.Resource.Attribute.PaddingRight
        };

        //These indexes must be related with the ATTR array above
        private const int TextColorPrimaryIndex = 0;
        private const int TextSizeIndex = 1;
        private const int TextColorIndex = 2;
        private const int PaddingIndex = 3;
        private const int PaddingLeftIndex = 4;
        private const int PaddingRightIndex = 5;

        private LinearLayout.LayoutParams defaultTabLayoutParams;
        private LinearLayout.LayoutParams expandedTabLayoutParams;


        public LinearLayout TabsContainer { get; set; }
        private ViewPager pager;

        private int tabCount;

        private int currentPosition = 0;
        private float currentPositionOffset = 0f;

        private Paint rectPaint;
        private Paint dividerPaint;

        private int indicatorColor;
        /// <summary>
        /// Gets or sets the indicator color
        /// </summary>
        public int IndicatorColor
        {
            get { return indicatorColor; }
            set
            {
                indicatorColor = value;
                Invalidate();
            }
        }


        private int indicatorHeight = 2;
        /// <summary>
        /// Gets or sets the indicator height
        /// </summary>
        public int IndicatorHeight
        {
            get { return indicatorHeight; }
            set
            {
                indicatorHeight = value;
                Invalidate();
            }
        }

        private int underlineHeight = 0;
        /// <summary>
        /// Gets or sets the underline height
        /// </summary>
        public int UnderlineHeight
        {
            get { return underlineHeight; }
            set
            {
                underlineHeight = value;
                Invalidate();
            }
        }
        private int underlineColor;
        /// <summary>
        /// Gets or sets the underline color
        /// </summary>
        public int UnderlineColor
        {
            get { return underlineColor; }
            set
            {
                underlineColor = value;
                Invalidate();
            }
        }

        private int dividerWidth = 0;
        /// <summary>
        /// gets or sets the divider width
        /// </summary>
        public int DividerWidth
        {
            get { return dividerWidth; }
            set
            {
                dividerWidth = value;
                Invalidate();
            }
        }

        private int dividerPadding = 0;
        /// <summary>
        /// gets or sets the divider padding
        /// </summary>
        public int DividerPadding
        {
            get { return dividerPadding; }
            set
            {
                dividerPadding = value;
                Invalidate();
            }
        }
        private int dividerColor;
        /// <summary>
        /// gets or sets the divider color
        /// </summary>
        public int DividerColor
        {
            get { return dividerColor; }
            set
            {
                dividerColor = value;
                Invalidate();
            }
        }

        private int tabPadding = 12;
        /// <summary>
        /// Gets or sets padding left
        /// </summary>
        public int TabPaddingLeftRight
        {
            get { return tabPadding; }
            set
            {
                tabPadding = value;
                UpdateTabStyles();
            }
        }
        private int tabTextSize = 14;
        /// <summary>
        /// Gets or sets text size
        /// </summary>
        public int TabTextSize
        {
            get { return tabTextSize; }
            set
            {
                tabTextSize = value;
                UpdateTabStyles();
            }
        }

        private int textAlpha = 150;
        /// <summary>
        /// Gets or sets the text alpha
        /// </summary>
        public int TextAlpha
        {
            get { return textAlpha; }
            set
            {
                textAlpha = value;
                Invalidate();
            }
        }

        private ColorStateList tabTextColorSelected;
        /// <summary>
        /// Gets or sets the inactive text color
        /// </summary>
        public ColorStateList TabTextColorSelected
        {
            get { return tabTextColorSelected; }
            set
            {
                tabTextColorSelected = value;
                Invalidate();
            }
        }

        private ColorStateList tabTextColor = null;
        /// <summary>
        /// Gets or sets tab text color
        /// </summary>
        public ColorStateList TabTextColor
        {
            get { return tabTextColor; }
            set
            {
                tabTextColor = value;
                UpdateTabStyles();
            }
        }

        public void SetTabTextColor(int textColor)
        {
            TabTextColor = GetColorStateList(textColor);
        }

        public void SetTextColorResource(int resId)
        {
            TabTextColor = GetColorStateList(Resources.GetColor(resId));
        }

        public void SetTabTextColorListResource(int resId)
        {
            TabTextColor = Resources.GetColorStateList(resId);
        }


        private ColorStateList GetColorStateList(int textColor)
        {
            return new ColorStateList(new int[][] { new int[] { } }, new int[] { textColor });
        }

        private int paddingLeft = 0;
        private int paddingRight = 0;

        private bool shouldExpand = false;
        /// <summary>
        /// Gets or sets if should expand
        /// </summary>
        public bool ShouldExpand
        {
            get { return shouldExpand; }
            set
            {
                shouldExpand = value;
                if (pager != null)
                    RequestLayout();
            }
        }
        private bool textAllCaps = true;
        public bool TextAllCaps
        {
            get { return textAllCaps; }
            set
            {
                textAllCaps = value;
            }
        }
        private bool isPaddingMiddle = false;
        /// <summary>
        /// Gets or sets is padding middle
        /// </summary>
        public bool IsPaddingMiddle
        {
            get { return isPaddingMiddle; }
            set
            {
                isPaddingMiddle = value;
                Invalidate();
            }
        }

        private Typeface tabTypeface = null;
        /// <summary>
        /// Sets the typeface
        /// </summary>
        /// <param name="typeFace"></param>
        /// <param name="style"></param>
        public void SetTypeface(Typeface typeFace, TypefaceStyle style)
        {
            this.tabTypeface = typeFace;
            this.tabTypefaceSelectedStyle = style;
            UpdateTabStyles();
        }

        private TypefaceStyle tabTypefaceStyle = TypefaceStyle.Bold;
        private TypefaceStyle tabTypefaceSelectedStyle = TypefaceStyle.Bold;

        private int scrollOffset;
        /// <summary>
        /// Sets the scrolloffset
        /// </summary>
        public int ScrollOffset
        {
            get { return scrollOffset; }
            set
            {
                scrollOffset = value;
                Invalidate();
            }
        }
        private int lastScrollX = 0;

        private int tabBackgroundResId = global::PagerSlidingTab.Resource.Drawable.psts_background_tab;
        /// <summary>
        /// Sets tab background
        /// </summary>
        public int TabBackground
        {
            get { return tabBackgroundResId; }
            set
            {
                tabBackgroundResId = value;
            }
        }


        public PagerSlidingTabStrip(Context context)
            : this(context, null)
        {
        }

        public PagerSlidingTabStrip(Context context, IAttributeSet attrs)
            : this(context, attrs, 0)
        {
        }

        public PagerSlidingTabStrip(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            MyOnGlobalLayoutListner = new MyOnGlobalLayoutListener(this);
            FillViewport = true;
            this.VerticalScrollBarEnabled = false;
            this.HorizontalScrollBarEnabled = false;
            SetWillNotDraw(false);
            TabsContainer = new LinearLayout(context);
            TabsContainer.Orientation = global::Android.Widget.Orientation.Horizontal;
            TabsContainer.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            AddView(TabsContainer);

            var dm = Resources.DisplayMetrics;
            scrollOffset = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, scrollOffset, dm);
            indicatorHeight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, indicatorHeight, dm);
            underlineHeight = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, underlineHeight, dm);
            dividerPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dividerPadding, dm);
            tabPadding = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, tabPadding, dm);
            dividerWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dividerWidth, dm);
            tabTextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, tabTextSize, dm);

            //get system attrs (android:textSize and android:textColor)
            var a = context.ObtainStyledAttributes(attrs, Attrs);
            tabTextSize = a.GetDimensionPixelSize(TextSizeIndex, tabTextSize);
            var colorStateList = a.GetColorStateList(TextColorIndex);
            var textPrimaryColor = a.GetColor(TextColorPrimaryIndex, global::Android.Resource.Color.White);


            underlineColor = textPrimaryColor;
            dividerColor = textPrimaryColor;
            indicatorColor = textPrimaryColor;

            int padding = a.GetDimensionPixelSize(PaddingIndex, 0);
            paddingLeft = padding > 0 ? padding : a.GetDimensionPixelSize(PaddingLeftIndex, 0);
            paddingRight = padding > 0 ? padding : a.GetDimensionPixelSize(PaddingRightIndex, 0);



            //a = context.ObtainStyledAttributes(attrs, global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip);
            //indicatorColor = a.GetColor(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsIndicatorColor, indicatorColor);
            //underlineColor = a.GetColor(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsUnderlineColor, underlineColor);
            //dividerColor = a.GetColor(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsDividerColor, dividerColor);
            //dividerWidth = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsDividerWidth, dividerWidth);
            //indicatorHeight = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsIndicatorHeight, indicatorHeight);
            //underlineHeight = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsUnderlineHeight, underlineHeight);
            //dividerPadding = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsDividerPadding, dividerPadding);
            //tabPadding = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTabPaddingLeftRight, tabPadding);
            //tabBackgroundResId = a.GetResourceId(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTabBackground, tabBackgroundResId);
            //shouldExpand = a.GetBoolean(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsShouldExpand, shouldExpand);
            //scrollOffset = a.GetDimensionPixelSize(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsScrollOffset, scrollOffset);
            //textAllCaps = a.GetBoolean(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTextAllCaps, textAllCaps);
            //isPaddingMiddle = a.GetBoolean(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsPaddingMiddle, isPaddingMiddle);
            //tabTypefaceStyle = (TypefaceStyle)a.GetInt(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTextStyle, (int)TypefaceStyle.Bold);
            //tabTypefaceSelectedStyle = (TypefaceStyle)a.GetInt(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTextSelectedStyle, (int)TypefaceStyle.Bold);
            //tabTextColorSelected = a.GetColorStateList(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTextColorSelected);
            //textAlpha = a.GetInt(global::PagerSlidingTab.Resource.Styleable.PagerSlidingTabStrip_pstsTextAlpha, textAlpha);
            //a.Recycle();

            tabTextColor = colorStateList == null ? GetColorStateList(Color.Argb(textAlpha,
              Color.GetRedComponent(textPrimaryColor),
              Color.GetGreenComponent(textPrimaryColor),
              Color.GetBlueComponent(textPrimaryColor))) : colorStateList;

            tabTextColorSelected = tabTextColorSelected == null ? GetColorStateList(textPrimaryColor) : tabTextColorSelected;


            SetMarginBottomTabContainer();

            rectPaint = new Paint {AntiAlias = true};
            rectPaint.SetStyle(Paint.Style.Fill);

            dividerPaint = new Paint {AntiAlias = true, StrokeWidth = dividerWidth};

            defaultTabLayoutParams = new LinearLayout.LayoutParams(LayoutParams.WrapContent, LayoutParams.MatchParent);
            expandedTabLayoutParams = new LinearLayout.LayoutParams(0, LayoutParams.MatchParent, 1.0f);
        }


        private void SetMarginBottomTabContainer()
        {
            var mlp = (MarginLayoutParams)TabsContainer.LayoutParameters;
            var bottomMargin = indicatorHeight >= underlineHeight ? indicatorHeight : underlineHeight;
            mlp.SetMargins(mlp.LeftMargin, mlp.TopMargin, mlp.RightMargin, bottomMargin);
            TabsContainer.LayoutParameters = mlp;
        }

        public void SetViewPager(ViewPager pager)
        {
            this.pager = pager;
            if (pager.Adapter == null)
            {
                throw new ArgumentNullException("ViewPager does not have adapter instance.");
            }

            pager.SetOnPageChangeListener(this);
            NotifyDataSetChanged();
        }

        public void NotifyDataSetChanged()
        {
            TabsContainer.RemoveAllViews();
            tabCount = pager.Adapter.Count;
            for (int i = 0; i < tabCount; i++)
            {
                View tabView;
                if (pager.Adapter is ICustomTabProvider provider)
                {
                    tabView = provider.GetCustomTabView(this, i);
                }
                else
                {
                    tabView = LayoutInflater.From(Context).Inflate(global::PagerSlidingTab.Resource.Layout.psts_tab, this, false);
                }

                var title = pager.Adapter.GetPageTitle(i);

                AddTab(i, title, tabView);
            }

            UpdateTabStyles();
            ViewTreeObserver.AddOnGlobalLayoutListener(MyOnGlobalLayoutListner);

        }

        protected MyOnGlobalLayoutListener MyOnGlobalLayoutListner { get; set; }

        protected class MyOnGlobalLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
        {
            PagerSlidingTabStrip strip;
            public MyOnGlobalLayoutListener(PagerSlidingTabStrip strip)
            {
                this.strip = strip;
            }
            #region IOnGlobalLayoutListener implementation
            public void OnGlobalLayout()
            {
                strip.RemoveGlobals();
            }
            #endregion

        }

        private void RemoveGlobals()
        {
            if ((int)Build.VERSION.SdkInt < 16)
            {
                ViewTreeObserver.RemoveGlobalOnLayoutListener(MyOnGlobalLayoutListner);
            }
            else
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(MyOnGlobalLayoutListner);
            }
        }

        private void AddTab(int position, string title, View tabView)
        {
            var textView = tabView.FindViewById<TextView>(global::PagerSlidingTab.Resource.Id.psts_tab_title);
            if (textView != null)
            {
                if (title != null)
                {
                    textView.Text = title;
                }
            }

            tabView.Focusable = true;

            tabView.Click += (object sender, EventArgs e) =>
            {
                if (pager.CurrentItem != position)
                {
                    var tab = TabsContainer.GetChildAt(pager.CurrentItem);
                    NotSelected(tab);
                    pager.SetCurrentItem(position, true);
                }
                else if (OnTabReselectedListener != null)
                {
                    OnTabReselectedListener.OnTabReselected(position);
                }
            };

            TabsContainer.AddView(tabView, position, shouldExpand ? expandedTabLayoutParams : defaultTabLayoutParams);
        }

        private void UpdateTabStyles()
        {
            for (int i = 0; i < tabCount; i++)
            {
                var v = TabsContainer.GetChildAt(i);
                if (v == null)
                    continue;
                v.SetBackgroundResource(tabBackgroundResId);
                v.SetPadding(tabPadding, v.PaddingTop, tabPadding, v.PaddingBottom);
                var tab_title = v.FindViewById<TextView>(global::PagerSlidingTab.Resource.Id.psts_tab_title);

                if (tab_title != null)
                {
                    tab_title.SetTextSize(ComplexUnitType.Px, tabTextSize);

                    // setAllCaps() is only available from API 14, so the upper case is made manually if we are on a
                    // pre-ICS-build
                    if (textAllCaps)
                    {
                        if ((int)Build.VERSION.SdkInt >= 14)
                        {
                            tab_title.SetAllCaps(true);
                        }
                        else
                        {
                            tab_title.Text = tab_title.Text.ToUpperInvariant();
                        }
                    }
                }
            }
        }

        public void ScrollToChild(int position, int offset)
        {
            if (tabCount == 0)
                return;
            var child = TabsContainer.GetChildAt(position);
            if (child == null)
                return;
            int newScrollX = child.Left + offset;
            if (position > 0 || offset > 0)
            {

                //Half screen offset.
                //- Either tabs start at the middle of the view scrolling straight away
                //- Or tabs start at the begging (no padding) scrolling when indicator gets
                //  to the middle of the view width
                newScrollX -= scrollOffset;
                float first, second = 0f;
                GetIndicatorCoordinates(out first, out second);
                newScrollX += (int)((second - first) / 2f);
            }

            if (newScrollX != lastScrollX)
            {
                lastScrollX = newScrollX;
                ScrollTo(newScrollX, 0);
            }
        }

        private void GetIndicatorCoordinates(out float lineLeft, out float lineRight)
        {
            lineLeft = 0f;
            lineRight = 0f;
            var currentTab = TabsContainer.GetChildAt(currentPosition);
            if (currentTab == null)
                return;
            lineLeft = currentTab.Left;
            lineRight = currentTab.Right;

            // if there is an offset, start interpolating left and right coordinates between current and next tab
            if (currentPositionOffset > 0f && currentPosition < tabCount - 1)
            {

                View nextTab = TabsContainer.GetChildAt(currentPosition + 1);
                float nextTabLeft = nextTab.Left;
                float nextTabRight = nextTab.Right;

                lineLeft = (currentPositionOffset * nextTabLeft + (1f - currentPositionOffset) * lineLeft);
                lineRight = (currentPositionOffset * nextTabRight + (1f - currentPositionOffset) * lineRight);
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            if (isPaddingMiddle || paddingLeft > 0 || paddingRight > 0)
            {
                TabsContainer.SetMinimumWidth(Width);
                SetClipToPadding(false);
            }

            if (TabsContainer.ChildCount > 0)
            {
                TabsContainer.GetChildAt(0).ViewTreeObserver.AddOnGlobalLayoutListener(this);
            }
            base.OnLayout(changed, left, top, right, bottom);
        }

        public void OnGlobalLayout()
        {
            var view = TabsContainer.GetChildAt(0);

            if ((int)Build.VERSION.SdkInt < 16)
            {
                ViewTreeObserver.RemoveGlobalOnLayoutListener(this);
            }
            else
            {
                ViewTreeObserver.RemoveOnGlobalLayoutListener(this);
            }

            if (isPaddingMiddle)
            {
                int halfWidthFirstTab = view.Width / 2;
                paddingLeft = paddingRight = Width / 2 - halfWidthFirstTab;
            }

            SetPadding(paddingLeft, PaddingTop, paddingRight, PaddingBottom);
            if (scrollOffset == 0)
                scrollOffset = Width / 2 - paddingLeft;


            currentPosition = pager.CurrentItem;
            currentPositionOffset = 0f;
            ScrollToChild(currentPosition, 0);
            UpdateSelection(currentPosition);
        }



        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (IsInEditMode || tabCount == 0)
                return;


            try
            {


                var height = Height;
                //draw indicator line
                rectPaint.Color = new Color(indicatorColor);
                float first, second = 0f;
                GetIndicatorCoordinates(out first, out second);
                canvas.DrawRect(first + paddingLeft, height - indicatorHeight, second + paddingLeft, height, rectPaint);

                //draw underline
                rectPaint.Color = new Color(underlineColor);
                canvas.DrawRect(paddingLeft, height - underlineHeight, TabsContainer.Width + paddingRight, height,
                    rectPaint);

                //draw divider
                if (dividerWidth <= 0)
                    return;

                dividerPaint.StrokeWidth = dividerWidth;
                dividerPaint.Color = new Color(dividerColor);

                var offset = IsPaddingMiddle ? paddingLeft : 0F;

                for (int i = 0; i < tabCount - 1; i++)
                {
                    var tab = TabsContainer.GetChildAt(i);
                    if (tab != null)
                    {
                        canvas.DrawLine(offset + tab.Right, dividerPadding, offset + tab.Right, height - dividerPadding,
                            dividerPaint);

                    }
                }
            }
            catch
            {
                //disposed
            }
        }


        #region IOnPageChangeListener Implentation
        public void OnPageScrolled(int position, float positionOffset, int positionOffsetPixels)
        {
            currentPosition = position;
            currentPositionOffset = positionOffset;
            var child = TabsContainer.GetChildAt(position);
            if (child == null)
                return;
            var offset = tabCount > 0 ? (int)(positionOffset * child.Width) : 0;
            ScrollToChild(position, offset);
            Invalidate();
            if (OnPageChangeListener != null)
            {
                OnPageChangeListener.OnPageScrolled(position, positionOffset, positionOffsetPixels);
            }
        }

        public void OnPageScrollStateChanged(int state)
        {
            if (state == ViewPager.ScrollStateIdle)
            {
                ScrollToChild(pager.CurrentItem, 0);
            }
            //Full textAlpha for current item
            var currentTab = TabsContainer.GetChildAt(pager.CurrentItem);
            Selected(currentTab);
            //Half transparent for prev item
            if (pager.CurrentItem - 1 >= 0)
            {
                var prevTab = TabsContainer.GetChildAt(pager.CurrentItem - 1);
                NotSelected(prevTab);
            }
            //Half transparent for next item
            if (pager.CurrentItem + 1 <= pager.Adapter.Count - 1)
            {
                View nextTab = TabsContainer.GetChildAt(pager.CurrentItem + 1);
                NotSelected(nextTab);
            }

            if (OnPageChangeListener != null)
            {
                OnPageChangeListener.OnPageScrollStateChanged(state);
            }
        }



        public void OnPageSelected(int position)
        {
            UpdateSelection(position);
            if (OnPageChangeListener != null)
            {
                OnPageChangeListener.OnPageSelected(position);
            }
        }
        #endregion

        private void UpdateSelection(int position)
        {
            for (int i = 0; i < tabCount; ++i)
            {
                var tv = TabsContainer.GetChildAt(i);
                if (tv == null)
                    continue;

                var selected = i == position;
                tv.Selected = selected;
                if (selected)
                {
                    Selected(tv);
                }
                else
                {
                    NotSelected(tv);
                }
            }
        }

        void NotSelected(View tab)
        {
            if (tab == null)
                return;
            (pager.Adapter as ICustomTabProvider)?.TabUnselected(tab);
            var title = tab.FindViewById<TextView>(global::PagerSlidingTab.Resource.Id.psts_tab_title);
            if (title == null)
                return;

            title.SetTypeface(tabTypeface, tabTypefaceStyle);
            title.SetTextColor(tabTextColor);


        }

        void Selected(View tab)
        {
            if (tab == null)
                return;
            (pager.Adapter as ICustomTabProvider)?.TabSelected(tab);
            var title = tab.FindViewById<TextView>(global::PagerSlidingTab.Resource.Id.psts_tab_title);
            if (title == null)
                return;

            title.SetTypeface(tabTypeface, tabTypefaceSelectedStyle);
            title.SetTextColor(tabTextColorSelected);


        }
    }
}

