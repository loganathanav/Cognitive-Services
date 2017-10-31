using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host;

namespace Function.MediaSummaryQ.Model
{
    public partial class ZetronDbContext
    {
        private readonly string _connectionString;
        private readonly TraceWriter _log;
        public ZetronDbContext(string connectionString, TraceWriter log)
        {
            _connectionString = connectionString;
            _log = log;
        }

        public int UpdateSummaryUrl(int mediaId, string mediaSummaryUrl)
        {
            _log.Info($"UpdateSummaryUrl method entry");
            try
            {
                using (var context = new DbContext(_connectionString))
                {
                    context.Database.Connection.Open();
                    var mediaIdParam = new SqlParameter("@mediaId", SqlDbType.Int) {Value = mediaId};
                    var mediaSummaryUrlParam =
                        new SqlParameter("@mediaSummaryUrl", SqlDbType.VarChar) {Value = mediaSummaryUrl};
                    context.Database.ExecuteSqlCommand($"exec dbo.UpdateMediaSammaryUrl @mediaId, @mediaSummaryUrl",
                        mediaIdParam, mediaSummaryUrlParam);
                    context.Database.Connection.Close();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                _log.Error($"UpdateSummaryUrl method DbUpdateException: ", ex);
                return 0;
            }
            catch (Exception ex)
            {
                _log.Error($"UpdateSummaryUrl method Exception: ", ex);
                return 0;
            }
            _log.Info($"Summary url updation completed.");
            return 1;
        }

    }
}
