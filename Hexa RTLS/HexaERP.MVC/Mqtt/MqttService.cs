using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Threading;
using System.Threading.Tasks;

namespace HexaERP.MVC
{
    public class MqttService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IMqttClientOptions _mqttClientOptions;

        public MqttService()
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();
            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("192.168.1.3", 1883) // Connect to local Mosquitto broker
                .Build();
        }

        public async Task ConnectAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttClientOptions, CancellationToken.None);
            }
        }
        public async Task PublishAsync(string topic, string payload)
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync();
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .Build();

            await _mqttClient.PublishAsync(message, CancellationToken.None);
        }
    }
}