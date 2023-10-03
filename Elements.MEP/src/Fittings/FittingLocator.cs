using System;
using Elements.Flow;

namespace Elements.Fittings
{
    public partial class FittingLocator : IComparable<FittingLocator>
    {
        public static FittingLocator Empty()
        {
            return new FittingLocator(
              "",
              "",
              int.MinValue
            );
        }

        public FittingLocator(Section section)
        {
            NetworkReference = section.Tree.GetNetworkReference();
            SectionKey = section.SectionKey;
            IndexInSection = int.MinValue;
        }

        public void MatchNetworkSection(FittingLocator other)
        {
            this.NetworkReference = other.NetworkReference;
            this.SectionKey = other.SectionKey;
        }

        public bool IsInSameSection(string sectionReference, string networkReference)
        {
            return this.NetworkReference == networkReference && this.SectionKey == sectionReference;
        }

        public bool IsInSameSection(Section section)
        {
            return IsInSameSection(section.SectionKey, section.Tree.GetNetworkReference());
        }
        public bool IsInSameSection(FittingLocator locator)
        {
            return IsInSameSection(locator.SectionKey, locator.NetworkReference);
        }

        public override string ToString()
        {
            return $"{this.NetworkReference}:{this.SectionKey}:{this.IndexInSection}";
        }

        public int CompareTo(FittingLocator other)
        {
            int compare = NetworkReference.CompareTo(other.NetworkReference);
            if (compare == 0)
            {
                compare = SectionKey.CompareTo(other.SectionKey);
                if (compare == 0)
                {
                    compare = IndexInSection.CompareTo(other.IndexInSection);
                }
            }
            return compare;
        }
    }
}