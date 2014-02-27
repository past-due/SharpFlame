#region

using SharpFlame.Old.FileIO;
using SharpFlame.Core.Domain;
using SharpFlame.Old.Maths;

#endregion

namespace SharpFlame.Old.Mapping
{
    public class InterfaceOptions
    {
        public bool AutoScrollLimits;
        public int CampaignGameType;
        public string CompileMultiAuthor;
        public string CompileMultiLicense;
        public string CompileMultiPlayers;
        public string CompileName;
        public sXY_uint ScrollMax;
        public XYInt ScrollMin;

        public CompileType CompileType;

        public InterfaceOptions()
        {
            //set to default
            CompileName = "";
            CompileMultiPlayers = 2.ToStringInvariant();
            CompileMultiAuthor = "";
            CompileMultiLicense = "";
            AutoScrollLimits = true;
            ScrollMin = new XYInt(0, 0);
            ScrollMax.X = 0U;
            ScrollMax.Y = 0U;
            CampaignGameType = -1;
            CompileType = CompileType.Unspecified;
        }
    }

    public enum CompileType
    {
        Unspecified,
        Multiplayer,
        Campaign
    }
}