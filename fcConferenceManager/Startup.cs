using MAGI_API.Security;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System;
using System.Configuration;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Microsoft.Owin.Extensions;

[assembly: OwinStartupAttribute(typeof(MAGI_API.Startup))]
[assembly: OwinStartup(typeof(MAGI_API.Startup))]
namespace MAGI_API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
			//app.MapSignalR();				   
            app.Use((context, next) =>
            {
                var httpContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
                httpContext.SetSessionStateBehavior(SessionStateBehavior.Required);
                return next();
            }).UseStageMarker(PipelineStage.MapHandler);

            ConfigureOAuthTokenGeneration(app);
            ConfigureOAuthTokenConsumption(app);
            HttpConfiguration httpConfig = new HttpConfiguration();
            ConfigureWebApi(httpConfig);

            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll); 
            app.Use((context, next) =>
            {
                // now use the session 
                app.UseWebApi(httpConfig);
                return next();
            }).UseStageMarker(PipelineStage.PostAcquireState);

            RedisScaleoutConfiguration redis = new RedisScaleoutConfiguration("magi.redis.cache.windows.net:6380,password=f5oU4LyEhbv5Mj5hc223vEOAV8veba+bTV+Quwqnz00=,ssl=True,abortConnect=False", "MAGI-QA");
            GlobalHost.DependencyResolver.UseStackExchangeRedis(redis);
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
            GlobalHost.TraceManager.Switch.Level = SourceLevels.Information;
        }

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            // Plugin the OAuth bearer JSON Web Token tokens generation and Consumption will be here
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                //For Dev enviroment only (on production should be AllowInsecureHttp = false)
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/oauth/token"),                
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(7),
                Provider = new CustomOAuthProvider(),
                //AccessTokenFormat = new CustomJwtFormat("http://192.168.1.98/")
                AccessTokenFormat = new CustomJwtFormat(ConfigurationManager.AppSettings["AppURL"].ToString().Replace("/forms", "/"))
            };         
            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["AppURL"].ToString().Replace("/forms", "/");
            // var issuer = "http://192.168.1.98/";
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                    {
                        new SymmetricKeyIssuerSecurityKeyProvider(issuer, audienceSecret)
                    },
                    Provider = new QueryStringOAuthBearerProvider("token")
                });
        }

        private void ConfigureWebApi(HttpConfiguration config)
        {

            //var httpControllerRouteHandler = typeof(HttpControllerRouteHandler).GetField("_instance", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

            //if (httpControllerRouteHandler != null)
            //{
            //    httpControllerRouteHandler.SetValue(null,
            //        new Lazy<HttpControllerRouteHandler>(() => new SessionHttpControllerRouteHandler(), true));
            //}

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(name: "ControllerOnly", routeTemplate: "api/{controller}");
            config.Routes.MapHttpRoute(name: "ControllerAndAction", routeTemplate: "api/{controller}/{action}");
            config.Routes.MapHttpRoute(name: "ControllerAndId", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{action}/{id}", defaults: new { id = RouteParameter.Optional });

            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            config.Formatters.JsonFormatter.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None;
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff" });

            //var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            //jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }

    public class SessionControllerHandler : HttpControllerHandler, IRequiresSessionState
    {
        public SessionControllerHandler(RouteData routeData)
            : base(routeData)
        { }
    }

    public class SessionHttpControllerRouteHandler : HttpControllerRouteHandler
    {
        protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            return new SessionControllerHandler(requestContext.RouteData);
        }
    }
}