from sentence_transformers import SentenceTransformer

def embedd_text(texts: list[str]) -> list[list[float]]:
    model = SentenceTransformer('all-MiniLM-L6-v2')
    results = []
    for sentence in texts:
        emb = model.encode(sentence)
        results.append(emb.tolist())
    return results

def embedd_prompt(text: str) -> list[float]:
    model = SentenceTransformer('all-MiniLM-L6-v2')
    emb = model.encode(text)
    return emb.tolist()