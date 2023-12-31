// See https://aka.ms/new-console-template for more information

using bitport_google_drive_syncer;
using Quartz;
using Quartz.Impl;

var google = ClientBuilder.Build();
Drive.GetService(google);

var folder = Drive.Service.Files.List();
folder.Q = "name='BitPort' and mimeType='application/vnd.google-apps.folder'";
var found = await folder.ExecuteAsync();

if (found?.Files?.Count > 0)
{
    var id = found.Files.FirstOrDefault()?.Id;
    if (id != null) FolderCache.id = id;
}
else
{
    FolderCache.id = Drive.CreateFolder("BitPort");
}

FolderCache.folders = await ClientBuilder.getFolders();
ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
IScheduler scheduler = await schedulerFactory.GetScheduler();

// Define the job
IJobDetail job = JobBuilder.Create<SyncJob>()
    .WithIdentity("simpleJob", "group1")
    .StoreDurably()
    .Build();

// Trigger the job to run every 10 seconds
ITrigger trigger = TriggerBuilder.Create()
    .WithIdentity("simpleTrigger", "group1")
    .StartNow()
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(10)
        .RepeatForever()
        .WithMisfireHandlingInstructionNowWithExistingCount()) 
    .Build();

// Schedule the job with the trigger
await scheduler.ScheduleJob(job, trigger);

// Start the scheduler
await scheduler.Start();

// Keep the console application running
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

// Shut down the scheduler
await scheduler.Shutdown();