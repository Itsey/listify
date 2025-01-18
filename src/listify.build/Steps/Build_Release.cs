using System;
using System.IO;
using Flurl.Http;
using Nuke.Common;
using Serilog;
using WinSCP;

public partial class Build : NukeBuild {

    // Well known step for releasing into the selected environment.  Arrange Construct Examine Package [Release] Test
    public Target ReleaseStep => _ => _
      .DependsOn(Initialise, PackageStep)
      .Before(TestStep, Wrapup)
      .After(PackageStep)
      .Triggers(PostReleaseWarmup)
      .Executes(() => {
          if ((!IsLocalBuild) && (Configuration != Configuration.Release)) {
              Log.Error("ReleaseStep is only valid in Release mode");
              throw new InvalidOperationException("DeployStep is only valid in Release mode for remote builds.");
          }

          var ftpDeployment = settings.Config.BuildSection.FtpDeployment;

          if (!ftpDeployment.ValidSettings()) {
              b.Verbose.Log($"FTP>> {ftpDeployment.Server},{ftpDeployment.Username},{ftpDeployment.ServerPath}");
              Log.Error("FTP Configuration Is Missing.  Unable to proceed with release.");
              throw new InvalidOperationException("No FTP Server specified in configuration.");
          }

          // For debugging its simpler to be able to turn off some of the very long running activities, therfore these bools turn off chunks of the process for debug only.
          bool deployWebsite = true;
          bool deployAllNewFilesToSite = ftpDeployment.ActuallyDeployFiles;
          bool useAppOffline = ftpDeployment.OfflineSiteDuringDeployment;
          bool skipWebContent = ftpDeployment.SkipWebContentFolder;

          if (OverrideForceWebContentDeployment != null) {
              Log.Information($"Overriding SkipWebContent with {OverrideForceWebContentDeployment.Value}");
              skipWebContent = !OverrideForceWebContentDeployment.Value;
          }
          // Set up session options
          var sessionOptions = new SessionOptions {
              Protocol = Protocol.Ftp,
              HostName = ftpDeployment.Server,
              UserName = ftpDeployment.Username,
              Password = ftpDeployment.Password,
              Timeout = TimeSpan.FromMinutes(ftpDeployment.TimeoutInMins),
          };

          using (var session = new Session()) {
              session.SessionLogPath = Path.Combine(settings.ArtifactsDirectory, "winscp.log");

              int transferCount = 0;
              int transferCountWriteFrequency = 10;
              bool logProgress = false;

              session.FileTransferProgress += (sender, e) => {
                  if (logProgress) {
                      Log.Information($"Prg {e.OverallProgress}");
                  }
              };

              session.FileTransferred += (sender, e) => {
                  if (e.Error == null) {
                      transferCount++;
                      if (transferCount % transferCountWriteFrequency == 0) {
                          Log.Information($"Uploaded {transferCount} files to. Most Recent {e.FileName}");
                      }
                  } else {
                      Log.Error("Error uploading {0} to {1} - {2}", e.FileName, e.Destination, e.Error);
                  }
              };

              // Connect
              session.Open(sessionOptions);

              var transferOptions = new TransferOptions();
              transferOptions.TransferMode = TransferMode.Binary;
              transferOptions.OverwriteMode = OverwriteMode.Overwrite;
              if (skipWebContent) {
                  transferOptions.FileMask = "|*/";
                  Log.Warning("BuildContent Folder Skipped - Deployment Faster but no new JS or web content uploaded");
              }

              TransferOperationResult transferResult;

              var wd = settings.ArtifactsDirectory / "deploy_temp";
              Log.Information($"Source ]{wd}[");

              Directory.CreateDirectory(wd);
              string aolFile = Path.Combine(settings.DependenciesDirectory, "Publishing", "app_offline.htm");
              string webConfigFile = Path.Combine(settings.DependenciesDirectory, "Publishing", "web.config");

              if (deployWebsite) {
                  // Hosting configuration files do not align with dev files, therefore save them before redeployment.
                  session.GetFiles(ftpDeployment.ServerPath + "*.donotcommit", wd + "\\*.donotcommit", false, transferOptions);
                  session.GetFiles(ftpDeployment.ServerPath + "web.config", wd + "\\web.config", false, transferOptions);

                  var ad = settings.ArtifactsDirectory / "publish";

                  if (useAppOffline) {
                      // Mark live site as down.
                      Log.Warning("Taking Live Site OFFLINE.");

                      if (!session.Opened) {
                          throw new InvalidOperationException("Session not open");
                      }

                      transferResult = session.PutFiles(aolFile, ftpDeployment.ServerPath, false, transferOptions);
                      transferResult.Check();
                  }

                  if (deployAllNewFilesToSite) {
                      // Upload all the files from publish directory
                      Log.Information("Uploading replacement site copy.");

                      transferResult = session.PutFiles(ad + "\\*.*", $"{ftpDeployment.ServerPath}/*.*", false, transferOptions);
                      transferResult.Check();
                  }

                  transferResult = session.PutFiles(webConfigFile, ftpDeployment.ServerPath + "web.config", false, transferOptions);
                  transferResult.Check();

                  if (useAppOffline) {
                      var rea = session.RemoveFile($"{ftpDeployment.ServerPath}/app_offline.htm");
                      if (rea.Error != null) {
                          Log.Error("Error removing app_offline.htm - {0}", rea.Error);
                      } else {
                          Log.Information("Live Site back ONLINE.");
                      }
                  }
              }

              session.Close();
          }
      });

    public Target PostReleaseWarmup => _ => _
       .DependsOn(Initialise)
       .After(ReleaseStep)
       .Executes(() => {
           string url = settings.Config.AppSection.PrimaryUrl;
           var f = url.GetAsync();
           f.Wait();
           if (f.Result.StatusCode == (int)System.Net.HttpStatusCode.OK) {
               Log.Information("Warmup of site successful.");
           } else {
               Log.Error("Warmup of site failed >> SITE POTENTIALLY DOWN.");
           }
       });
}