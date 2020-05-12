﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Quiz
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            var main = new MainPage();
            MainPage = new NavigationPage(main);
            NavigationPage.SetHasNavigationBar(main, false);
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
