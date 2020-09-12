namespace DigitalIcebreakers

open DigitalIcebreakers.Hubs
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System.Collections.Generic
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer
open System
open Model

type AppSettings = {
    AnyEnvironmentVariable: string
}

type Startup(configuration: IConfiguration) = 
    
       // member this.Configuration with get()

        // This method gets called by the runtime. Use this method to add services to the container.
        member this.ConfigureServices(services: IServiceCollection) =
            services.AddApplicationInsightsTelemetry() |> ignore
            
            services.AddMvc() |> ignore

            services.AddSignalR(fun options -> 
                options.ClientTimeoutInterval <- System.Nullable<System.TimeSpan>(TimeSpan(0, 0, 4))
            ) |> ignore

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(fun c ->
                c.RootPath <- (configuration.["SpaPath"] + "/build")
            ) |> ignore

            services.Configure<AppSettings>(configuration) |> ignore
            services.AddSingleton<List<Lobby>>() |> ignore
            services.AddScoped<IClientHelper, ClientHelper>() |> ignore
            services.AddScoped<LobbyManager>() |> ignore

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
            match env.IsDevelopment() with
            | true -> app.UseDeveloperExceptionPage() |> ignore
            | false -> 
                app.UseExceptionHandler("/Error")
                    .UseHsts() |> ignore
            
            app.UseStaticFiles() |> ignore
            app.UseSpaStaticFiles() |> ignore
            app.UseRouting() |> ignore

            app.UseEndpoints( fun endpoints -> 
                endpoints.MapHub<GameHub>("/gameHub") |> ignore
                endpoints.MapControllerRoute(
                    name = "default",
                    pattern = "{controller}/{action=Index}/{id?}") |> ignore
            ) |> ignore

            app.UseSpa(fun spa ->
                spa.Options.SourcePath <- configuration.["SpaPath"]
                match env.IsDevelopment() with
                | true -> spa.UseReactDevelopmentServer(npmScript = "start")
                | _ -> do()
            )
