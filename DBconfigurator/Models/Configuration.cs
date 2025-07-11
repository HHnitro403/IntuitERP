﻿using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TableAttribute = SQLite.TableAttribute;

namespace DBconfigurator.Models
{
    [Table("Connection")]
    public class Configuration
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        [NotNull]
        public string Server { get; set; }

        [NotNull]
        public string DataBase { get; set; }

        [NotNull]
        public string User { get; set; }

        [NotNull]
        public string Password { get; set; }
    }
}