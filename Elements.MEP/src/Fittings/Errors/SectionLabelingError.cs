using Elements.Annotations;

namespace Elements.Fittings
{
    public class SectionLabelingError : FittingError
    {
        public string Section { get; }
        
        public string TreeName { get; }

        public SectionLabelingError(string section, string treeName, string text)
            : base(text)
        {
            Section = section;
            TreeName = treeName;
        }

        public override Message GetMessage()
        {
            return Message.FromText($"Section {Section} in {TreeName} failed to assign labels. {Text}.");
        }
    }
}
