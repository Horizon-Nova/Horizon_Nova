using System;
using HNB.Areas.WW.Models;
using HNB.Areas.WW.Utilities;

namespace HNB.Areas.WW.Services
{
    public interface IHistoryService
    {
        HistoryIndexModel QueryHistoryIndexModel(DateTime nowLocal);
    }

    public class HistoryService : IHistoryService
    {
        #region Public Methods

        public HistoryIndexModel QueryHistoryIndexModel(DateTime nowLocal)
            => HistoryUtilities.QueryHistoryIndexModel(nowLocal);

        #endregion
    }
}

