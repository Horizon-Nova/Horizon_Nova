using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Quartz.Impl.Triggers;
using YiSha.Entity.SystemManage;
using YiSha.Enum;
using YiSha.Service.SystemManage;
using YiSha.Util;
using YiSha.Util.Extension;
using YiSha.Util.Model;

namespace YiSha.Business.AutoJob
{
    public class JobExecute : IJob
    {
        private AutoJobService autoJobService = new AutoJobService();
        private AutoJobLogService autoJobLogService = new AutoJobLogService();

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(async () =>
            {
                TData obj = new TData();
                long jobId = 0;
                JobDataMap jobData = null;
                AutoJobEntity dbJobEntity = null;
                try
                {
                    jobData = context.JobDetail.JobDataMap;
                    jobId = jobData["Id"].ParseToLong();
                    // 獲取資料庫中的任務
                    dbJobEntity = await autoJobService.GetEntity(jobId);
                    if (dbJobEntity != null)
                    {
                        if (dbJobEntity.JobStatus == StatusEnum.Yes.ParseToInt())
                        {
                            CronTriggerImpl trigger = context.Trigger as CronTriggerImpl;
                            if (trigger != null)
                            {
                                if (trigger.CronExpressionString != dbJobEntity.CronExpression)
                                {
                                    // 更新任務周期
                                    trigger.CronExpressionString = dbJobEntity.CronExpression;
                                    await JobScheduler.GetScheduler().RescheduleJob(trigger.Key, trigger);
                                }

                                #region 執行任務
                                switch (context.JobDetail.Key.Name)
                                {
                                    case "資料庫備份":
                                        obj = await new DatabasesBackupJob().Start();
                                        break;
                                }
                                #endregion
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    obj.Message = ex.GetOriginalException().Message;
                    LogHelper.Error(ex);
                }

                try
                {
                    if (dbJobEntity != null)
                    {
                        if (dbJobEntity.JobStatus == StatusEnum.Yes.ParseToInt())
                        {
                            #region 更新下次運行時間
                            await autoJobService.SaveForm(new AutoJobEntity
                            {
                                Id = dbJobEntity.Id,
                                NextStartTime = context.NextFireTimeUtc.Value.DateTime.AddHours(8)
                            });
                            #endregion

                            #region 記錄執行狀態
                            await autoJobLogService.SaveForm(new AutoJobLogEntity
                            {
                                JobGroupName = context.JobDetail.Key.Group,
                                JobName = context.JobDetail.Key.Name,
                                LogStatus = obj.Tag,
                                Remark = obj.Message
                            });
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    obj.Message = ex.GetOriginalException().Message;
                    LogHelper.Error(ex);
                }
            });
        }
    }
}
