// See https://aka.ms/new-console-template for more information
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder();
var pythonHome = Path.Join(Environment.CurrentDirectory, "./PythonFiles");

builder.Services.WithPython().WithHome(pythonHome).FromRedistributable().WithPipInstaller().WithVirtualEnvironment(Path.Join(pythonHome, ".venv"));

var app = builder.Build();

var pythonEnvironment = app.Services.GetRequiredService<IPythonEnvironment>();

var pythonModule = pythonEnvironment.Example();

var ollamaModule = pythonEnvironment.OllamaTest();


var response = await ollamaModule.Prompt("Hello!");

Console.WriteLine(pythonModule.SumTwoNumbers(1, 2));

Console.WriteLine(response);
