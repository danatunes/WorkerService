using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EmailService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerService2
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IEmailSender _emailSender;
        private FileSystemWatcher watcher;
        private readonly string directory = @"C:\Users\maga_\OneDrive\Рабочий стол\MyFolder";

        public Worker(ILogger<Worker> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = directory;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnRenamed;
            return base.StartAsync(cancellationToken);
        }



        private void OnRenamed(object source, RenamedEventArgs e)
        {
            SendRenamedInfo(e);
        }

        private void SendRenamedInfo(RenamedEventArgs e)
        {
            _logger.LogInformation("A file renamed at : {time}", DateTimeOffset.Now);
            var message = new Message(new string[] { "codebuster01@gmail.com" }, "FILE RENAMED ", $"File: {e.OldFullPath} RENAMED TO {e.FullPath}", e.FullPath);
            _emailSender.SendEmail(message);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
           SendEmail(e);
        }

        private void SendEmail(FileSystemEventArgs e)
        {
            _logger.LogInformation($"A file {e.ChangeType} " + "at {time}", DateTimeOffset.Now);
            var message = new Message(new string[] { "codebuster01@gmail.com" }, $"FILE {e.ChangeType}", $"File:  {e.FullPath} {e.ChangeType}", e.FullPath);
            _emailSender.SendEmail(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                watcher.EnableRaisingEvents = true; //starts listening
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }
        }



    }
}
