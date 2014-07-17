﻿using io.ebu.eis.canvasgenerator;
using io.ebu.eis.datastructures;
using io.ebu.eis.datastructures.Plain.Collections;
using io.ebu.eis.mq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

namespace io.ebu.eis.contentmanager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IAMQDataMessageHandler
    {
        CMConfigurationSection _config;
        ManagerContext _context;

        AMQConsumer _dataInConnection;

        HTMLRenderer _renderer;

        public MainWindow()
        {
            _config = (CMConfigurationSection)ConfigurationManager.GetSection("CMConfiguration");

            // Data Context
            _context = new ManagerContext();
            DataContext = _context;

            InitializeComponent();

            _context.DummyData();

            _renderer = new HTMLRenderer(_config.SlideConfiguration.TemplatePath);

            // Open Connection to INBOUND MQ
            var amquri = _config.MQConfiguration.Uri;
            var amqexchange = _config.MQConfiguration.DPExchange;
            _dataInConnection = new AMQConsumer(amquri, amqexchange, this);

            // TODO Catch hand handle connection
            _dataInConnection.Connect();

        }

        public void OnReceive(DataMessage message)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;

                if (_config.DataConfiguration.DataFlowTypes.Split(';').Contains(message.DataType))
                {
                    // TODO Generalize Message creation
                    // Add the message to the data flow
                    DataFlowItem d = new DataFlowItem()
                    {
                        DataMessage = message,
                        Name = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).NamePath),
                        Category = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).CategoryPath),
                        Type = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).TypePath),
                        Short = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).ShortPath),
                        Priority = DataFlowPriority.Low
                    };

                    _context.DataFlowItems.Add(d);
                }
                if (_config.DataConfiguration.ImageFlowTypes.Split(';').Contains(message.DataType))
                {
                    // Add the message to the image flow
                    DataFlowItem d = new DataFlowItem()
                    {
                        DataMessage = message,
                        Name = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).NamePath),
                        Category = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).CategoryPath),
                        Type = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).TypePath),
                        Short = message.GetValue(_config.DataConfiguration.GetPathByDataType(message.DataType).ShortPath),
                        Priority = DataFlowPriority.Low
                    };

                    _context.ImageFlowItems.Add(d);
                }

                if (_config.DataConfiguration.DataBaseTypes.Split(';').Contains(message.DataType))
                {
                    // Add the message to the database
                    _context.UpdateDataBase(message);
                }

            }, null);

        }

        private BitmapImage renderImageWithTemplateContext(string template, DataMessage context)
        {
            var filename = System.IO.Path.Combine(_config.SlideConfiguration.TemplatePath, template);
            var templateHtml = File.ReadAllText(filename);

            // Replace @@values@@ with context Values
            var pattern = "@@(.*?)@@";
            foreach (Match m in Regex.Matches(templateHtml, pattern))
            {
                var variable = m.Groups[1].Value;
                var matchedValue = m.Value;
                var replaceValue = context.GetValue(variable);
                templateHtml = templateHtml.Replace(matchedValue, replaceValue);
            }

            return _renderer.RenderHtml(templateHtml);
        }

        private void rendreButtonTest_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Render,
            (SendOrPostCallback)delegate
            {
                _context = (ManagerContext)DataContext;

                _context.MainImage = _renderer.Render("zurichsample001.html");

            }, null);

        }

        private void DataFlowList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (sender is ListBox)
                {
                    var box = sender as ListBox;
                    if (box.SelectedItem is DataFlowItem)
                    {
                        var m = box.SelectedItem as DataFlowItem;
                        var template = "index.html";
                        switch(m.DataMessage.DataType)
                        {
                            case "WEATHER": template = "weather.html"; break;
                            case "STARTLIST": template = "startlist.html"; break;
                        }
                        _context.PreviewImage = renderImageWithTemplateContext(template, m.DataMessage);
                    }
                }
            } catch (Exception ex)
            {
                // TODO HAndle and not generic Exceltion
            }
        }
    }
}
