﻿using io.ebu.eis.datastructures;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SMPAG.MM.MMConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace io.ebu.eis.mq
{
    public class AMQQueuePublisher
    {
        private string _amqpUri;
        private string _amqpExchange;

        private AMQClient _amq;

        public AMQQueuePublisher(string uri, string exchange)
        {
            _amqpUri = uri;
            _amqpExchange = exchange;
        }

        public void Connect(string filter = "#")
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = _amqpUri;

            var conn = factory.CreateConnection();
            _amq = new AMQClient();
            _amq.channel = conn.CreateModel();
            _amq.channel.QueueDeclare(_amqpExchange, true, false, false, null);

            Console.WriteLine("AMQPublisher started and connected to queue " + _amqpUri + ":" + _amqpExchange);
        }

        public void Dispatch(DispatchNotificationMessage message)
        {
            // Create Persistence
            var properties = _amq.channel.CreateBasicProperties();
            properties.SetPersistent(true);

            _amq.channel.BasicPublish("", _amqpExchange, properties, Encoding.UTF8.GetBytes(message.Serialize()));
        }

    }
}
