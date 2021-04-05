using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DemoOrleans.SiloHost
{
    public class SiloHostService: IHostedService
    {
        private IHostApplicationLifetime _appLifetime;
        private ILogger<SiloHostService> _logger;
        private IConfiguration _config;
        private ISiloHost _host;

        public SiloHostService(IHostApplicationLifetime appLifetime,
            ILogger<SiloHostService> logger,
            IConfiguration config)
        {
            _appLifetime = appLifetime;
            _logger = logger;
            _config = config;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            try
            {
                int portAdd = _config.GetValue<int>("portadd");
                StartSilo(portAdd).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while instantiating silo.");
            }
        }

        private void OnStopping()
        {
            _logger.LogInformation("Stopping host...");
            if (_host != null)
            {
                _host.StopAsync().Wait();
            }
        }

        private void OnStopped()
        {
            _logger.LogInformation("Host stopped.");
        }

        private async Task StartSilo(int portAdd)
        {
            var builder = new SiloHostBuilder()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "DemoOrleansHostA";
                })

                //configura o garbage collection
                .Configure<GrainCollectionOptions>(options =>
                {
                    //define o periodo de inatividade necessária para o grão estar disponivel para coleta e desativação
                    options.CollectionAge = TimeSpan.FromMinutes(3);
                    
                    // Substitua o valor da CollectionAge para o ProductGrain para 5 minutos
                    //options.ClassSpecificCollectionAge[typeof(ProductGrain).FullName] = TimeSpan.FromMinutes(5);
                    
                    //define o tempo de coleta periodica dos grãos inativos
                    options.CollectionQuantum = TimeSpan.FromMinutes(2);
                })
                
                 //configuração do cluster 
                 .UseAdoNetClustering(options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = _config.GetConnectionString("ClusterStorage");
                })
                
                 //configuração do banco de dados dos grãos
                .AddAdoNetGrainStorage("Products", options =>
                {
                    options.Invariant = "System.Data.SqlClient";
                    options.ConnectionString = _config.GetConnectionString("GrainStorage");
                    options.UseJsonFormat = true;
                })

                //configuração dos endpoints
                .ConfigureEndpoints(siloPort: 11111 + portAdd, gatewayPort: 30000 + portAdd)
                
                //fala para o framework onde estão os grãos
                .ConfigureApplicationParts(parts => parts.AddFromApplicationBaseDirectory())
                //.ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ProductGrain).Assembly).WithReferences())
                
                //configura o dashboard
                .UseDashboard(options => options.Port = 8081)
                
                //configura o log
                .ConfigureLogging(logging => logging.AddConsole());

            _host = builder.Build();
            await _host.StartAsync();
            _logger.LogInformation("Host started.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Host stopping...");
            if (_host != null)
            {
                _host.StopAsync(cancellationToken);
            }
            return Task.CompletedTask;
        }
    }
}
