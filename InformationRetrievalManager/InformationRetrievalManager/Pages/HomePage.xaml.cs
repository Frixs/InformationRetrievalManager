﻿namespace InformationRetrievalManager
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : BasePage<HomePageViewModel>
    {
        public HomePage()
        {
            InitializeComponent();
            LazyUnload = 250;
        }
    }
}
