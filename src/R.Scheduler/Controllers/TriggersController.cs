﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Http;
using Common.Logging;
using Quartz;
using R.Scheduler.Contracts.Model;
using R.Scheduler.Interfaces;
using StructureMap;

namespace R.Scheduler.Controllers
{
    public class TriggersController : ApiController
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        readonly ISchedulerCore _schedulerCore;

        public TriggersController()
        {
            _schedulerCore = ObjectFactory.GetInstance<ISchedulerCore>();
        }

        /// <summary>
        /// Get all triggers of a specified job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/jobs/{jobId}/triggers")]
        public IList<TriggerDetails> Get(Guid jobId)
        {
            Logger.DebugFormat("Entered TriggersController.Get(). jobId = {0}", jobId);

            IDictionary<ITrigger, Guid> quartzTriggers = _schedulerCore.GetTriggersOfJob(jobId);

            return TriggerHelper.GetTriggerDetails(quartzTriggers);
        }

        /// <summary>
        /// Get all triggers of all jobs
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [Route("api/fireTimes")]
        public IList<TriggerFireTime> Get(DateTime start, DateTime end)
        {
            Logger.Debug("Entered TriggersController.Get()");

            IEnumerable<TriggerFireTime> fireTimes = _schedulerCore.GetFireTimesBetween(start, end);

            return fireTimes as IList<TriggerFireTime>;
        }

        /// <summary>
        /// Schedule SimpleTrigger for a specified job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/simpleTriggers")]
        public QueryResponse Post([FromBody]SimpleTrigger model)
        {
            Logger.InfoFormat("Entered TriggersController.Post(). Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.ScheduleTrigger(model);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = "Server",
                        Message = string.Format("Error scheduling trigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Schedule CronTrigger for a specified job
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [Route("api/cronTriggers")]
        public QueryResponse Post([FromBody] CronTrigger model)
        {
            Logger.DebugFormat("Entered TriggersController.Post(). Name = {0}", model.Name);

            var response = new QueryResponse { Valid = true };

            try
            {
                var id = _schedulerCore.ScheduleTrigger(model);
                response.Id = id;
            }
            catch (Exception ex)
            {
                string type = "Server";

                if (ex is FormatException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorSchedulingTrigger",
                        Type = type,
                        Message = string.Format("Error scheduling CronTrigger {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Remove all triggers of a specified job
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/jobs/{jobId}/triggers")]
        public QueryResponse Unschedule(Guid jobId)
        {
            Logger.DebugFormat("Entered TriggersController.Unschedule(). jobId = {0}", jobId);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveJobTriggers(jobId);
            }
            catch (Exception ex)
            {
                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorUnschedulingJob",
                        Type = "Server",
                        Message = string.Format("Error: {0}", ex.Message)
                    }
                };
            }

            return response;
        }

        /// <summary>
        /// Remove specified trigger
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [AcceptVerbs("DELETE")]
        [Route("api/triggers/{id}")]
        public QueryResponse DeleteTrigger(Guid id)
        {
            Logger.DebugFormat("Entered TriggersController.DeleteTrigger(). id = {0}", id);

            var response = new QueryResponse { Valid = true };

            try
            {
                _schedulerCore.RemoveTrigger(id);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Error removing trigger {0}. {1}", id, ex.Message);

                string type = "Server";
                if (ex is ArgumentException)
                {
                    type = "Sender";
                }

                response.Valid = false;
                response.Errors = new List<Error>
                {
                    new Error
                    {
                        Code = "ErrorRemovingTrigger",
                        Type = type,
                        Message = string.Format("Error removing trigger {0}.", id)
                    }
                };
            }

            return response;
        }
    }
}
