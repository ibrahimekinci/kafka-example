
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KafkaExample.Producer
{
    class Program
    {
        private readonly static string _bootstrapServers = "127.0.0.1:9092";
        private readonly static string _topicName = "KafkaExampleTestTopic1";

        private readonly static int _totalPartitions = 1;
        private readonly static short _totalReplications = 1;
        static async Task CreateTopic()
        {
            using var adminClient = new AdminClientBuilder(new ClientConfig() { BootstrapServers = _bootstrapServers }).Build();

            try
            {
                await adminClient.CreateTopicsAsync(new List<TopicSpecification> {
                     new TopicSpecification { Name = _topicName,
                                              NumPartitions = _totalPartitions,
                                              ReplicationFactor = _totalReplications
                                            }});

                Console.WriteLine($"Topic created ({_topicName})");
            }
            catch (CreateTopicsException e)
            {
                if (e.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                {
                    Console.WriteLine($"An error occured creating topic {_topicName}: {e.Results[0].Error.Reason}");
                }
                else
                {
                    Console.WriteLine("Topic already exists");
                }
            }
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("KafkaExample.Producer started");

            await CreateTopic();

            var config = new ClientConfig()
            {
                BootstrapServers = _bootstrapServers
            };

            // Create a producer that can be used to send messages to kafka that have no key and a value of type string 
            using var producer = new ProducerBuilder<Null, string>(config).Build();

            string textMessage = null;

            while (textMessage != "quit")
            {
                // Construct the message to send (generic type must match what was used above when creating the producer)
                Console.Write("Add Message: ");
                textMessage = Console.ReadLine();

                var message = new Message<Null, string>
                {
                    Value = $"Message {textMessage}"
                };

                // Send the message to our test topic in Kafka
                var dr = await producer.ProduceAsync(_topicName, message);
                Console.WriteLine($"Delivered '{dr.Value}' to topic {dr.Topic}, partition {dr.Partition}, offset {dr.Offset}");
            }


            Console.WriteLine("KafkaExample.Producer end");
        }
    }
}
