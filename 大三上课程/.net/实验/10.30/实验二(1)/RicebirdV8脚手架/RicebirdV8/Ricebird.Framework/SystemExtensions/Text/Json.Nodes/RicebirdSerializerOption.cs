using Ricebird.Framework.Tools.JsonConverters;

namespace System.Text.Json.Nodes
{
    public static class RicebirdSerializerOption
    {
        //static RicebirdSerializerOption()
        //{
        //    _default = new JsonSerializerOptions(JsonSerializerDefaults.General);
        //    _default.AddConverter<RicebirdGuidConverter>();
        //    _default.AddConverter<SequentialGuidConverter>();
        //    _default.AddConverter<SystemCodeConverter>();
        //}

        //private static readonly JsonSerializerOptions _default;
        public static JsonSerializerOptions Default
        {
            get
            {
                JsonSerializerOptions _default = new JsonSerializerOptions(JsonSerializerDefaults.General);
                _default.AddConverter<RicebirdGuidConverter>();
                _default.AddConverter<SequentialGuidConverter>();
                _default.AddConverter<SystemCodeConverter>();
                _default.AddConverter<DateRangeConverter>();
                return _default;
            }
        }
    }
}
