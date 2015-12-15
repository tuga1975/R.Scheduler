﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Common.Logging;
using Npgsql;
using NpgsqlTypes;
using Quartz;
using R.Scheduler.Interfaces;

namespace R.Scheduler.Persistance
{
    /// <summary>
    /// PostgreSQL implementation of <see cref="IPersistanceStore"/>
    /// </summary>
    public class PostgreStore : IPersistanceStore
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _connectionString;

        public PostgreStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Insert AuditLog. 
        /// Each entry is read-only.
        /// </summary>
        /// <param name="log"></param>
        public void InsertAuditLog(AuditLog log)
        {
            const string sqlInsert = @"INSERT INTO RSCHED_AUDIT_HISTORY(time_stamp
                                                               ,action
                                                               ,fire_instance_id
                                                               ,job_name
                                                               ,job_group
                                                               ,job_type
                                                               ,trigger_name
                                                               ,trigger_group
                                                               ,fire_time_utc
                                                               ,scheduled_fire_time_utc
                                                               ,job_run_time
                                                               ,params
                                                               ,refire_count
                                                               ,recovering
                                                               ,result
                                                               ,execution_exception) 
                                                            VALUES (
                                                                :timeStamp, 
                                                                :action, 
                                                                :fireInstanceId, 
                                                                :jobName, 
                                                                :jobGroup, 
                                                                :jobType, 
                                                                :triggerName, 
                                                                :triggerGroup, 
                                                                :fireTimeUtc, 
                                                                :scheduledFireTimeUtc, 
                                                                :jobRunTime, 
                                                                :params, 
                                                                :refireCount, 
                                                                :recovering, 
                                                                :result, 
                                                                :executionException);";


            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sqlInsert, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("timeStamp", NpgsqlDbType.Timestamp));
                        command.Parameters.Add(new NpgsqlParameter("action", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("fireInstanceId", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobGroup", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("jobType", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerName", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("triggerGroup", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("fireTimeUtc", NpgsqlDbType.TimestampTZ));
                        command.Parameters.Add(new NpgsqlParameter("scheduledFireTimeUtc", NpgsqlDbType.TimestampTZ));
                        command.Parameters.Add(new NpgsqlParameter("jobRunTime", NpgsqlDbType.Bigint));
                        command.Parameters.Add(new NpgsqlParameter("params", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("refireCount", NpgsqlDbType.Integer));
                        command.Parameters.Add(new NpgsqlParameter("recovering", NpgsqlDbType.Boolean));
                        command.Parameters.Add(new NpgsqlParameter("result", NpgsqlDbType.Varchar));
                        command.Parameters.Add(new NpgsqlParameter("executionException", NpgsqlDbType.Varchar));

                        command.Parameters[0].Value = DateTime.UtcNow;
                        command.Parameters[1].Value = log.Action;
                        command.Parameters[2].Value = log.FireInstanceId;
                        command.Parameters[3].Value = log.JobName;
                        command.Parameters[4].Value = log.JobGroup;
                        command.Parameters[5].Value = log.JobType;
                        command.Parameters[6].Value = log.TriggerName;
                        command.Parameters[7].Value = log.TriggerGroup;
                        command.Parameters[8].Value = log.FireTimeUtc;
                        command.Parameters[9].Value = log.ScheduledFireTimeUtc;
                        command.Parameters[10].Value = log.JobRunTime.Ticks;
                        command.Parameters[11].Value = log.Params;
                        command.Parameters[12].Value = log.RefireCount;
                        command.Parameters[13].Value = log.Recovering;
                        command.Parameters[14].Value = log.Result ?? string.Empty;
                        command.Parameters[15].Value = log.ExecutionException ?? string.Empty;

                        command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error persisting AuditLog. {0}", ex.Message);
                }
            }
        }

        public int GetJobDetailsCount()
        {
            long retval = 0;
            const string sql = @"SELECT count(*) FROM qrtz_job_details;";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        retval = (long) command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting job details count. {0}", ex.Message);
                }
            }

            return (int) retval;
        }

        public int GetTriggerCount()
        {
            long retval = 0;
            const string sql = @"SELECT count(*) FROM qrtz_triggers;";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        retval = (long)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting triggers count. {0}", ex.Message);
                }
            }

            return (int)retval;
        }

        public IList<TriggerKey> GetFiredTriggers()
        {
            IList<TriggerKey> keys = new List<TriggerKey>();

            const string sql = @"SELECT trigger_name, trigger_group FROM qrtz_fired_triggers";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                keys.Add(new TriggerKey(rs.GetString(0), rs.GetString(1)));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting fired triggers. {0}", ex.Message);
                }
            }

            return keys;
        }

        public IEnumerable<AuditLog> GetErroredJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT * FROM rsched_audit_history WHERE execution_exception <> '' AND action = 'JobWasExecuted' order by time_stamp desc limit {0}", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        public IEnumerable<AuditLog> GetExecutedJobs(int count)
        {
            if (count > 1000)
            {
                Logger.Warn("Max number of AuditLogs to fetch is 1000");
                count = 1000;
            }

            string sql = string.Format(@"SELECT * FROM rsched_audit_history WHERE action = 'JobWasExecuted' order by time_stamp desc limit {0}", count);

            IEnumerable<AuditLog> retval = GetAuditLogs(sql);

            return retval;
        }

        public Guid UpsertJobKeyIdMap(string jobName, string jobGroup)
        {
            throw new NotImplementedException();
        }

        public JobKey GetJobKey(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid GetJobId(JobKey jobKey)
        {
            throw new NotImplementedException();
        }

        public TriggerKey GetTriggerKey(Guid id)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertTriggerKeyIdMap(string triggerName, string triggerGroup)
        {
            throw new NotImplementedException();
        }

        public Guid UpsertCalendarIdMap(string name)
        {
            throw new NotImplementedException();
        }

        public string GetCalendarName(Guid id)
        {
            string name = null;

            const string sql = @"SELECT calendar_name FROM rsched_calendar_id_name_map] WHERE id = :id";

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        command.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid));
                        command.Parameters[1].Value = id;
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            if (rs.Read())
                            {
                                name = rs.GetString(0);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting calendar name. {0}", ex.Message);
                }
            }

            return name;
        }

        private IEnumerable<AuditLog> GetAuditLogs(string sql)
        {
            IList<AuditLog> retval = new List<AuditLog>();

            using (var con = new NpgsqlConnection(_connectionString))
            {
                try
                {
                    con.Open();
                    using (var command = new NpgsqlCommand(sql, con))
                    {
                        using (NpgsqlDataReader rs = command.ExecuteReader())
                        {
                            while (rs.Read())
                            {
                                retval.Add(new AuditLog
                                {
                                    TimeStamp = (DateTime) rs["time_stamp"],
                                    Action = rs["action"].ToString(),
                                    ExecutionException = rs["execution_exception"].ToString(),
                                    FireInstanceId = rs["fire_instance_id"].ToString(),
                                    FireTimeUtc = (DateTimeOffset?) rs["fire_time_utc"],
                                    JobGroup = rs["job_group"].ToString(),
                                    JobName = rs["job_name"].ToString(),
                                    JobType = rs["job_type"].ToString(),
                                    TriggerName = rs["trigger_name"].ToString(),
                                    TriggerGroup = rs["trigger_group"].ToString(),
                                    JobRunTime = new TimeSpan((long) rs["job_run_time"]),
                                    ScheduledFireTimeUtc = (DateTimeOffset?) rs["scheduled_fire_time_utc"],
                                    Params = rs["params"].ToString(),
                                    RefireCount = (int) rs["refire_count"],
                                    Recovering = (bool) rs["recovering"],
                                    Result = rs["result"].ToString()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("Error getting AuditLogs. {0}", ex.Message);
                }
            }
            return retval;
        }
    }
}
