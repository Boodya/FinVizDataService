namespace FinVizScreener.Helpers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MapToTitleAttribute : Attribute
    {
        public string Title { get; }
        public MapToTitleAttribute(string title) => Title = title;
    }
}
