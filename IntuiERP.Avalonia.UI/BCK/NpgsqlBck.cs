using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntuiERP.Avalonia.UI.BCK
{
    class NpgsqlBck
    {
        private readonly IDbConnection _connection;

        public NpgsqlBck(IDbConnection connection)
        {

            _connection = connection;
        }


    }
}
