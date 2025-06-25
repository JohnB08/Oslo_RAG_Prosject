import ollama
import asyncio

async def prompt(prmt: str) -> str:
    client = ollama.AsyncClient(host = 'http://localhost:11434')
    response = client.generate(
        model = 'deepseek-r1:1.5b',
        prompt = prmt
    )
    return response['response']