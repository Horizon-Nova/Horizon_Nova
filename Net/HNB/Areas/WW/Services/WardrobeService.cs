using HNB.Areas.WW.Models;
using HNB.Areas.WW.Utilities;

namespace HNB.Areas.WW.Services
{
    public interface IWardrobeService
    {
        WardrobeIndexModel QueryWardrobeIndexModel();
    }

    public class WardrobeService : IWardrobeService
    {
        #region Public Methods

        public WardrobeIndexModel QueryWardrobeIndexModel()
            => WardrobeUtilities.QueryWardrobeIndexModel();

        #endregion
    }
}

