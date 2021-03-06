﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace InformationRetrievalManager
{
    /// <summary>
    /// Interaction logic for PageHost.xaml
    /// </summary>
    public partial class PageHost : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// The current page to show in the page host.
        /// </summary>
        public ApplicationPage CurrentPage
        {
            get => (ApplicationPage)GetValue(CurrentPageProperty);
            set => SetValue(CurrentPageProperty, value);
        }

        /// <summary>
        /// Registers <see cref="CurrentPage"/> as a DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register(nameof(CurrentPage), typeof(ApplicationPage), typeof(PageHost), new UIPropertyMetadata(default(ApplicationPage), null, CurrentPagePropertyChanged));

        /// <summary>
        /// The current page to show in the page host.
        /// </summary>
        public BaseViewModel CurrentPageViewModel
        {
            get => (BaseViewModel)GetValue(CurrentPageViewModelProperty);
            set => SetValue(CurrentPageViewModelProperty, value);
        }

        /// <summary>
        /// Registers <see cref="CurrentPageViewModel"/> as a DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty CurrentPageViewModelProperty =
            DependencyProperty.Register(nameof(CurrentPageViewModel), typeof(BaseViewModel), typeof(PageHost), new UIPropertyMetadata());

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public PageHost()
        {
            InitializeComponent();

            // If we are in DesignMode, show the current page as the dependency property does not fire.
            if (DesignerProperties.GetIsInDesignMode(this))
                NewPage.Content = new ApplicationViewModel().CurrentPage.ToBasePage();
        }

        #endregion

        #region Property Changed Events

        /// <summary>
        /// Called when the <see cref="CurrentPage"/> value has changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static object CurrentPagePropertyChanged(DependencyObject d, object value)
        {
            // Get current values from XAML binding.
            var currentPage = (ApplicationPage)value;
            var currentPageViewModel = d.GetValue(CurrentPageViewModelProperty);

            // Get the frames.
            var newPageFrame = (d as PageHost).NewPage;
            var oldPageFrame = (d as PageHost).OldPage;

            // If the current page hasn't changed, just update the view model.
            if (newPageFrame.Content is BasePage page && page.ToApplicationPage() == currentPage)
            {
                // Just update the view model.
                page.ViewModelObject = currentPageViewModel;

                return value;
            }

            // Store the current page content as the old page.
            var oldPageContent = newPageFrame.Content;

            // Remove current page from new page frame.
            newPageFrame.Content = null;

            // Move the previous page into the old page frame.
            oldPageFrame.Content = oldPageContent;

            bool lazyUnloadFlag = false;
            // Animate out previous page when the Loaded event fires right after this call due to moving frames.
            if (oldPageContent is BasePage oldPage)
            {
                oldPage.ShouldAnimateOut = true;

                // Set specific heavy-pages and make them ready for lazy-unload 
                // (make sure old page exists - thats why this call is inside if statement)
                lazyUnloadFlag = oldPage.LazyUnload > 0;

                // Once it is done, remove it.
                _ = Task.Delay((int)(oldPage.SlideSeconds * 1000 + oldPage.LazyUnload)).ContinueWith((t) =>
                {
                    // Remove old page.
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        oldPageFrame.Content = null;
                    });
                });
                // Load new page content after lazy unload delay...
                _ = Task.Delay((int)(oldPage.LazyUnload)).ContinueWith((t) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Lazy unload
                        if (lazyUnloadFlag)
                            // Set the new page content.
                            newPageFrame.Content = currentPage.ToBasePage(currentPageViewModel);
                    });
                });
            }

            // Set the new page content.
            if (!lazyUnloadFlag) newPageFrame.Content = currentPage.ToBasePage(currentPageViewModel);

            return value;
        }

        #endregion
    }
}
