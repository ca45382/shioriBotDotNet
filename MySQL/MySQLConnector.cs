using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;

using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.Script;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLConnector : DbContext
    {

        public DbSet<BotDatabase> BotDatabase { get; set; }
        public JsonDataManager JsonData;

        public MySQLConnector()
        {
            JsonData = new JsonDataManager();
        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(JsonData.MySQLConnectionString());

    }
}
