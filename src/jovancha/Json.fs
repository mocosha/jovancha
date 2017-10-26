namespace Jovancha

module Json =
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization

    let serializerSettings =
        JsonSerializerSettings (
            ContractResolver = CamelCasePropertyNamesContractResolver (),
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            // NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.Indented,
            Converters = [|Fable.JsonConverter ()|]
        )

    let applyGlobalJsonSettings () =
        JsonConvert.DefaultSettings <- fun _ -> serializerSettings

    let serialize x = JsonConvert.SerializeObject (x, serializerSettings)
    let deserialize<'a> x = JsonConvert.DeserializeObject<'a> (x, serializerSettings)