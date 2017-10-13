using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;

namespace Function.MediaQ.Model
{
    public class ZetronDbContext
    {
        private readonly string _connectionString;
        private readonly TraceWriter _log;
        public ZetronDbContext(string connectionString, TraceWriter log)
        {
            _connectionString = connectionString;
            _log = log;
        }
        public int AddTags(IEnumerable<FrameTag> tags)
        {
            _log.Info($"Add Tag method entry");
            try
            {
                using (var context = new DbContext(_connectionString))
                {
                    var tagsTable = new DataTable();
                    tagsTable.Columns.Add("MediaId", typeof(int));
                    tagsTable.Columns.Add("FrameTime", typeof(DateTime));
                    tagsTable.Columns.Add("Tag", typeof(string));
                    tagsTable.Columns.Add("ConfidenceLevel", typeof(double));
                    foreach (var tag in tags)
                    {
                        var dr = tagsTable.NewRow();
                        dr["MediaId"] = tag.MediaId;
                        dr["FrameTime"] = tag.FrameTime;
                        dr["Tag"] = tag.Tag;
                        dr["ConfidenceLevel"] = tag.ConfidenceLevel;
                        tagsTable.Rows.Add(dr);
                    }

                    context.Database.Connection.Open();
                    var param = new SqlParameter("@FrameTags", SqlDbType.Structured) { Value = tagsTable, TypeName = "dbo.FrameTag" };
                    context.Database.ExecuteSqlCommand($"exec dbo.AddTags @FrameTags", param);
                    context.Database.Connection.Close();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
            {
                _log.Error($"Add Tag method DbUpdateException: ", ex);
            }
            catch (Exception ex)
            {
                _log.Error($"Add Tag method Exception: ", ex);
            }

            return 1;
        }

        public IncidentStatus CheckEventState(int mediaId)
        {
            _log.Info($"CheckEventState method entry");
            IncidentStatus status;
            try
            {
                using (var context = new DbContext(_connectionString))
                {
                    context.Database.Connection.Open();
                    status = (IncidentStatus)context.Database.SqlQuery<int>(
                        $"SELECT I.Status AS IncidentStatus FROM ZetronMstIncidents I WITH(NOLOCK) INNER JOIN  ZetronTrnMediaDetails M WITH(NOLOCK) ON I.IncidentID=M.IncidentID WHERE M.MediaID=@MediaId",
                        new SqlParameter("@MediaId", mediaId)).FirstAsync().Result;

                    context.Database.Connection.Close();
                }
            }
            catch (Exception ex)
            {
                _log.Error($"CheckEventState method Exception: ", ex);
                status = IncidentStatus.Deactivated;
            }

            return status;
        }
    }
}
