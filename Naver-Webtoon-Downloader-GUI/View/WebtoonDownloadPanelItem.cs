using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WRforest.Model;
using WRforest.View;

namespace WRforest.View
{
    class WebtoonDownloadPanelItem : Border
    {
        Grid webtoonDownloadGrid;

        CheckBox checkBox;
        Label titleLabel;
        Label writerLabel;
        Label lastEpisodeInfoLabel;
        Label stateLabel;
        ProgressBar progressBar;
        Label progressLabel;
        Label sizeLabel;
        Button runPauseButton;
        Button stopDeleteButton;

        Image runButtonImage = new Image() { Source = new BitmapImage(new Uri("Resources/Run_16x.png", UriKind.Relative)) };
        Image pauseButtonImage = new Image() { Source = new BitmapImage(new Uri("Resources/Pause_grey_16x.png", UriKind.Relative)) };
        Image stopButtonImage = new Image() { Source = new BitmapImage(new Uri("Resources/Stop_16x.png", UriKind.Relative)) };
        Image deleteButtonImage = new Image() { Source = new BitmapImage(new Uri("Resources/Cancel_16x.png", UriKind.Relative)) };

        public RoutedEventHandler RunButtonClickEventHandler;
        public RoutedEventHandler PauseButtonClickEventHandler;
        public RoutedEventHandler StopButtonClickEventHandler;
        public RoutedEventHandler DeleteButtonClickEventHandler;


