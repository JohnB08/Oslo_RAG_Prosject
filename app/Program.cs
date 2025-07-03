// See https://aka.ms/new-console-template for more information
using app.Models.Embeddings;
using app.VectorDbContext;
using CSnakes.Runtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pgvector;
using Pgvector.EntityFrameworkCore;

Console.WriteLine("Hello, World!");
var builder = Host.CreateApplicationBuilder();
var pythonHome = Path.Join(Environment.CurrentDirectory, "./PythonFiles");

builder.Services.WithPython().WithHome(pythonHome).WithPipInstaller().WithVirtualEnvironment(Path.Join(pythonHome, ".venv")).FromRedistributable();
builder.Services.AddDbContext<VectorDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), opt => opt.UseVector()));

var app = builder.Build();

var pythonEnvironment = app.Services.GetRequiredService<IPythonEnvironment>();

var sTModule = pythonEnvironment.SentenceTransformerModule();
var sentences = await File.ReadAllLinesAsync("../sentences.txt");
var results = sTModule.EmbeddText(sentences);

using var context = app.Services.GetRequiredService<VectorDbContext>();
if (!context.Embeddings.Any())
{
    var embeddings = results.Select((emb, i) => new Embeddings
    {
        Text = sentences[i],
        Embedding = new Pgvector.Vector(emb.Select(num => (float)num).ToArray())
    });
    await context.AddRangeAsync(embeddings);
    await context.SaveChangesAsync();
}

while (true)
{
    Console.WriteLine("Write a prompt to the database....:");

    var prompt = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(prompt)) continue;
    if (prompt == "exit" || prompt == "q" || prompt == "quit") break;

    var promptemb = sTModule.EmbeddPrompt(prompt);
    var promptVector = new Vector(promptemb.Select(num => (float)num).ToArray());
    var queryResults = await context.Embeddings.Where(text => text.Embedding!.L2Distance(promptVector) < 1).OrderBy(text => text.Embedding!.L2Distance(promptVector) < 1).Take(5).ToListAsync();
    foreach (var result in queryResults) Console.WriteLine(result.Text);
}



