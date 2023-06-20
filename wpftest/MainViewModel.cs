﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TBird.Core;
using TBird.Wpf;
using TBird.Wpf.Controls;

namespace wpftest
{
    public class MainViewModel : WindowViewModel
    {
        public MainViewModel()
        {
            Loaded.Add(() => Task.Delay(new Random().Next(1000, 3000)));

            Closing.Add(() =>
            {
                Command.Dispose();
                //using (Command.Lock())
                {
                    MessageBox.Show("test");
                }
            });
        }

        public string Text
        {
            get => _Text;
            set => SetProperty(ref _Text, value);
        }
        private string _Text;

        public IRelayCommand Command => _Command = _Command ?? RelayCommand.Create(async _ =>
        {
            Text += "Command: lock: ";
            Text += this.LockCount();
            Text += "index: " + _index++;
            Text += "\n";
            Text += "B:" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss.fff ");
            await Task.Delay(new Random().Next(1000, 5000));
            Text += "E:" + DateTime.Now.ToString("yyyy.MM.dd-HH:mm:ss.fff ");
            Text += "\n";
        });
        private IRelayCommand _Command;
        private int _index;

        public IRelayCommand DragDrop => _DragDrop = _DragDrop ?? RelayCommand.Create<DragEventArgs>(e =>
        {
            var data = e.Data;
            var url = e.Data.GetData(DataFormats.Text);
            MessageService.Debug(url as string);
        });
        private IRelayCommand _DragDrop;

    }
}
