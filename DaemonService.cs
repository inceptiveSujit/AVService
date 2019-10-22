using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using nClam;

namespace daemon
{

    public class DaemonService : IHostedService, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IOptions<DaemonConfig> _config;

        public string Schedule => "0 5 * * *";

        public DaemonService(ILogger<DaemonService> logger, IOptions<DaemonConfig> config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                await ExecuteOnceAsync(cancellationToken);
            }
            //return Task.CompletedTask;
        }

        private async Task ExecuteOnceAsync(CancellationToken cancellationToken)
        {

            var taskFactory = new TaskFactory(TaskScheduler.Current);
            await taskFactory.StartNew(
                async () =>
                {
                    try
                    {
                        _logger.LogInformation("Starting daemon: " );

                        try
                        {


                           var clam = new ClamClient("10.0.0.40",3310);
                            var pingResult = await clam.PingAsync();

                            if (!pingResult)
                            {
                                Console.WriteLine("test failed. Exiting.");
                                _logger.LogInformation("test failed. Exiting.");
                               
                            }

                            Console.WriteLine("connected to clam.");
                            _logger.LogInformation("connected to clam.");
                            var scanResult = await clam.ScanFileOnServerAsync("/home/tes-mvc/Final_Team.txt");  //any file you would like!

                            switch (scanResult.Result)
                            {
                                case ClamScanResults.Clean:
                                    Console.WriteLine("The file is clean!");
                                    _logger.LogInformation("The file is clean!");
                                    break;
                                case ClamScanResults.VirusDetected:
                                    Console.WriteLine("Virus Found!");
                                    _logger.LogInformation("Virus Found!");
                                    break;
                                case ClamScanResults.Error:
                                    Console.WriteLine("Woah an error occured! Error: {0}", scanResult.RawResult);
                                    _logger.LogInformation("Woah an error occured! Error: {0}", scanResult.RawResult);
                                     break;
                            }

                            
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("not connected.", e);
                            
                        }


                    }
                    catch (Exception ex)
                    {

                    }
                },
                cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping daemon.");
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing....");

        }
    }
}
