// See https://aka.ms/new-console-template for more information
using CSnakes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder();
var pythonHome = Path.Join(Environment.CurrentDirectory, "./PythonFiles");

builder.Services.WithPython().WithHome(pythonHome).WithPipInstaller().WithVirtualEnvironment(Path.Join(pythonHome, ".venv")).FromRedistributable();

var app = builder.Build();

var pythonEnvironment = app.Services.GetRequiredService<IPythonEnvironment>();


var chromaDbModule = pythonEnvironment.ChromadbTest();

var chromaTest = chromaDbModule.CreateAndFetchFromChromadb("which document contains information about fruits?");

Console.WriteLine(chromaTest);