        public WebtoonDownloadPanelItem(WebtoonDownloadInfo webtoonDownloadInfo, bool gray, ColumnDefinitionCollection columnDefinitions, RoutedEventHandler runButtonClickEventHandler, RoutedEventHandler pauseButtonClickEventHandler, RoutedEventHandler stopButtonClickEventHandler, RoutedEventHandler deleteButtonClickEventHandler)
        {
            RunButtonClickEventHandler = runButtonClickEventHandler;
            PauseButtonClickEventHandler = pauseButtonClickEventHandler;
            StopButtonClickEventHandler = stopButtonClickEventHandler;
            DeleteButtonClickEventHandler = deleteButtonClickEventHandler;
            int checkboxColumnIndex = 0;
            int titleColumnIndex = 1;
            int writerColumnIndex = 2;
            int lastEpisodeInfoColumnIndex = 3;
            int statusColumnIndex = 4;
            int progressColumnIndex = 5;
            int sizeColumnIndex = 6;
            int runPauseButtonColumnIndex = 7;
            int stopDeleteButtonColumnIndex = 8;
            if (gray)
            {
                Background = new SolidColorBrush(Color.FromRgb(0xf0, 0xf0, 0xf0));
            }
            else
            {
                Background = Brushes.White;
            }
            webtoonDownloadGrid = new Grid();
            Child = webtoonDownloadGrid;
            DataContext = webtoonDownloadInfo;

            //체크박스
            var checkBoxColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[checkboxColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(checkBoxColumn);
            checkBox = new CheckBox()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(checkBox, checkboxColumnIndex);
            webtoonDownloadGrid.Children.Add(checkBox);

            //제목
            var titleColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[titleColumnIndex].Width
            }; 
            webtoonDownloadGrid.ColumnDefinitions.Add(titleColumn);
            var titleBinding = new Binding("Title") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            titleLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            titleLabel.SetBinding(ContentControl.ContentProperty, titleBinding);
            Grid.SetColumn(titleLabel, titleColumnIndex);
            webtoonDownloadGrid.Children.Add(titleLabel);

            //작가
            var writerColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[writerColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(writerColumn);
            var writerBinding = new Binding("Writer") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            writerLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            writerLabel.SetBinding(ContentControl.ContentProperty, writerBinding);
            Grid.SetColumn(writerLabel, writerColumnIndex);
            webtoonDownloadGrid.Children.Add(writerLabel);

            //마지막 회차 정보
            var lastEpisodeInfoColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[lastEpisodeInfoColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(lastEpisodeInfoColumn);
            var lastEpisodeInfoBinding = new Binding("LastEpisodeInfo") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            lastEpisodeInfoLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            lastEpisodeInfoLabel.SetBinding(ContentControl.ContentProperty, lastEpisodeInfoBinding);
            Grid.SetColumn(lastEpisodeInfoLabel, lastEpisodeInfoColumnIndex);
            webtoonDownloadGrid.Children.Add(lastEpisodeInfoLabel);

            //상태
            var stateColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[statusColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(stateColumn);
            var stateBinding = new Binding("Status") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            stateLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            stateLabel.SetBinding(ContentControl.ContentProperty, stateBinding);
            Grid.SetColumn(stateLabel, statusColumnIndex);
            webtoonDownloadGrid.Children.Add(stateLabel);

            //진행상황
            var progressColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[progressColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(progressColumn);
            var progressBinding = new Binding("Progress") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            progressBar = new ProgressBar()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Maximum = 1,
                Minimum = 0,
                Height = 3,

                Padding = new Thickness(0)
            };
            progressBar.SetBinding(ProgressBar.ValueProperty, progressBinding);
            Grid.SetColumn(progressBar, progressColumnIndex);
            webtoonDownloadGrid.Children.Add(progressBar);

            //진행상황 텍스트
            var progressTextBinding = new Binding("ProgressText") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            progressLabel = new Label()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            progressLabel.SetBinding(ContentControl.ContentProperty, progressTextBinding);
            Grid.SetColumn(progressLabel, progressColumnIndex);
            webtoonDownloadGrid.Children.Add(progressLabel);

            //크기
            var sizeColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[sizeColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(sizeColumn);
            var sizeBinding = new Binding("Size") { Mode = BindingMode.OneWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
            sizeLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(2d, 0d, 2d, 0d),
                Padding = new Thickness(0)
            };
            sizeLabel.SetBinding(ContentControl.ContentProperty, sizeBinding);
            Grid.SetColumn(sizeLabel, sizeColumnIndex);
            webtoonDownloadGrid.Children.Add(sizeLabel);

            //runPause 버튼 
            var runPauseButtonColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[runPauseButtonColumnIndex].Width
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(runPauseButtonColumn);
            runPauseButton = new Button()
            {
                Margin = new Thickness(5d),
                Background = Brushes.White,
                BorderThickness = new Thickness(0d),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = pauseButtonImage
            };
            Grid.SetColumn(runPauseButton, runPauseButtonColumnIndex);
            webtoonDownloadGrid.Children.Add(runPauseButton);
            SetRunPauseButtonMode(RunPauseButtonMode.Pause);


            //delete 버튼
            var stopDeleteButtonColumn = new ColumnDefinition()
            {
                Width = columnDefinitions[stopDeleteButtonColumnIndex].Width,
            };
            webtoonDownloadGrid.ColumnDefinitions.Add(stopDeleteButtonColumn);
            stopDeleteButton = new Button()
            {
                Margin = new Thickness(5d),
                Background = Brushes.White,
                BorderThickness = new Thickness(0d),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = stopButtonImage
            };
            Grid.SetColumn(stopDeleteButton, stopDeleteButtonColumnIndex);
            webtoonDownloadGrid.Children.Add(stopDeleteButton);
            SetStopDeleteButtonMode(StopDeleteButtonMode.Stop);

        }//public WebtoonDownloadPanelItem(

        public void SetRunPauseButtonMode(RunPauseButtonMode mode)
        {
            if(mode == RunPauseButtonMode.Run)
            {
                runPauseButton.Click -= PauseButtonClickEventHandler;
                runPauseButton.Click += RunButtonClickEventHandler;
                runPauseButton.Content = runButtonImage;
            }
            else
            {
                runPauseButton.Click -= RunButtonClickEventHandler;
                runPauseButton.Click += PauseButtonClickEventHandler;
                runPauseButton.Content = pauseButtonImage;
            }
        }
        public void SetStopDeleteButtonMode(StopDeleteButtonMode mode)
        {
            if (mode == StopDeleteButtonMode.Stop)
            {
                stopDeleteButton.Click -= DeleteButtonClickEventHandler;
                stopDeleteButton.Click += StopButtonClickEventHandler;
                stopDeleteButton.Content = stopButtonImage;
            }
            else
            {
                stopDeleteButton.Click -= StopButtonClickEventHandler;
                stopDeleteButton.Click += DeleteButtonClickEventHandler;
                stopDeleteButton.Content = deleteButtonImage;
            }
        }
    }
    public enum RunPauseButtonMode
    {
        Run = 0,
        Pause = 1
    }
    public enum StopDeleteButtonMode
    {
        Stop = 10,
        Delete = 11
    }
}
