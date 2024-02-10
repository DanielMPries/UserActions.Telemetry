using System.Text;
using System.Text.Json;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.Reliable;

namespace UserActions.Telemetry.RabbitMQ;

public class RabbitMQUserActionRepository(StreamSystem streamSystem) : IUserActionRepository {
    private readonly StreamSystem _streamSystem = streamSystem;

    public async Task AddUserActionAsync(string Key, string userAction, Dictionary<string, string> contextData)
    {
        // create stream if it doesn't exist
        if(! await _streamSystem.StreamExists(Key)) {
            await _streamSystem.CreateStream(new StreamSpec(Key));
        };

        var producer = await Producer.Create( new ProducerConfig(_streamSystem, Key));
        var jsonData = JsonSerializer.Serialize(contextData);
        var message = new Message(Encoding.UTF8.GetBytes(jsonData));
        await producer.Send(message);
    }
}
