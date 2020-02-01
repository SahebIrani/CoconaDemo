using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Cocona;
using Cocona.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Simple
{
	public class Program : CoconaConsoleAppBase
	{
		public Program(ILogger<Program> logger) 
			=> logger.LogInformation("Create Instance");


		public static async Task Main(string[] args)
		{
			await CoconaApp.Create()
				.ConfigureServices(services =>
				{
					services.AddTransient<MyService>();
				})
				.RunAsync<Program>(args , _ => _.TreatPublicMethodsAsCommands = true)
			;

			//? await CreateHostBuilder(args).Build().RunAsync();
		}

		public async Task RunAsync()
		{
			while (!Context.CancellationToken.IsCancellationRequested)
			{
				Console.WriteLine($"Request is {DateTime.Now.Millisecond} Millisecond .. !!!!");

				await Task.Delay(620);
			}

			Console.WriteLine("Request is Cancel .. !!!!");
		}


		[SampleCommandFilter]
		[Command(Description = "Say hello")]
		public void Hello(
			[FromService]MyService myService,
			bool key,
			[Argument]string name = "default user")
		{
			Console.WriteLine($"{(key ? "Hey" : "Hello")} {name}");

			myService.Hello($"Hello {name} .. !!!!");
		}

		[Command(Description = "Say goodbye")]
		public void Bye([Argument]string name) =>
			Console.WriteLine($"Goodbye {name}!");

		public void Run(
			[Range(1, 128)]int width,
			[Range(1, 128)]int height,
			[Argument][PathExists]string filePath)
		{
			Console.WriteLine($"Size: {width}x{height}");
			Console.WriteLine($"Path: {filePath}");
		}

		class PathExistsAttribute : ValidationAttribute
		{
			protected override ValidationResult IsValid(
				object value,
				ValidationContext validationContext)
			{
				if (value is string path &&
					(Directory.Exists(path) ||
					Directory.Exists(path)))
				{
					return ValidationResult.Success;
				}

				return new ValidationResult($"The path '{value}' is not found.");
			}
		}

		class SampleCommandFilterAttribute : CommandFilterAttribute
		{
			public override async ValueTask<int> OnCommandExecutionAsync(
				CoconaCommandExecutingContext ctx,
				CommandExecutionDelegate next)
			{
				Console.WriteLine($"Before Command: {ctx.Command.Name}");
				try
				{
					return await next(ctx);
				}
				finally
				{
					Console.WriteLine($"End Command: {ctx.Command.Name}");
				}
			}
		}

		public class MyService
		{
			public MyService(ILogger<MyService> logger) => Logger = logger;
			private readonly ILogger Logger;

			public void Hello(string message) => Logger.LogInformation(message);
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
