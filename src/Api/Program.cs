using Api.Configurations;

using CrossCutting.ScopeInjectors;

using Data.Context;

using Domain.Entities;

using Domain.Enums;

using Microsoft.EntityFrameworkCore;

using Serilog;



var builder = WebApplication.CreateBuilder(args);



builder.Host.UseSerilog((context, configuration) =>

    configuration.ReadFrom.Configuration(context.Configuration));



builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddSwaggerDocumentation();

builder.Services.AddCorsPolicy();



var app = builder.Build();



if (app.Environment.IsDevelopment())

{

    app.UseSwagger();

    app.UseSwaggerUI(options =>

    {

        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Cabeleleila Leila API v1");

    });

}



app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors("ClientApp");

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html");



if (app.Environment.IsDevelopment())

{

    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<SalaoCabeleleilaDbContext>();

    db.Database.Migrate();

    await SeedAdminAsync(db);

}



app.Run();



static async Task SeedAdminAsync(SalaoCabeleleilaDbContext db)

{

    if (await db.Usuarios.AnyAsync(u => u.Email == "admin@cabeleleila.com"))

        return;



    db.Usuarios.Add(new Usuario

    {

        Nome = "Leila Admin",

        Email = "admin@cabeleleila.com",

        Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),

        Telefone = "11999990000",

        PerfilId = (int)PerfilEnum.Usuario_admin,

        Ativo = true,

        CreatedAt = DateTime.Now

    });



    await db.SaveChangesAsync();

}

