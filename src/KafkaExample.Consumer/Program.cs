using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KafkaExample.Consumer
{
    class Program
    {
        private readonly static string _bootstrapServers = "127.0.0.1:9092";
        private readonly static string _topicName = "KafkaExampleTestTopic1";
        private readonly static string _groupId = "KafkaExampleTestConsumerGroup";

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
            catch (CreateTopicsException ex)
            {
                if (ex.Results[0].Error.Code != ErrorCode.TopicAlreadyExists)
                {
                    Console.WriteLine($"An error occured creating topic {_topicName}: {ex.Results[0].Error.Reason}");
                }
                else
                {
                    Console.WriteLine("Topic already exists");
                }
            }
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine("KafkaExample.Consumer started");
            await CreateTopic();

            var consumerConfig = new ConsumerConfig
            {
                GroupId = _groupId,
                BootstrapServers = _bootstrapServers,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true
            };

            using var c = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            c.Subscribe(_topicName);

            // Because Consume is a blocking call, we want to capture Ctrl+C and use a cancellation token to get out of our while loop and close the consumer gracefully.
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                while (true)
                {
                    // Consume a message from the test topic.  Pass in a cancellation token so we can break out of our loop when Ctrl+C is pressed

                    var cr = c.Consume(cts.Token);
                    Console.WriteLine($"Consumed message '{cr.Value}' from topic {cr.Topic}, partition {cr.Partition}, offset {cr.Offset}");

                    // Do something interesting with the message you consumed
                }
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine($"KafkaExample.Consumer OperationCanceledException : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"KafkaExample.Consumer exception : {ex.Message}");
            }
            finally
            {
                c.Close();
            }

            Console.WriteLine("KafkaExample.Consumer end");
        }
    }
}
