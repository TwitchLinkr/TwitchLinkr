using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using TwitchLinkr.TwitchAPI.APIResponseModels;

internal class WebsocketMessageConverter : JsonConverter<WebsocketMessage<Metadata, Payload>>
{
    public override WebsocketMessage<Metadata, Payload> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
        {
            JsonElement root = doc.RootElement;
            JsonElement metadataElement = root.GetProperty("metadata");
            string messageType = metadataElement.GetProperty("message_type").GetString()!;

            Type metadataType = typeof(Metadata);
            Type payloadType = typeof(Payload);

            switch (messageType)
            {
                case "notification":
                    metadataType = typeof(NotificationMetadata);
                    payloadType = typeof(NotificationPayload);
                    break;
                case "session_welcome":
                    metadataType = typeof(Metadata);
                    payloadType = typeof(ServicePayload);
                    break;
                case "session_keepalive":
					metadataType = typeof(Metadata);
					payloadType = typeof(ServicePayload);
					break;
                case "session_reconnect":
                    metadataType = typeof(Metadata);
                    payloadType = typeof(ServicePayload);
                    break;
                case "session_revocation":
					metadataType = typeof(Metadata);
					payloadType = typeof(ServicePayload);
					break;
                default:
                    throw new NotSupportedException($"Message type '{messageType}' is not supported");
            }

            var metadata = JsonSerializer.Deserialize(metadataElement.GetRawText(), metadataType, options) as Metadata;
            var payload = JsonSerializer.Deserialize(root.GetProperty("payload").GetRawText(), payloadType, options) as Payload;

            var messageTypeInstance = typeof(WebsocketMessage<,>).MakeGenericType(metadataType, payloadType);
            var message = Activator.CreateInstance(messageTypeInstance) as WebsocketMessage<Metadata, Payload>;
            message!.Metadata = metadata;
            message!.Payload = payload;

            return message;
        }
    }

    public override void Write(Utf8JsonWriter writer, WebsocketMessage<Metadata, Payload> value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}
