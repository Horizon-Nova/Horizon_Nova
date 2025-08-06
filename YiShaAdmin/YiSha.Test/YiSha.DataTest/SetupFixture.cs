using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using YiSha.Util;
using YiSha.Util.Model;

namespace YiSha.DataTest
{
    [SetUpFixture]
    public class SetupFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GlobalContext.SystemConfig = new SystemConfig
            {
                DBProvider = "MySql",
                DBConnectionString = "Server=shinkansen.proxy.rlwy.net;Port=48591;Database=YsData;User Id=root;Password=uMLFqLyRVyNiPQwpdVAaVRRbuSZNJVFe;",
                DBCommandTimeout = 180,
                DBBackup = "DataBase"
            };
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {

        }
    }
}
