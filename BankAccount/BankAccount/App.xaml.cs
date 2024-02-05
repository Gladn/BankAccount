﻿using BankAccount.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BankAccount
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        public IServiceProvider Container { get; }

        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода, поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Container = ConfigureDependencyInjection();
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем. Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;


            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // навигации
                    rootFrame.Navigate(typeof(View.MainBankAccountPage), e.Arguments);
                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();
            }


            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;

            ApplicationViewTitleBar tb = ApplicationView.GetForCurrentView().TitleBar;

            tb.BackgroundColor = Windows.UI.Colors.LightGray;
            tb.ButtonBackgroundColor = Windows.UI.Colors.LightGray;
            tb.ButtonForegroundColor = Windows.UI.Colors.Black;
            tb.ButtonHoverBackgroundColor = Windows.UI.Colors.Blue;
            tb.ButtonHoverForegroundColor = Windows.UI.Colors.White;
            tb.ButtonPressedBackgroundColor = Windows.UI.Colors.DarkBlue;
            tb.ButtonPressedForegroundColor = Windows.UI.Colors.White;
            tb.ForegroundColor = Windows.UI.Colors.Black;
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }


        IServiceProvider ConfigureDependencyInjection()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IDataBaseService, DataBaseService>();
            serviceCollection.AddScoped<ICurrencyApiService, CurrencyApiService>();
            serviceCollection.AddScoped<ICurrencyConverterService, CurrencyConverterService>();
            serviceCollection.AddScoped<IDataBalanceService, DataBalanceService>();
            serviceCollection.AddScoped<IDataCurrencyService, DataCurrencyService>();
            serviceCollection.AddScoped<IDataTransactionService, DataTransactionService>();


            return serviceCollection.BuildServiceProvider();
        }
    }
}
