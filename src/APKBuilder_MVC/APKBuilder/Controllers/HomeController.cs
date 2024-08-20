﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APKBuilder.Hubs;
using APKBuilder.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using APKBuilder.Models;
using APKBuilder.Services;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace APKBuilder.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAndroidBuildManager _androidBuildManager;
        private readonly IGitCommandManager _gitCommandManager;
        private readonly IQueueManager _queueManager;
        private readonly IFileManager _fileManager;
        private readonly INotificationManager _notificationManager;
        private readonly IHubContext<ClientHub> _hubContext;
        private readonly AppConfig _appConfig;

        public HomeController(ILogger<HomeController> logger, IQueueManager queueManager, IHubContext<ClientHub> hubContext, IOptions<AppConfig>Options)
        {
            _logger = logger;
            _appConfig = Options.Value;
            _androidBuildManager = new AndroidBuildManager(_appConfig.CMDEXE, _appConfig.OutputPath);
            _gitCommandManager =
                new GitCommandManager(_appConfig.GitExePath, _appConfig.WorkingDirectory);
            _queueManager = queueManager;
            _hubContext = hubContext;
             _fileManager = new FileManager();
            _notificationManager = new NotificationManager(_hubContext);
            //_logger.LogInformation("logTested");


        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(BuildRequest request)
        {
            _queueManager.Produce(request.env, request.entity, request.branch, request.guid);
            ViewBag.GUID = request.guid;
            ViewBag.Env = request.env;
            ViewBag.Entity = request.entity;
            ViewBag.Branch = request.branch;
            return View(request);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
             return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

      

        [HttpPost]
        public async Task NotifyUser(string hash, string response, string path)
        {
            await _notificationManager.SendMessage(hash, response, path: path);
        }

        public async Task<IActionResult> Download(string filePath, string guid)
        {
            if (filePath == null)
                filePath = ViewBag.Path;
            var path = filePath;
            if (!System.IO.File.Exists(filePath))
            {
                _notificationManager.SendMessage(guid, AppConstants.AppConstants.FAILED, path: AppConstants.AppConstants.FILE_NOT_FOUND).GetAwaiter().GetResult();
                return NoContent();
            }
            var memory = new MemoryStream();
            await using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, _fileManager.GetContentType(path), Path.GetFileName(path));
        }
    }
}
