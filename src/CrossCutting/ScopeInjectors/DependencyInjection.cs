using AutoMapper;
using Business.Configurations;
using Business.Mappings;
using Business.Services;
using Data.Context;
using Data.UnitOfWork;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrossCutting.ScopeInjectors
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SalaoCabeleleilaDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<ModalTitleConfig>(configuration.GetSection("ModalTitleConfig"));
            services.Configure<SalaoHorarioConfig>(configuration.GetSection("SalaoHorarioConfig"));

            services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ModalTitleConfig>>().Value);
            services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<SalaoHorarioConfig>>().Value);

            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UsuarioProfile>();
                cfg.AddProfile<ServicoProfile>();
            });

            services.AddSingleton<IMapper>(mapperConfig.CreateMapper());

            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IAutenticacaoService, AutenticacaoService>();
            services.AddScoped<IAgendamentoService, AgendamentoService>();
            services.AddScoped<IServicoService, ServicoService>();
            services.AddScoped<IDashboardService, DashboardService>();

            return services;
        }
    }
}
